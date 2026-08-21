using FluentValidation;
namespace Application.Commands.UpdateWalletFromPlatform
{
    public class UpdateWalletFromPlatformCommandValidator
    : AbstractValidator<UpdateWalletFromPlatformCommand>
    {
        public UpdateWalletFromPlatformCommandValidator()
        {
            RuleFor(x => x.Mid)
                .NotEmpty().WithMessage("Mid обязателен.");

            RuleFor(x => x.WalletCode)
                .NotEmpty().WithMessage("Код кошелька обязателен.");

            RuleFor(x => x.NewStatus)
                .IsInEnum().WithMessage("Некорректный статус.");
        }
    }
}
