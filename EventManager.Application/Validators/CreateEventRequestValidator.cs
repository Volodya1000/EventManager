using FluentValidation;

using EventManager.Application.Requests;

namespace EventManager.Application.Validators;

public class CreateEventRequestValidator : AbstractValidator<CreateEventRequest>
{
    public CreateEventRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty();

        RuleFor(r => r.Description)
            .NotEmpty();

        RuleFor(r => r.DateTime)
            .GreaterThan(DateTime.Now);

        RuleFor(r => r.Location)
            .NotEmpty();

        RuleFor(r => r.Category)
            .NotEmpty();

        RuleFor(r => r.MaxParticipants).GreaterThan(0);

        //список url либо пустой, либо все url валидны
        RuleFor(r => r.ImageUrls)
           .Must(list => list == null || list.All(url => Uri.TryCreate(url, UriKind.Absolute, out _)))
           .WithMessage("All url should be valid");
    }
}
