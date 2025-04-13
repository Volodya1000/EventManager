using EventManager.Domain.Models;
using EventManager.Application.Requests;
using EventManager.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using EventManager.Application.Interfaces.Repositories;
using EventManager.Application.Dtos;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace EventManager.Persistence.Repositories;

public class EventRepository : IEventRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public EventRepository(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResponse<EventDto>> GetAllAsync(int pageNumber, int pageSize)
    {
        var query = _context.Events
            .AsNoTracking()
            .ProjectTo<EventDto>(_mapper.ConfigurationProvider); //проекция для оптимального sql запроса

        var totalRecords = await query.CountAsync();
        var data = await query
            .OrderBy(e => e.DateTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<EventDto>(data, pageNumber, pageSize, totalRecords);
    }

    public async Task<Event?> GetByIdAsync(Guid id)
    {
        var entity = await _context.Events
            .AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.Images)
            .Include(e => e.Participants)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(e => e.Id == id);

        return entity != null ? _mapper.Map<Event>(entity) : null;
    }

    public async Task<Event?> GetByNameAsync(string name)
    {
        var entity = await _context.Events
            .AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.Images)
            .Include(e => e.Participants)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(e => e.Name == name);

        return entity != null ? _mapper.Map<Event>(entity) : null;
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
        var entity = new EventEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            DateTime = dateTime,
            Location = location,
            CategoryId = categoryId,
            MaxParticipants = maxParticipants,
            Images = images?.Select(url
            => new ImageEntity { Id = Guid.NewGuid(), Url = url })
            .ToList() ?? new List<ImageEntity>()
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
            throw new InvalidOperationException("Event not found");

        if (categoryId.HasValue && !await _context.Categories.AnyAsync(c => c.Id == categoryId))
            throw new InvalidOperationException("Category not found");

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
        EventFilterRequest filter,
        int pageNumber,
        int pageSize)
    {
        var query = _context.Events
            .AsNoTracking()
             .Include(e => e.Category)
             .Include(e => e.Images)
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
            query = query.Where(e => e.MaxParticipants - e.Participants.Count() >= filter.AvailableSpaces.Value);

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
            throw new InvalidOperationException("Event not found");
        var newImage = new ImageEntity { Id = Guid.NewGuid(), Url = imageUrl };
        entity.Images.Add(newImage);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveImageFromEventAsync(Guid eventId, string imageUrl)
    {
        var entity = await _context.Events
            .Include(e => e.Images)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (entity == null)
            throw new InvalidOperationException("Event not found");

        var imageToRemove = entity.Images.FirstOrDefault(i => i.Url == imageUrl);
        if (imageToRemove != null)
        {
            entity.Images.Remove(imageToRemove);
            await _context.SaveChangesAsync();
        }
    }
}
