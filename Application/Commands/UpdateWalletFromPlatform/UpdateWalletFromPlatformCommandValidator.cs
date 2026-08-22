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

            RuleFor(x => x.NewStatus)
                .IsInEnum().WithMessage("Status is not allowed. Allowed: Prcs, Actv, Blck.");
        }
    }
}
