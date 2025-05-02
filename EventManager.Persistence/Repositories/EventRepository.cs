using AutoMapper;
using EventManager.Domain.Interfaces.Repositories;
using EventManager.Domain.Models;
using EventManager.Persistence.Entities;
using EventManager.Persistence;
using Microsoft.EntityFrameworkCore;

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
    public async Task<PagedResponse<Event>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cst = default)
    {
        var query = _context.Events
            .AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.Images)
            .Include(e => e.Participants)
                .ThenInclude(p => p.User)
            .OrderBy(e => e.DateTime);

        var totalRecords = await query.CountAsync(cst);
        var entities = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cst);

        var events = _mapper.Map<List<Event>>(entities);
        return new PagedResponse<Event>(events, pageNumber, pageSize, totalRecords);
    }

    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cst = default)
    {
        var entity = await _context.Events
            .AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.Images)
            .Include(e => e.Participants)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(e => e.Id == id, cst);

        return entity != null ? _mapper.Map<Event>(entity) : null;
    }

    public async Task<Event?> GetByNameAsync(string name, CancellationToken cst = default)
    {
        var entity = await _context.Events
            .AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.Images)
            .Include(e => e.Participants)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(e => e.Name == name, cst);

        return entity != null ? _mapper.Map<Event>(entity) : null;
    }

    public async Task AddAsync(Event newEvent, CancellationToken cst = default)
    {
        var entity = _mapper.Map<EventEntity>(newEvent);
        await _context.Events.AddAsync(entity, cst);
        await _context.SaveChangesAsync(cst);
    }

    public async Task UpdateAsync(Event updatedEvent, CancellationToken cst = default)
    {
        var entity = _mapper.Map<EventEntity>(updatedEvent);
        _context.Events.Update(entity);
        await _context.SaveChangesAsync(cst);
    }

    public async Task DeleteAsync(Event deleteEvent, CancellationToken cst = default)
    {
        var entity = _mapper.Map<EventEntity>(deleteEvent);
        _context.Events.Remove(entity);
        await _context.SaveChangesAsync(cst);
    }

    public async Task<PagedResponse<Event>> GetFilteredAsync(
      int pageNumber,
      int pageSize,
      DateTime? dateFrom = null,
      DateTime? dateTo = null,
      string? location = null,
      List<string>? categories = null,
      int? maxParticipants = null,
      int? availableSpaces = null,
      CancellationToken cst = default)
    {
        var query = _context.Events
            .AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.Images)
            .Include(e => e.Participants)
                .ThenInclude(p => p.User)
            .AsQueryable();

        if (dateFrom.HasValue)
            query = query.Where(e => e.DateTime >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(e => e.DateTime <= dateTo.Value);

        if (!string.IsNullOrEmpty(location))
            query = query.Where(e => e.Location.Contains(location));

        if (categories != null && categories.Count > 0)
            query = query.Where(e => categories.Contains(e.Category.Name));

        if (maxParticipants.HasValue)
            query = query.Where(e => e.MaxParticipants <= maxParticipants.Value);

        if (availableSpaces.HasValue)
            query = query.Where(e => e.MaxParticipants - e.Participants.Count >= availableSpaces.Value);

        var totalRecords = await query.CountAsync(cst);
        var entities = await query
            .OrderBy(e => e.DateTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cst);

        var events = _mapper.Map<List<Event>>(entities);
        return new PagedResponse<Event>(events, pageNumber, pageSize, totalRecords);
    }

    public async Task<PagedResponse<Event>> GetEventsByUserAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cst = default)
    {
        var query = _context.Events
           .AsNoTracking()
           .Include(e => e.Category)
           .Include(e => e.Images)
           .Include(e => e.Participants)
               .ThenInclude(p => p.User)
           .Where(e => e.Participants.Any(p => p.UserId == userId))
           .OrderBy(e => e.DateTime);

        var totalRecords = await query.CountAsync(cst);
        var entities = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cst);

        var events = _mapper.Map<List<Event>>(entities);
        return new PagedResponse<Event>(events, pageNumber, pageSize, totalRecords);
    }

    public async Task<bool> AnyEventWithCategoryAsync(Guid categoryId, CancellationToken cst = default)
    {
        return await _context.Events
            .AsNoTracking()
            .AnyAsync(e => e.Category.Id == categoryId, cst);
    }
    #endregion

    #region OperationsWithParticipants
    public async Task<PagedResponse<Participant>> GetParticipantsAsync(
        Guid eventId,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cst = default)
    {
        var query = _context.Participants
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p => p.EventId == eventId);

        var totalCount = await query.CountAsync(cst);

        var participants = await query
            .OrderBy(p => p.RegistrationDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cst);

        var mappedParticipants = _mapper.Map<List<Participant>>(participants);

        return new PagedResponse<Participant>(
            mappedParticipants,
            pageNumber,
            pageSize,
            totalCount
        );
    }


    public async Task AddAsync(Participant participant, CancellationToken cst = default)
    {
        var entity = _mapper.Map<ParticipantEntity>(participant);
        await _context.Participants.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(Participant participant, CancellationToken cst = default)
    {
        var entity = _mapper.Map<ParticipantEntity>(participant);
        _context.Participants.Remove(entity);
        await _context.SaveChangesAsync();
    }
    #endregion
}