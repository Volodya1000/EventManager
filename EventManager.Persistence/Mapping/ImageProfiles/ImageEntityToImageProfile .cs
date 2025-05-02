using AutoMapper;
using EventManager.Domain.Models;
using EventManager.Persistence.Entities;

namespace EventManager.Persistence.Mapping.ImageProfiles;

public class ImageEntityToImageProfile : Profile
{
    public ImageEntityToImageProfile()
    {
        CreateMap<ImageEntity, Image>()
            .ConstructUsing(src =>
                  Image.Create(
                    src.Id,
                    src.EventId,
                    src.Url
                )
            );
    }
}