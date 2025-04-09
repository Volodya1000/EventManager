﻿namespace EventManager.Persistence.Entities;

public class ParticipantEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid EventId { get; set; }
    public DateTime RegistrationDate { get; set; }

    public UserEntity User { get; set; }
    public EventEntity Event { get; set; }
}
