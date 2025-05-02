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

    public async Task DeleteAsync(Image image, CancellationToken cst = default)
    {
        var entity = _mapper.Map<ImageEntity>(image);
        _context.Images.Remove(entity);
        await _context.SaveChangesAsync(cst);
    }
}