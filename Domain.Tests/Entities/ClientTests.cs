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
            Wallet wallet = new Wallet(client, "abrac1312", WalletStatus.Prcs);

            client.AddWallet(wallet);

            client.Wallets.Should().Contain(wallet);
        }

        [Fact]
        public void BindingWalletDublicateExceptionToClient()
        {
            Client client = new Client("mid1", "Петров", "Пётр", "Петрович");
            Wallet wallet = new Wallet(client, "abrac1312", WalletStatus.Prcs);

            client.AddWallet(wallet);
            Action act = () => client.AddWallet(wallet);
            act.Should().Throw<Exception>().And.Should().BeOfType(typeof(DomainException));
        }

        [Fact]
        public void BindingWalletToACtiveExceptionToClient()
        {
            Client client = new Client("mid1", "Петров", "Пётр", "Петрович");
            Wallet wallet1 = new Wallet(client, "abrac1312", WalletStatus.Prcs);
            Wallet wallet2 = new Wallet(client, "goyda1432", WalletStatus.Blck);

            client.AddWallet(wallet1);
            Action act = () => client.AddWallet(wallet2);
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

    }
}
