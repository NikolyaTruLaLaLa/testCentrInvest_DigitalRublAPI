using FluentValidation;
using WebAPI.Contracts;

namespace WebAPI.Validators
{
    public class PlatformWalletRequestValidator : AbstractValidator<PlatformWalletRequest>
    {
        public PlatformWalletRequestValidator()
        {
            RuleFor(x => x.Mid)
                .NotEmpty().WithMessage("MID is neccesary");

            RuleFor(x => x.WalletCode)
                .NotEmpty().WithMessage("Wallet's code is neccesary");

            RuleFor(x => x.Status)
                .Must(s => new[] { "Prcs", "Actv", "Blck", "Clsd" }.Contains(s))
                .WithMessage("Status is not allowed. Allowed: Prcs, Actv, Blck, Clsd");

            RuleFor(x => x.AccountNumber)
                .Length(20).When(x => !string.IsNullOrEmpty(x.AccountNumber))
                .WithMessage("Number of AccountNumber must contains 20 digits");
        }
    }
}
