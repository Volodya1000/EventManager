using EventManager.Domain.Models;
using EventManager.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace EventManager.Persistence.Repositories;

public class EventRepository //: IEventRepository
{
    private readonly ApplicationDbContext _context;

    public EventRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<EventEntity>> GetAllAsync(int pageNumber, int pageSize)
    {
        var query = _context.Events
            .AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.Participants);

        var totalRecords = await query.CountAsync();
        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<EventEntity>(data, pageNumber, pageSize, totalRecords);
    }

    public async Task<EventEntity?> GetByIdAsync(Guid id)
    {
        return await _context.Events
            .AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.Participants)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<EventEntity?> GetByNameAsync(string name)
    {
        return await _context.Events
            .AsNoTracking()
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Name == name);
    }

    public async Task<EventEntity> AddAsync(
        string name,
        string description,
        DateTime dateTime,
        string location,
        Guid categoryId,
        int maxParticipants,
        List<string>? images = null)
    {
        var existingImageUrls = await _context.Images.Select(i => i.Url).ToListAsync();
        var imageEntities = images?.Select(url => existingImageUrls.Contains(url)
            ? _context.Images.FirstOrDefault(i => i.Url == url)
            : new ImageEntity { Id = Guid.NewGuid(), Url = url })
            .ToList() ?? new List<ImageEntity>();

        var entity = new EventEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            DateTime = dateTime,
            Location = location,
            CategoryId = categoryId,
            MaxParticipants = maxParticipants,
            ImageUrls = imageEntities
        };

        await _context.Events.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<EventEntity> UpdateAsync(
        Guid eventId,
        string? name,
        string? description,
        DateTime? dateTime,
        string? location,
        Guid? categoryId,
        int? maxParticipants)
    {
        var entity = await _context.Events.FindAsync(eventId);
        if (entity == null)
            throw new ArgumentException("Event not found");

        entity.Name = name ?? entity.Name;
        entity.Description = description ?? entity.Description;
        entity.DateTime = dateTime ?? entity.DateTime;
        entity.Location = location ?? entity.Location;
        entity.CategoryId = categoryId ?? entity.CategoryId;
        entity.MaxParticipants = maxParticipants ?? entity.MaxParticipants;

        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Events.FindAsync(id);
        if (entity != null)
        {
            _context.Events.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<PagedResponse<EventEntity>> GetByFilterAsync(
        EventFilter filter,
        int pageNumber,
        int pageSize)
    {
        var query = _context.Events
            .AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.Participants)
            .AsQueryable();

        if (filter.DateFrom.HasValue)
            query = query.Where(e => e.DateTime >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(e => e.DateTime <= filter.DateTo.Value);

        if (!string.IsNullOrEmpty(filter.Location))
            query = query.Where(e => e.Location.Contains(filter.Location));

        if (filter.CategoryIds != null && filter.CategoryIds.Any())
            query = query.Where(e => filter.CategoryIds.Contains(e.CategoryId));

        if (filter.MaxParticipants.HasValue)
            query = query.Where(e => e.MaxParticipants <= filter.MaxParticipants.Value);

        if (filter.AvailableSpaces.HasValue)
            query = query.Where(e => e.MaxParticipants - e.Participants.Count >= filter.AvailableSpaces.Value);

        var totalRecords = await query.CountAsync();
        var data = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<EventEntity>(data, pageNumber, pageSize, totalRecords);
    }

    public async Task AddImageToEventAsync(Guid eventId, string imageUrl)
    {
        var entity = await _context.Events
            .Include(e => e.Images)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (entity == null)
            throw new ArgumentException("Event not found");

        if (!entity.Images.Contains(imageUrl))
        {
            entity.Images.Add(imageUrl);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveImageFromEventAsync(Guid eventId, string imageUrl)
    {
        var entity = await _context.Events
            .Include(e => e.Images)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (entity == null)
            throw new ArgumentException("Event not found");

        if (entity.Images.Remove(imageUrl))
        {
            await _context.SaveChangesAsync();
        }
    }
}
