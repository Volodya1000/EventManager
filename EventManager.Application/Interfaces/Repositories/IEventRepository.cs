﻿using EventManager.Application.Dtos;
using EventManager.Domain.Models;

namespace EventManager.Application.Interfaces.Repositories;

public interface IEventRepository
{
    public Task<PagedResponse<EventDto>> GetAllAsync(int pageNumber, int pageSize);
}
