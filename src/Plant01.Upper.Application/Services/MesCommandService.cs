using Microsoft.Extensions.DependencyInjection;
using Plant01.Upper.Application.DTOs;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Domain.Entities;
using Plant01.Upper.Domain.Services;
using Plant01.Upper.Domain.ValueObjects;

namespace Plant01.Upper.Application.Services;

public class MesCommandService : IMesCommandService
{
    private readonly IMesWebApi _mesWebApi;
    private readonly IServiceScopeFactory _scopeFactory;

    public MesCommandService(IMesWebApi mesWebApi, IServiceScopeFactory scopeFactory)
    {
        _mesWebApi = mesWebApi;
        _scopeFactory = scopeFactory;
        
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
            var repo = unitOfWork.Repository<WorkOrder>();
            var existing = await repo.GetByIdAsync(dto.Code);

            var workOrder = existing ?? new WorkOrder();
            
            // Map properties
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
                await repo.AddAsync(workOrder);
            }
            else
            {
                await repo.UpdateAsync(workOrder);
            }
            
            await unitOfWork.SaveChangesAsync();

            return new WorkOrderResponse { ErrorCode = 0, ErrorMsg = "Success" };
        }
        catch (Exception ex)
        {
            return new WorkOrderResponse { ErrorCode = 500, ErrorMsg = ex.Message };
        }
    }
}
