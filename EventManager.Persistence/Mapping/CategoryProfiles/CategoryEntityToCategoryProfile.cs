using AutoMapper;
using EventManager.Domain.Models;
using EventManager.Persistence.Entities;

namespace EventManager.Persistence.Mapping.CategoryProfiles;

public class CategoryEntityToCategoryProfile : Profile
{
    public CategoryEntityToCategoryProfile()
    {
        CreateMap<CategoryEntity, EventManager.Domain.Models.Category>()
            .ConstructUsing(src => EventManager.Domain.Models.Category.Create(src.Id, src.Name));
    }
}