using FluentValidation;
namespace Application.Commands.UpdateWalletFromPlatform
{
    public class UpdateWalletFromPlatformCommandValidator
    : AbstractValidator<UpdateWalletFromPlatformCommand>
    {
        public UpdateWalletFromPlatformCommandValidator()
        {
            RuleFor(x => x.Mid)
                .NotEmpty().WithMessage("Mid is neccesary.");

            RuleFor(x => x.WalletCode)
                .NotEmpty().WithMessage("Code is necccesary.");

            When(x => x.NewStatus.HasValue, () =>
            {
                RuleFor(x => x.NewStatus.Value)
                    .IsInEnum().WithMessage("Некорректный статус.");
            });

            RuleFor(x => x)
            .Must(x => x.NewStatus.HasValue || !string.IsNullOrWhiteSpace(x.AccountNumber))
            .WithMessage("Должен быть указан либо новый статус, либо номер счёта (или оба).");
        }
    }
}
