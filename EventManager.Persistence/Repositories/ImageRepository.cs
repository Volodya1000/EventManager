using AutoMapper;
using EventManager.Domain.Interfaces.Repositories;
using EventManager.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using EventManager.Domain.Models;

namespace EventManager.Persistence.Repositories;

public class ImageRepository : IImageRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ImageRepository(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task AddAsync(Image image, CancellationToken cst = default)
    {
        var entity = _mapper.Map<ImageEntity>(image);
        _context.Images.Add(entity);
        await _context.SaveChangesAsync(cst);
    }

    public async Task UpdateAsync(Image image, CancellationToken cst = default)
    {
        var entity = _mapper.Map<ImageEntity>(image);
        _context.Images.Update(entity);
        await _context.SaveChangesAsync(cst);
    }

    public async Task DeleteImageAsyncWithoutSaveChanges(Image image, CancellationToken cst = default)
    {
        var entity = _mapper.Map<ImageEntity>(image);
        _context.Images.Remove(entity);
    }

    public async Task<bool> ExistsAsync(Guid eventId, string url, CancellationToken cst = default)
    {
        return await _context.Images.AsNoTracking().AnyAsync(
            i => i.EventId == eventId && i.Url == url, cst);
    }

    public async Task<Image?> GetByEventIdAndUrlAsync(Guid eventId, string url, CancellationToken cst = default)
    {
        var entity = await _context.Images
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.EventId == eventId && i.Url == url, cst);

        return entity == null
            ? null
            : _mapper.Map<Image>(entity);
    }
}