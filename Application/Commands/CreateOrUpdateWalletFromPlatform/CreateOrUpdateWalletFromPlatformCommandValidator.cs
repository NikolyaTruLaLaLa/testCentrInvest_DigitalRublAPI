using Domain.Enums;
using FluentValidation;

namespace Application.Commands.CreateOrUpdateWalletFromPlatform
{
    public class CreateOrUpdateWalletFromPlatformCommandValidator : AbstractValidator<CreateOrUpdateWalletFromPlatformCommand>
    {
        public CreateOrUpdateWalletFromPlatformCommandValidator()
        {
            RuleFor(x => x.Mid)
                .NotEmpty().WithMessage("Mid is neccesary.");

            When(x => !string.IsNullOrEmpty(x.ParticipantDRId), () =>
            {
                RuleFor(x => x.ParticipantDRId)
                    .MaximumLength(255).WithMessage("ParticipantDRId couldn't be more than 255 symbols.");
            });

            RuleFor(x => x.WalletCode)
                .NotEmpty().WithMessage("Wallet's code is neccesary.");

            RuleFor(x => x.Status)
                .Must(s => s == WalletStatus.Prcs || s == WalletStatus.Actv || s == WalletStatus.Blck)
                .WithMessage("Status is not allowed. Allowed: Prcs, Actv, Blck.");

            When(x => !string.IsNullOrEmpty(x.AccountNumber), () =>
            {
                RuleFor(x => x.AccountNumber)
                    .Length(20).WithMessage("Number of AccountNumber must contains 20 digits");
            });
        }
    }
}
