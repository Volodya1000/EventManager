﻿namespace EventManager.Persistence.Entities;

public class CategoryEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}