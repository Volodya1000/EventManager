using EventManager.Application.Requests;
using EventManager.Domain.Models;
using FluentValidation;

namespace EventManager.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .Length(User.MIN_NAME_LENGTH, User.MAX_NAME_LENGTH)
            .WithMessage($"First name must be between {User.MIN_NAME_LENGTH} and {User.MAX_NAME_LENGTH} characters")
            .Matches(@"^[\p{L} \-']+$")
            .WithMessage("First name contains invalid characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .Length(User.MIN_NAME_LENGTH, User.MAX_NAME_LENGTH)
            .WithMessage($"Last name must be between {User.MIN_NAME_LENGTH} and {User.MAX_NAME_LENGTH} characters")
            .Matches(@"^[\p{L} \-']+$")
            .WithMessage("Last name contains invalid characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Valid email address is required")
            .MaximumLength(256)
            .WithMessage("Email too long");
       

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .WithMessage("Date of birth is required")
            .Must(dob => dob > DateTime.UtcNow.AddYears(-120))
            .WithMessage("Invalid date of birth");
    }
}