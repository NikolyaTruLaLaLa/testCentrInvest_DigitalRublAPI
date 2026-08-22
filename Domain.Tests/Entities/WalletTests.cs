using Domain.Entities;
using Domain.Exceptions;
using Domain.Enums;
using FluentAssertions;

namespace Domain.Tests.Entities
{
    public class WalletTests
    {
        private static readonly Client _client = new Client("mid123", "Тёркин", "Василий", "Степанович");

        public WalletTests() { }

        public static TheoryData<Client, string, WalletStatus, Type?> WalletConstructorTestData = new()
        {
            // Успешные кейсы
            { _client, "WALLET001", WalletStatus.Prcs, null },
            { _client, "WALLET002", WalletStatus.Actv, null },
            { _client, "WALLET003", WalletStatus.Blck, null },

            // Ошибки
            { null!, "WALLET004", WalletStatus.Prcs, typeof(ArgumentNullException) },
            { _client, null!, WalletStatus.Prcs, typeof(ArgumentException) },
            { _client, "", WalletStatus.Prcs, typeof(ArgumentException) },
            { _client, "   ", WalletStatus.Prcs, typeof(ArgumentException) },
            { _client, "WALLET005", WalletStatus.Clsd, typeof(DomainException) },
        };

        [Theory]
        [MemberData(nameof(WalletConstructorTestData))]
        public void WalletConstructor_ShouldValidate(
        Client client, string code, WalletStatus initialStatus, Type expectedExceptionType)
        {
            // Act
            Action act = () => new Wallet(client, code, initialStatus);

            // Assert
            if (expectedExceptionType != null)
            {
                act.Should().Throw<Exception>().And.Should().BeOfType(expectedExceptionType);
            }
            else
            {
                var wallet = new Wallet(client, code, initialStatus);
                wallet.Client.Should().Be(client);
                wallet.ClientId.Should().Be(client.Id);
                wallet.Code.Should().Be(code);
                wallet.Status.Should().Be(initialStatus);
                wallet.AccountNumber.Should().BeNull();
                wallet.Id.Should().NotBeEmpty();
            }
        }

        public static TheoryData<WalletStatus, WalletStatus, Type?> SetStatusTestData = new()
        {
            // Разрешённые переходы
            { WalletStatus.Prcs, WalletStatus.Actv, null },
            { WalletStatus.Actv, WalletStatus.Blck, null },
            { WalletStatus.Blck, WalletStatus.Actv, null },
            { WalletStatus.Blck, WalletStatus.Clsd, null },

            // Запрещённые переходы
            { WalletStatus.Prcs, WalletStatus.Blck, typeof(DomainException) },
            { WalletStatus.Prcs, WalletStatus.Clsd, typeof(DomainException) },
            { WalletStatus.Actv, WalletStatus.Prcs, typeof(DomainException) },
            { WalletStatus.Actv, WalletStatus.Clsd, typeof(DomainException) },
            { WalletStatus.Blck, WalletStatus.Prcs, typeof(DomainException) },
        };

        [Theory]
        [MemberData(nameof(SetStatusTestData))]
        public void SetStatus_ShouldValidateTransitions(
            WalletStatus initialStatus,
            WalletStatus newStatus,
            Type? expectedExceptionType)
        {
            var wallet = new Wallet(_client, $"WALLET_{Guid.NewGuid():N}", initialStatus);

            Action act = () => wallet.SetStatus(newStatus);

            if (expectedExceptionType != null)
            {
                act.Should().Throw<Exception>()
                   .And.Should().BeOfType(expectedExceptionType);
            }
            else
            {
                act.Should().NotThrow();
                wallet.Status.Should().Be(newStatus);
            }
        }

        [Fact]
        public void SetAccountNumberCorrectTest()
        {
            Wallet wallet = new Wallet(_client, "asdasd123", WalletStatus.Prcs);

            wallet.SetAccountNumber("213123asdasd");

            wallet.AccountNumber.Should().Be("213123asdasd");
        }

        public static TheoryData<string?> SetAccountNumberValidationNullData = new()
        {
            null, "", " "
        };

        [Theory]
        [MemberData(nameof(SetAccountNumberValidationNullData))]
        public void SetAccountNumberValidationNullTest(string? code)
        {
            Wallet wallet = new Wallet(_client, "dasd", WalletStatus.Prcs);

            Action act = () => wallet.SetAccountNumber(code);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void SetAccountNumberValidationAfterChangingTest()
        {
            Wallet wallet = new Wallet(_client, "asydguas65d61t263132d", WalletStatus.Prcs);

            wallet.SetAccountNumber("asydguas65d61t263132dasydguas65d61t263132d");

            Action act = () => wallet.SetAccountNumber("dasd");

            act.Should().Throw<DomainException>();
        }


    }
}
