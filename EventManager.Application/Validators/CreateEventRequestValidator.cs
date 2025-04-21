using FluentValidation;
using EventManager.Application.Requests;
using EventManager.Domain.Models;

namespace EventManager.Application.Validators;

public class CreateEventRequestValidator : AbstractValidator<CreateEventRequest>
{
    public CreateEventRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty()
            .WithMessage("Event name is required");

        RuleFor(r => r.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .Length(Event.MIN_DESCRIPTION_LENGTH, Event.MAX_DESCRIPTION_LENGTH)
            .WithMessage($"Description must be between {Event.MIN_DESCRIPTION_LENGTH} and {Event.MAX_DESCRIPTION_LENGTH} characters");

        RuleFor(r => r.DateTime)
            .GreaterThan(DateTime.Now)
            .WithMessage("Event date must be in the future");

        RuleFor(r => r.Location)
            .NotEmpty()
            .WithMessage("Location is required")
            .Length(Event.MIN_LOCATION_LENGTH, Event.MAX_LOCATION_LENGTH)
            .WithMessage($"Location must be between {Event.MIN_LOCATION_LENGTH} and {Event.MAX_LOCATION_LENGTH} characters");

        RuleFor(r => r.Category)
            .NotEmpty()
            .WithMessage("Category is required");

        RuleFor(r => r.MaxParticipants)
            .GreaterThan(0)
            .WithMessage("Max participants must be positive");
    }
}
