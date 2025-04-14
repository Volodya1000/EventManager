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

    #region OperationsWithEvents
    public async Task<PagedResponse<EventDto>> GetAllAsync(int pageNumber, int pageSize)
    {
        var query = _context.Events
            .AsNoTracking()
            .Include(e => e.Category)
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

        // Проверка уникальности имени события
        if (await _context.Events.AnyAsync(e => e.Name == newEvent.Name))
            throw new InvalidOperationException("Event with this name already exists");

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

        if (entity == null)
            throw new InvalidOperationException("Событие не найдено");

        // Проверяем, существует ли категория
        var categoryExists = await _context.Categories.AnyAsync(c => c.Name == updatedEvent.Category);
        if (!categoryExists)
            throw new InvalidOperationException("Категория не найдена");

        // Обновляем свойства
        entity.Description = updatedEvent.Description;
        entity.DateTime = updatedEvent.DateTime;
        entity.Location = updatedEvent.Location;
        entity.MaxParticipants = updatedEvent.MaxParticipants;

        // Обновляем категорию, если необходимо
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == updatedEvent.Category);
        if (category != null)
            entity.CategoryId = category.Id;

        // Обновляем участников
        var existingParticipantIds = entity.Participants.Select(p => p.UserId).ToList();
        var newParticipantIds = updatedEvent.Participants?.Select(p => p.UserId) ?? Enumerable.Empty<Guid>();

        // Удаляем участников, которых нет в новом списке
        foreach (var participantId in existingParticipantIds.Except(newParticipantIds))
        {
            var participantToRemove = entity.Participants.FirstOrDefault(p => p.UserId == participantId);
            if (participantToRemove != null)
                entity.Participants.Remove(participantToRemove);
        }

        // Добавляем новых участников
        foreach (var participantId in newParticipantIds.Except(existingParticipantIds))
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == participantId);
            if (userExists)
            {
                entity.Participants.Add(new ParticipantEntity
                {
                    UserId = participantId,
                    EventId = entity.Id,
                    RegistrationDate = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
    }



    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Events.FindAsync(id);

        if (entity == null)
            throw new InvalidOperationException("Event not found");

        _context.Events.Remove(entity);
        var result = await _context.SaveChangesAsync();
    }


    public async Task<PagedResponse<EventDto>> GetFilteredAsync(
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

        if (filter.Categories != null && filter.Categories.Any())
            query = query.Where(e => filter.Categories.Contains(e.Category.Name));

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


    #endregion




    #region OperationsWithImages

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

    public async Task DeleteImageAsyncWithoutCommit(Guid eventId, string imageUrl)
    {
        var entity = await _context.Events
            .Include(e => e.Images)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (entity == null)
            throw new InvalidOperationException("Event not found");

        var imageToRemove = entity.Images.FirstOrDefault(i => i.Url == imageUrl);
        if (imageToRemove != null)
            entity.Images.Remove(imageToRemove);
    }


    #endregion



    #region OperationsWithParticipants
    public async Task<PagedResponse<ParticipantDto>> GetParticipantsAsync(
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

        var participantDtos = _mapper.Map<List<ParticipantDto>>(participants);

        return new PagedResponse<ParticipantDto>(
            participantDtos, 
            pageNumber, 
            pageSize, 
            totalCount
        );
    }

    #endregion

    #region OperationsWithCategories
    
    #endregion
}
