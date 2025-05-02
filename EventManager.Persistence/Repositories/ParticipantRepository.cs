using AutoMapper;
using EventManager.Domain.Interfaces.Repositories;
using EventManager.Domain.Models;
using EventManager.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Persistence.Repositories;

public class ParticipantRepository : IParticipantRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ParticipantRepository(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

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
        await _context.Participants.AddAsync(entity, cst);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(Participant participant, CancellationToken cst = default)
    {
        var entity = _mapper.Map<ParticipantEntity>(participant);
        _context.Participants.Remove(entity);
        await _context.SaveChangesAsync();
    }
}
