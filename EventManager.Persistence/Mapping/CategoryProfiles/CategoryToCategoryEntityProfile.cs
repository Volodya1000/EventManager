using AutoMapper;
using EventManager.Persistence.Entities;

namespace EventManager.Persistence.Mapping.Category;

public  class CategoryToCategoryEntityProfile:Profile
{
    public CategoryToCategoryEntityProfile()
    {
        CreateMap<EventManager.Domain.Models.Category, CategoryEntity>()
          .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
          .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
    }
}
