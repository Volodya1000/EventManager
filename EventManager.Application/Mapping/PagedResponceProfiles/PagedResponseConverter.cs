using AutoMapper;
using EventManager.Domain.Models;

namespace EventManager.Application.Mapping.PagedResponceProfiles;

public class PagedResponseConverter<TSource, TDestination>
    : ITypeConverter<PagedResponse<TSource>, PagedResponse<TDestination>>
{
    public PagedResponse<TDestination> Convert(
        PagedResponse<TSource> source,
        PagedResponse<TDestination> destination,
        ResolutionContext context)
    {
        var items = context.Mapper.Map<List<TDestination>>(source.Data);

        return new PagedResponse<TDestination>(
            items,
            source.PageNumber,
            source.PageSize,
            source.TotalRecords
        );
    }
}