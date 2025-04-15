using AutoMapper;
using EventManager.Domain.Models;
using EventManager.Persistence.Entities;

namespace EventManager.Persistence.Mapping;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<CategoryEntity, Category>()
            .ConstructUsing(src => Category.Create(src.Id, src.Name));
    }
}