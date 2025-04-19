using EventManager.Application.Interfaces.Repositories;
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
    public async Task AddImageToEventAsync(Guid eventId, string imageUrl)
    {
        var entity = await _context.Events
            .Include(e => e.Images)
            .FirstOrDefaultAsync(e => e.Id == eventId);
      
        var newImage = new ImageEntity { Url = imageUrl };
        entity.Images.Add(newImage);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteImageAsyncWithoutSaveChanges(Guid eventId, string imageUrl)
    {
        var entity = await _context.Events
            .Include(e => e.Images)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        var imageToRemove = entity.Images.FirstOrDefault(i => i.Url == imageUrl);
        if (imageToRemove != null)
            entity.Images.Remove(imageToRemove);
    }

    public async Task<bool> ExistsAsync(Guid eventId, string url)
    {
        return await _context.Events.AsNoTracking()
            .AnyAsync(e => e.Id == eventId
                && e.Images.Any(i => i.Url == url));
    }
}
