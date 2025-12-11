using AutoMapper;

using Plant01.Upper.Application.Contracts.Api.Requests;
using Plant01.Upper.Application.Contracts.DTOs;
using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Domain.Aggregation;
using Plant01.Upper.Domain.Entities;
using Plant01.Upper.Domain.Repository;

namespace Plant01.Upper.Application.Services;

public class ProductionQueryService : IProductionQueryService
{
    private readonly IRepository<WorkOrder> _workOrderRepository;
    private readonly IRepository<Bag> _bagRepository;
    private readonly IRepository<Pallet> _palletRepository;
    private readonly IMapper _mapper;

    public ProductionQueryService(
        IRepository<WorkOrder> workOrderRepository,
        IRepository<Bag> bagRepository,
        IRepository<Pallet> palletRepository,
        IMapper mapper)
    {
        _workOrderRepository = workOrderRepository;
        _bagRepository = bagRepository;
        _palletRepository = palletRepository;
        _mapper = mapper;
    }

    public async Task<List<WorkOrderRequestDto>> GetRecentWorkOrdersAsync(int count = 10)
    {
        var entities = await _workOrderRepository.GetPagedAsync(1, count);
        return _mapper.Map<List<WorkOrderRequestDto>>(entities);
    }

    public async Task<List<BagDto>> GetRecentBagsAsync(int count = 50)
    {
        var entities = await _bagRepository.GetPagedAsync(1, count);
        return _mapper.Map<List<BagDto>>(entities);
    }

    public async Task<List<PalletDto>> GetRecentPalletsAsync(int count = 10)
    {
        var entities = await _palletRepository.GetPagedAsync(1, count);
        return _mapper.Map<List<PalletDto>>(entities);
    }
}
