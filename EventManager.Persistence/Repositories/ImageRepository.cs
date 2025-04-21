using EventManager.Domain.Interfaces.Repositories;
using EventManager.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Persistence.Repositories;

public class ImageRepository:IImageRepository
{
    private readonly ApplicationDbContext _context;

    public ImageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddImageToEventAsync(
        Guid eventId,
        string imageUrl,
        CancellationToken cst = default)
    {
        var entity = await _context.Events
            .Include(e => e.Images)
            .FirstOrDefaultAsync(e => e.Id == eventId, cst);

        var newImage = new ImageEntity { Url = imageUrl };
        entity.Images.Add(newImage);
        await _context.SaveChangesAsync(cst);
    }

    public async Task DeleteImageAsyncWithoutSaveChanges(
        Guid eventId,
        string imageUrl,
        CancellationToken cst = default)
    {
        var entity = await _context.Events
            .Include(e => e.Images)
            .FirstOrDefaultAsync(e => e.Id == eventId, cst);

        var imageToRemove = entity.Images.FirstOrDefault(i => i.Url == imageUrl);
        entity.Images.Remove(imageToRemove);
    }

    public async Task<bool> ExistsAsync(
        Guid eventId,
        string url,
        CancellationToken cst = default)
    {
        return await _context.Events.AsNoTracking()
            .AnyAsync(e => e.Id == eventId
                && e.Images.Any(i => i.Url == url), cst);
    }
}
