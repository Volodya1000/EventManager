using AutoMapper;
using EventManager.Persistence.Entities;
using EventManager.Domain.Models;

namespace EventManager.Persistence.Mapping.ImageProfiles;

public class ImageToImageEntityProfile : Profile
{
    public ImageToImageEntityProfile()
    {
        CreateMap<Image, ImageEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.EventId, opt => opt.MapFrom(src => src.EventId))
            .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url));
    }
}
