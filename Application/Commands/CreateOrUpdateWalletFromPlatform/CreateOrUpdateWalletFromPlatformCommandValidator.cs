using Domain.Enums;
using FluentValidation;

namespace Application.Commands.CreateOrUpdateWalletFromPlatform
{
    public class CreateOrUpdateWalletFromPlatformCommandValidator : AbstractValidator<CreateOrUpdateWalletFromPlatformCommand>
    {
        public CreateOrUpdateWalletFromPlatformCommandValidator()
        {
            RuleFor(x => x.Mid)
                .NotEmpty().WithMessage("Mid обязателен.");

            RuleFor(x => x.ParticipantDRId)
                .NotEmpty().WithMessage("Идентификатор участника обязателен.");

            RuleFor(x => x.WalletCode)
                .NotEmpty().WithMessage("Код кошелька обязателен.");

            RuleFor(x => x.Status)
                .Must(s => s == WalletStatus.Prcs || s == WalletStatus.Actv || s == WalletStatus.Blck)
                .WithMessage("Допустимые статусы при создании: Prcs, Actv, Blck.");

            When(x => !string.IsNullOrEmpty(x.AccountNumber), () =>
            {
                RuleFor(x => x.AccountNumber)
                    .MinimumLength(5).WithMessage("Номер счёта должен содержать минимум 5 символов.");
            });
        }
    }
}
