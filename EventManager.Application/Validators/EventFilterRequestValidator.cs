using EventManager.Application.Requests;
using FluentValidation;

namespace EventManager.Application.Validators;

public class EventFilterRequestValidator : AbstractValidator<EventFilterRequest>
{
    public EventFilterRequestValidator()
    {
        // Проверка, что DateFrom < DateTo, если оба указаны
        RuleFor(x => x)
            .Must(x => x.DateFrom < x.DateTo)
            .When(x => x.DateFrom.HasValue && x.DateTo.HasValue)
            .WithMessage("DateFrom must be before DateTo.");
    }
}
