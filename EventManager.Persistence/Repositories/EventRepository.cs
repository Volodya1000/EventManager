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


    public async Task AddAsync(Event newEvent)
    {  
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == newEvent.Category);
        if (category == null)
            throw new InvalidOperationException("Category not found");

        var entity = new EventEntity
        {
            Id = Guid.NewGuid(),
            Name = newEvent.Name,
            Description = newEvent.Description,
            DateTime = newEvent.DateTime,
            Location = newEvent.Location,
            CategoryId = category.Id,
            MaxParticipants = newEvent.MaxParticipants,
            Images = newEvent.ImageUrls?.Select(url => new ImageEntity { Id = Guid.NewGuid(), Url = url })
                .ToList() ?? new List<ImageEntity>()
        };

        await _context.Events.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(Event updatedEvent)
    {
        try
        {
            var entity = await _context.Events
            .Include(e => e.Participants)
            .FirstOrDefaultAsync(e => e.Id == updatedEvent.Id);

            if (entity == null)
                throw new InvalidOperationException("Event not found");

            if (!await _context.Categories.AnyAsync(c => c.Name == updatedEvent.Category))
                throw new InvalidOperationException("Category not found");

            entity.Name = updatedEvent.Name ?? entity.Name;
            entity.Description = updatedEvent.Description;
            entity.DateTime = updatedEvent.DateTime;
            entity.Location = updatedEvent.Location;

            // Обновление категории по имени
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == updatedEvent.Category);
            if (category != null)
                entity.CategoryId = category.Id;

            entity.MaxParticipants = updatedEvent.MaxParticipants;

            // Обновление участников
            var existingParticipantIds = entity.Participants.Select(p => p.UserId).ToList();
            var newParticipantIds = updatedEvent.Participants.Select(p => p.UserId) ?? new List<Guid>();

            // Удаление лишних участников
            foreach (var participantId in existingParticipantIds.Except(newParticipantIds))
            {
                var participantToRemove = entity.Participants.FirstOrDefault(p => p.UserId == participantId);
                if (participantToRemove != null)
                    entity.Participants.Remove(participantToRemove);
            }

            // Добавление новых участников
            foreach (var participantId in newParticipantIds.Except(existingParticipantIds))
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == participantId);
                if (user != null)
                {
                    var newParticipant = new ParticipantEntity
                    {
                        UserId = participantId,
                        EventId = entity.Id,
                        RegistrationDate = DateTime.UtcNow,
                        User = user
                    };
                    entity.Participants.Add(newParticipant);
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception)
        {
            return false;
        }

        return true;
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

    public async Task<PagedResponse<EventDto>> GetByFilterAsync(
     EventFilterRequest filter,
     int pageNumber,
     int pageSize)
    {
        var query = _context.Events
            .AsNoTracking()
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
            .ProjectTo<EventDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResponse<EventDto>(data, pageNumber, pageSize, totalRecords);
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
