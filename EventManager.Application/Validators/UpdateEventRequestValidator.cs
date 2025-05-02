using EventManager.Application.Requests;
using EventManager.Domain.Models;
using FluentValidation;

namespace EventManager.Application.Validators;

public class UpdateEventRequestValidator : AbstractValidator<UpdateEventRequest>
{
    public UpdateEventRequestValidator()
    {
        RuleFor(request => request.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .Length(Event.MIN_DESCRIPTION_LENGTH, Event.MAX_DESCRIPTION_LENGTH)
            .WithMessage($"Description must be between {Event.MIN_DESCRIPTION_LENGTH} and {Event.MAX_DESCRIPTION_LENGTH} characters")
            .When(request => request.Description != null);

        RuleFor(request => request.DateTime)
            .Must(date => date > DateTime.UtcNow)
            .WithMessage($"Event date must be at least {DateTime.UtcNow.AddMinutes(1):HH:mm} (current time + 1 minute)")
            .When(request => request.DateTime != default);

        RuleFor(request => request.Location)
            .NotEmpty()
            .WithMessage("Location is required")
            .Length(Event.MIN_LOCATION_LENGTH, Event.MAX_LOCATION_LENGTH)
            .WithMessage($"Location must be between {Event.MIN_LOCATION_LENGTH} and {Event.MAX_LOCATION_LENGTH} characters")
            .When(request => request.Location != null);

        RuleFor(request => request.MaxParticipants)
            .GreaterThan(0)
            .WithMessage("Max participants must be greater than 0");
    }
}
