using AutoMapper;

using Plant01.Upper.Application.Contracts.DTOs;
using Plant01.Upper.Domain.Aggregation;
using Plant01.Upper.Domain.Entities;

namespace Plant01.Upper.Application.Mappings;

public class ProductionMappingProfile : Profile
{
    public ProductionMappingProfile()
    {
        CreateMap<WorkOrder, WorkOrderDto>();
        
        CreateMap<BagProcessRecord, BagProcessRecordDto>();
        
        CreateMap<Bag, BagDto>()
            .ForMember(dest => dest.CurrentStatus, opt => opt.MapFrom(src => GetCurrentStatus(src)));
            
        CreateMap<Pallet, PalletDto>()
            .ForMember(dest => dest.BagCount, opt => opt.MapFrom(src => src.Items.Count));
    }

    private string GetCurrentStatus(Bag bag)
    {
        if (bag.Records == null || !bag.Records.Any()) return "Created";
        var lastRecord = bag.Records.OrderByDescending(r => r.OccurredTime).FirstOrDefault();
        return lastRecord != null ? $"{lastRecord.Step} ({(lastRecord.IsSuccess ? "OK" : "Fail")})" : "Unknown";
    }
}
