using AutoMapper;

using CommunityToolkit.Mvvm.Messaging;

using Plant01.Upper.Application.Contracts.Api.Requests;
using Plant01.Upper.Application.Contracts.Api.Responses;
using Plant01.Upper.Application.Contracts.DTOs;
using Plant01.Upper.Application.Contracts.IntegrationEvents;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Domain.Entities;
using Plant01.Upper.Domain.Repository;
using Plant01.Upper.Domain.ValueObjects;

namespace Plant01.Upper.Application.Services;

public class MesCommandService : IMesCommandService
{
    private readonly IMesWebApi _mesWebApi;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMapper _mapper;
    private readonly IWorkOrderRepository _workOrderRepository;

    public MesCommandService(IMesWebApi mesWebApi, IServiceScopeFactory scopeFactory,IMapper mapper,IWorkOrderRepository workOrderRepository)
    {
        _mesWebApi = mesWebApi;
        _scopeFactory = scopeFactory;
        _mapper = mapper;
        _workOrderRepository = workOrderRepository;

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

    private async Task<WorkOrderResponse> HandleWorkOrderReceived(WorkOrderRequestDto dto)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var workOrderRepo = unitOfWork.Repository<WorkOrder>();
            var existing = await workOrderRepo.GetByIdAsync(dto.Code);

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
                await workOrderRepo.UpdateAsync(workOrder);
            }

            await unitOfWork.SaveChangesAsync();

            WeakReferenceMessenger.Default.Send(new WorkOrderReceivedEvent(_mapper.Map<WorkOrderDto>(workOrder)));

            return new WorkOrderResponse { ErrorCode = 0, ErrorMsg = "Success" };
        }
        catch (Exception ex)
        {
            return new WorkOrderResponse { ErrorCode = 500, ErrorMsg = ex.Message };
        }
    }
}
