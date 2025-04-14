using EventManager.Application.Requests;
using FluentValidation;

namespace EventManager.Application.Validators;

public class UpdateEventRequestValidator : AbstractValidator<UpdateEventRequest>
{
    public UpdateEventRequestValidator()
    {
        RuleFor(request => request.DateTime)
            .Must(date => date > DateTime.Now)
            .WithMessage("The event date must be later than the current time.");

        RuleFor(request => request.MaxParticipants)
            .GreaterThan(0)
            .WithMessage("The maximum number of participants must be greater than zero.");
    }
}
