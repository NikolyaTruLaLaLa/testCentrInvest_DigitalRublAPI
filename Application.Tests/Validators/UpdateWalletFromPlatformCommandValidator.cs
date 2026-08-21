using Application.Commands.UpdateWalletFromPlatform;
using Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;

namespace Application.Tests.Validators
{
    public class UpdateWalletFromPlatformCommandValidatorTests
    {
        private readonly UpdateWalletFromPlatformCommandValidator _validator = new();

        [Fact]
        public void Should_HaveError_WhenMidIsEmpty()
        {
            var command = new UpdateWalletFromPlatformCommand
            {
                Mid = "",
                WalletCode = "WALLET001",
                NewStatus = WalletStatus.Actv
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.Mid);
        }

        [Fact]
        public void Should_HaveError_WhenWalletCodeIsEmpty()
        {
            var command = new UpdateWalletFromPlatformCommand
            {
                Mid = "mid1",
                WalletCode = "",
                NewStatus = WalletStatus.Actv
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.WalletCode);
        }

        [Fact]
        public void Should_HaveError_WhenNewStatusIsInvalid()
        {
            var command = new UpdateWalletFromPlatformCommand
            {
                Mid = "mid1",
                WalletCode = "WALLET001",
                NewStatus = (WalletStatus)99 
            };

            var result = _validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.NewStatus);
        }

        [Fact]
        public void Should_NotHaveError_WhenAllValid()
        {
            var command = new UpdateWalletFromPlatformCommand
            {
                Mid = "mid1",
                WalletCode = "WALLET001",
                NewStatus = WalletStatus.Blck
            };

            var result = _validator.TestValidate(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}