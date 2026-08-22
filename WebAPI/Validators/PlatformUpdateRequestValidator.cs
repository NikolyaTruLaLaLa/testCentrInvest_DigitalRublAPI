using FluentValidation;
using WebAPI.Contracts;
namespace WebAPI.Validators
{
    public class PlatformUpdateRequestValidator : AbstractValidator<PlatformUpdateRequest>
    {
        public PlatformUpdateRequestValidator()
        {
            RuleFor(x => x.NewStatus)
                .Must(s => string.IsNullOrEmpty(s) || new[] { "Prcs", "Actv", "Blck", "Clsd" }.Contains(s))
                .WithMessage("Status is not allowed. Allowed: Prcs, Actv, Blck, Clsd");

            RuleFor(x => x.AccountNumber)
                .Length(20).When(x => !string.IsNullOrEmpty(x.AccountNumber))
                .WithMessage("Number of AccountNumber must contains 20 digits");
        }
    }
}
