using Application.Commands.CreateOrUpdateWalletFromPlatform;
using Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;

namespace Application.Tests.Validators
{
    public class CreateOrUpdateWalletFromPlatformCommandValidatorTests
    {
        private readonly CreateOrUpdateWalletFromPlatformCommandValidator _validator = new();

        [Fact]
        public void Should_HaveError_WhenMidIsEmpty()
        {
            var command = new CreateOrUpdateWalletFromPlatformCommand { Mid = "" };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Mid);
        }

        [Fact]
        public void Should_HaveError_WhenStatusIsInvalid()
        {
            var command = new CreateOrUpdateWalletFromPlatformCommand
            {
                Mid = "123",
                ParticipantDRId = "p",
                WalletCode = "c",
                Status = (WalletStatus)99
            };
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Status);
        }

        [Fact]
        public void Should_NotHaveError_WhenAllValid()
        {
            var command = new CreateOrUpdateWalletFromPlatformCommand
            {
                Mid = "123",
                ParticipantDRId = "p",
                WalletCode = "c",
                Status = WalletStatus.Actv,
                AccountNumber = "ACC123"
            };
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
