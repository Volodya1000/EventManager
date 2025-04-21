using EventManager.Domain.Models;
using EventManager.Application.Requests;
using EventManager.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using EventManager.Application.Interfaces.Repositories;
using AutoMapper;

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

    #region OperationsWithEvents
    public async Task<PagedResponse<Event>> GetAllAsync(int pageNumber, int pageSize)
    {
        var query = _context.Events
            .AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.Images)
            .Include(e => e.Participants)
                .ThenInclude(p => p.User)
            .OrderBy(e => e.DateTime);

        var totalRecords = await query.CountAsync();
        var entities = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var events = _mapper.Map<List<Event>>(entities);
        return new PagedResponse<Event>(events, pageNumber, pageSize, totalRecords);
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

        var entity = new EventEntity
        {
            Id = newEvent.Id,
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

    public async Task UpdateAsync(Event updatedEvent)
    {
        var entity = await _context.Events
            .Include(e => e.Participants)
            .FirstOrDefaultAsync(e => e.Id == updatedEvent.Id);

        // Обновляем свойства
        entity.Description = updatedEvent.Description;
        entity.DateTime = updatedEvent.DateTime;
        entity.Location = updatedEvent.Location;
        entity.MaxParticipants = updatedEvent.MaxParticipants;

        // Обновляем участников
        entity.Participants = updatedEvent.Participants?
            .Select(p => new ParticipantEntity
            {
                UserId = p.UserId,
                EventId = entity.Id,
                RegistrationDate = p.RegistrationDate
            })
            .ToList() ?? new List<ParticipantEntity>();

        await _context.SaveChangesAsync();
    }



    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Events.FindAsync(id);

        _context.Events.Remove(entity);
        var result = await _context.SaveChangesAsync();
    }


    public async Task<PagedResponse<Event>> GetFilteredAsync(
       EventFilterRequest filter,
       int pageNumber,
       int pageSize)
    {
        var query = _context.Events
            .AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.Images)
            .Include(e => e.Participants)
                .ThenInclude(p => p.User)
            .AsQueryable();

        if (filter.DateFrom.HasValue)
            query = query.Where(e => e.DateTime >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(e => e.DateTime <= filter.DateTo.Value);

        if (!string.IsNullOrEmpty(filter.Location))
            query = query.Where(e => e.Location.Contains(filter.Location));

        if (filter.Categories != null && filter.Categories.Any())
            query = query.Where(e => filter.Categories.Contains(e.Category.Name));

        if (filter.MaxParticipants.HasValue)
            query = query.Where(e => e.MaxParticipants <= filter.MaxParticipants.Value);

        if (filter.AvailableSpaces.HasValue)
            query = query.Where(e => e.MaxParticipants - e.Participants.Count >= filter.AvailableSpaces.Value);

        var totalRecords = await query.CountAsync();
        var entities = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var events = _mapper.Map<List<Event>>(entities);
        return new PagedResponse<Event>(events, pageNumber, pageSize, totalRecords);
    }

    public async Task<PagedResponse<Event>> GetEventsByUserAsync(Guid userId, int pageNumber, int pageSize)
    {
        var query = _context.Events
           .AsNoTracking()
           .Include(e => e.Category)
           .Include(e => e.Images)
           .Include(e => e.Participants)
               .ThenInclude(p => p.User)
           .Where(e => e.Participants.Any(p => p.UserId == userId))
           .OrderBy(e => e.DateTime);

        var totalRecords = await query.CountAsync();
        var entities = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var events = _mapper.Map<List<Event>>(entities);
        return new PagedResponse<Event>(events, pageNumber, pageSize, totalRecords);
    }

    public async Task<bool> AnyEventWithCategoryAsync(Guid categoryId)
    {
        return await _context.Events.
            AsNoTracking().AnyAsync(e => e.Category.Id== categoryId);
    }


    #endregion


    #region OperationsWithParticipants
    public async Task<PagedResponse<Participant>> GetParticipantsAsync(
        Guid eventId,
        int pageNumber = 1,
        int pageSize = 10)
    {
        var query = _context.Participants
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p => p.EventId == eventId);

        var totalCount = await query.CountAsync();

        var participants = await query
            .OrderBy(p => p.RegistrationDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var mappedParticipants = _mapper.Map<List<Participant>>(participants);

        return new PagedResponse<Participant>(
            mappedParticipants,
            pageNumber,
            pageSize,
            totalCount
        );
    }

    #endregion
}
