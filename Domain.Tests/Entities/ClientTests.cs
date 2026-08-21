using Domain.Entities;
using Domain.Exceptions;
using Domain.Enums;
using FluentAssertions;

namespace Domain.Tests.Entities
{
    public class ClientTests
    {
        public ClientTests() { }

        public static TheoryData<string, string, string, string, string?, Type?> ClientTestData = new()
        {
            // Успешные кейсы
            { "mid1", "Петров", "Пётр", "Петрович", null, null },
            { "mid2", "Сидоров", "Сидор", "Сидорович", "part123", null },
            { "mid2", "Сидоров", "Сидор", "-", null, null },

            // ArgumentException
            { "",      "Петров", "Пётр", "Петрович", null, typeof(ArgumentException) },
            { "mid3",  "",       "Пётр", "Петрович", null, typeof(ArgumentException) },
            { "mid4",  "Петров", "",     "Петрович", null, typeof(ArgumentException) },
            { "mid5",  "Петров", "Пётр", "",         null, typeof(ArgumentException) },
            { null!,   "Петров", "Пётр", "Петрович", null, typeof(ArgumentException) },
            { "mid6",  null!,    "Пётр", "Петрович", null, typeof(ArgumentException) },
            { "mid7",  "Петров", null!,  "Петрович", null, typeof(ArgumentException) },
            { "mid8",  "Петров", "Пётр", null!,      null, typeof(ArgumentException) },
        };

        [Theory]
        [MemberData(nameof(ClientTestData))]
        public void ClientConstructor_ShouldValidate(
            string mid, string lastName, string firstName, string patronymic,
            string? participantDRId, Type? expectedExceptionType)
        {
            Action act = () => new Client(mid, lastName, firstName, patronymic, participantDRId);

            if (expectedExceptionType != null)
            {
                act.Should().Throw<Exception>().And.Should().BeOfType(expectedExceptionType);
            }
            else
            {
                var client = new Client(mid, lastName, firstName, patronymic, participantDRId);
                client.Mid.Should().Be(mid);
                client.LastName.Should().Be(lastName);
                client.FirstName.Should().Be(firstName);
                client.Patronymic.Should().Be(patronymic);
                client.ParticipantDRId.Should().Be(participantDRId);
            }
        }

        [Fact]
        public void BindingWalletToClient()
        {
            Client client = new Client("mid1", "Петров", "Пётр", "Петрович");

            client.AddWallet("abrac1312", WalletStatus.Prcs);

            client.Wallets.First().Code.Should().Be("abrac1312");
            client.Wallets.First().Status.Should().Be(WalletStatus.Prcs);
        }

        [Fact]
        public void BindingWalletDublicateExceptionToClient()
        {
            Client client = new Client("mid1", "Петров", "Пётр", "Петрович");

            client.AddWallet("abrac1312", WalletStatus.Prcs);
            Action act = () => client.AddWallet("abrac1312", WalletStatus.Prcs);
            act.Should().Throw<Exception>().And.Should().BeOfType(typeof(DomainException));
        }

        [Fact]
        public void BindingWalletToACtiveExceptionToClient()
        {
            Client client = new Client("mid1", "Петров", "Пётр", "Петрович");

            client.AddWallet("abrac1312", WalletStatus.Prcs);
            Action act = () => client.AddWallet("goyda1432", WalletStatus.Blck);
            act.Should().Throw<Exception>().And.Should().BeOfType(typeof(DomainException));
        }

        [Fact]
        public void SettingParticipantDRId()
        {
            Client client = new Client("mid1", "Петров", "Пётр", "Петрович");

            client.ParticipantDRId.Should().Be(null);

            client.SetParticipantDRId("grokaemAlgosi123r3");

            client.ParticipantDRId.Should().Be("grokaemAlgosi123r3");
        }

        [Fact]
        public void GetWalletByCode_ShouldReturnWallet_WhenCodeExists()
        {
            var client = new Client("mid1", "Иванов", "Иван", "Иванович", null);
            var wallet = client.AddWallet("WALLET001", WalletStatus.Actv);

            var result = client.getWalletByCode("WALLET001");

            Assert.NotNull(result);
            Assert.Equal("WALLET001", result.Code);
            Assert.Same(wallet, result);
        }

        [Fact]
        public void GetWalletByCode_ShouldThrowArgumentNullException_WhenCodeIsNull()
        {
            var client = new Client("mid1", "Иванов", "Иван", "Иванович", null);

            var ex = Assert.Throws<ArgumentNullException>(() => client.getWalletByCode(null));
            Assert.Contains("Code cannot be null or empty.", ex.Message);
        }

        [Fact]
        public void GetWalletByCode_ShouldThrowArgumentNullException_WhenCodeIsEmpty()
        {
            var client = new Client("mid1", "Иванов", "Иван", "Иванович", null);

            var ex = Assert.Throws<ArgumentNullException>(() => client.getWalletByCode(""));
            Assert.Contains("Code cannot be null or empty.", ex.Message);
        }

        [Fact]
        public void GetWalletByCode_ShouldThrowArgumentNullException_WhenCodeIsWhitespace()
        {
            var client = new Client("mid1", "Иванов", "Иван", "Иванович", null);

            var ex = Assert.Throws<ArgumentNullException>(() => client.getWalletByCode("   "));
            Assert.Contains("Code cannot be null or empty.", ex.Message);
        }

        [Fact]
        public void GetWalletByCode_ShouldThrowDomainException_WhenCodeNotFound()
        {
            var client = new Client("mid1", "Иванов", "Иван", "Иванович", null);

            var ex = Assert.Throws<DomainException>(() => client.getWalletByCode("NONEXISTENT"));
            Assert.Contains("Wallet with code 'NONEXISTENT' not found", ex.Message);
            Assert.Contains("mid1", ex.Message);
        }

    }
}
