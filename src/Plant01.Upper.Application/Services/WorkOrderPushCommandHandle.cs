using AutoMapper;

using CommunityToolkit.Mvvm.Messaging;

using Plant01.Upper.Application.Contracts.Api.Requests;
using Plant01.Upper.Application.Contracts.Api.Responses;
using Plant01.Upper.Application.Contracts.IntegrationEvents;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Domain.Entities;
using Plant01.Upper.Domain.Repository;
using Plant01.Upper.Domain.ValueObjects;

namespace Plant01.Upper.Application.Services;

public class WorkOrderPushCommandHandle : IWorkOrderPushCommandHandle
{
    private readonly IMesWebApi _mesWebApi;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMapper _mapper;
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly ILogger<WorkOrderPushCommandHandle> _logger;

    public WorkOrderPushCommandHandle(
        IMesWebApi mesWebApi,
        IServiceScopeFactory scopeFactory,
        IMapper mapper,
        IWorkOrderRepository workOrderRepository,
        ILogger<WorkOrderPushCommandHandle> logger)
    {
        _mesWebApi = mesWebApi;
        _scopeFactory = scopeFactory;
        _mapper = mapper;
        _workOrderRepository = workOrderRepository;
        _logger = logger;

        _mesWebApi.OnWorkOrderReceived += HandleWorkOrderReceived;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (!_mesWebApi.IsRunning)
        {
            await _mesWebApi.StartAsync(cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _mesWebApi.StopAsync(cancellationToken);
    }

    private async Task<WorkOrderResponseDto> HandleWorkOrderReceived(WorkOrderRequestDto dto)
    {
        try
        {
            #region uow
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var workOrderRepo = unitOfWork.Repository<WorkOrder>();

            var existing = (await workOrderRepo.GetAllAsync(w => w.Code == dto.Code)).FirstOrDefault();

            var workOrder = existing ?? new WorkOrder();

            // 保存数据库
            workOrder.Code = dto.Code;
            workOrder.OrderDate = DateOnly.FromDateTime(dto.OrderDate);
            workOrder.LineNo = dto.LineNo;
            workOrder.ProductCode = dto.ProductCode;
            workOrder.ProductName = dto.ProductName;
            workOrder.ProductSpec = dto.ProductSpec;
            workOrder.Quantity = (int)dto.Quantity;
            workOrder.Unit = dto.Unit;
            workOrder.BatchNumber = dto.BatchNumber;
            workOrder.LabelTemplateCode = dto.LabelTemplateCode;
            workOrder.Status = WorkOrderStatus.开工; // Default to Started

            if (dto.OrderData != null)
            {
                workOrder.Items = dto.OrderData.Select(i => new WorkOrderItemProperty
                {
                    Key = i.Key,
                    Value = i.Value,
                    Name = i.Name
                }).ToList();
            }

            if (existing == null)
            {
                await workOrderRepo.AddAsync(workOrder);
            }
            else
            {
                workOrder.UpdatedAt = DateTime.UtcNow;
                await workOrderRepo.UpdateAsync(workOrder);
            }

            await unitOfWork.SaveChangesAsync();

            #endregion

            #region Repository
            //var workOrders = await _workOrderRepository.GetAllAsync(w => w.Code == dto.Code);
            //var workOrder = workOrders.FirstOrDefault();

            //workOrder ??= new WorkOrder();
            //workOrder.Code = dto.Code;
            //workOrder.OrderDate = DateOnly.FromDateTime(dto.OrderDate);
            //workOrder.LineNo = dto.LineNo;
            //workOrder.ProductCode = dto.ProductCode;
            //workOrder.ProductName = dto.ProductName;
            //workOrder.ProductSpec = dto.ProductSpec;
            //workOrder.Quantity = (int)dto.Quantity;
            //workOrder.Unit = dto.Unit;
            //workOrder.BatchNumber = dto.BatchNumber;
            //workOrder.LabelTemplateCode = dto.LabelTemplateCode;
            //workOrder.Status = WorkOrderStatus.开工;
            //if (dto.OrderData != null)
            //{
            //    var newItems = dto.OrderData.Select(i => new WorkOrderItemProperty
            //    {
            //        Key = i.Key,
            //        Value = i.Value,
            //        Name = i.Name
            //    }).ToList();
            //    if (workOrders.Count == 0)
            //    {
            //        workOrder.Items = newItems;
            //    }
            //    else
            //    {
            //        workOrder.Items.Clear();
            //        workOrder.Items.AddRange(newItems);
            //    }
            //}
            //else
            //{
            //    if (workOrders.Count == 0)
            //    {
            //        workOrder.Items = new List<WorkOrderItemProperty>();
            //    }
            //    else
            //    {
            //        workOrder.Items.Clear();
            //    }
            //}

            //if(workOrders.Count == 0)
            //{
            //    // 新增工单（暂时没有对旧工单做关闭处理）
            //    await _workOrderRepository.AddAsync(workOrder);
            //}
            //else
            //{
            //    workOrder.UpdatedAt = DateTime.UtcNow;
            //    await _workOrderRepository.UpdateAsync(workOrder);
            //}
            #endregion

            WeakReferenceMessenger.Default.Send(new WorkOrderReceivedEvent(_mapper.Map<WorkOrderRequestDto>(workOrder)));

            return new WorkOrderResponseDto { ErrorCode = 0, ErrorMsg = "Success" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理工单接收时出错：{Message}", ex.Message);
            return new WorkOrderResponseDto { ErrorCode = 500, ErrorMsg = ex.Message };
        }
    }
}
