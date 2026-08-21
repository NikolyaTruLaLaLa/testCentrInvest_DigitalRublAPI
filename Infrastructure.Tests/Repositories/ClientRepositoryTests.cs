using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Tests.Fixtures;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Repositories
{
    [Collection("Database collection")]
    public class ClientRepositoryTests : InfrastructureTestBase
    {
        public ClientRepositoryTests(PostgreSqlContainerFixture fixture) : base(fixture) { }

        #region GetByMidWithWalletsAsync
        [Fact]
        public async Task GetByMidWithWalletsAsync_ShouldReturnClientWithWallets_WhenClientExists()
        {
            var client = new Client(
                mid: "MID_001",
                lastName: "Ivanov",
                firstName: "Ivan",
                patronymic: "Ivanovich"
            );
            client.AddWallet("WALLET_001", WalletStatus.Prcs);

            await DbContext.Clients.AddAsync(client);
            await DbContext.SaveChangesAsync();

            var result = await ClientRepository.GetByMidWithWalletsAsync("MID_001");

            result.Should().NotBeNull();
            result!.Mid.Should().Be("MID_001");
            result.Wallets.Should().HaveCount(1);
            result.Wallets.First().Code.Should().Be("WALLET_001");
        }

        [Fact]
        public async Task GetByMidWithWalletsAsync_ShouldReturnNull_WhenClientNotFound()
        {
            var result = await ClientRepository.GetByMidWithWalletsAsync("NONEXISTENT");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByMidWithWalletsAsync_ShouldLoadAllWalletsForClient()
        {
            var client = new Client("MID_002", "Petrov", "Petr", "Petrovich");
            var wal2 = client.AddWallet("WALLET_002A", WalletStatus.Blck);
            wal2.SetStatus(WalletStatus.Clsd);
          
            client.AddWallet("WALLET_002B", WalletStatus.Blck);
            await DbContext.Clients.AddAsync(client);
            await DbContext.SaveChangesAsync();

            var result = await ClientRepository.GetByMidWithWalletsAsync("MID_002");

            result.Should().NotBeNull();
            result!.Wallets.Should().HaveCount(2);
            result.Wallets.Select(w => w.Code).Should().Contain(new[] { "WALLET_002A", "WALLET_002B" });
        }
        #endregion

        #region GetPagedAsync

        [Fact]
        public async Task GetPagedAsync_ShouldReturnCorrectPage_WithoutSearch()
        {
            for (int i = 1; i <= 10; i++)
            {
                var client = new Client($"MID_{i:D3}", $"LastName{i}", $"FirstName{i}", $"Patronymic{i}");
                await DbContext.Clients.AddAsync(client);
            }
            await DbContext.SaveChangesAsync();

            var (items, total) = await ClientRepository.GetPagedAsync(2, 3, null);

            total.Should().Be(10);
            items.Should().HaveCount(3);
            items.Select(c => c.Mid).Should().Contain(new[] { "MID_004", "MID_005", "MID_006" });
        }


        [Fact]
        public async Task GetPagedAsync_ShouldReturnPagedClients_WithSearch()
        {
            var client1 = new Client("MID_001", "Ivanov", "Ivan", "Ivanovich");
            var client2 = new Client("MID_002", "Petrov", "Petr", "Petrovich");
            var client3 = new Client("MID_003", "Sidorov", "Ivan", "Sidorovich");

            await DbContext.Clients.AddRangeAsync(client1, client2, client3);
            await DbContext.SaveChangesAsync();

            var (items, total) = await ClientRepository.GetPagedAsync(1, 2, "Ivan");

            total.Should().Be(2);
            items.Should().HaveCount(2);
            items.Should().Contain(c => c.Mid == "MID_001" || c.Mid == "MID_003");
        }

        [Fact]
        public async Task GetPagedAsync_ShouldFilterBySearchTerm_InMid()
        {
            var client1 = new Client("MID_001", "Ivanov", "Ivan", "Ivanovich");
            var client2 = new Client("MID_002", "Petrov", "Petr", "Petrovich");
            var client3 = new Client("MID_003", "Sidorov", "Sidor", "Sidorovich");
            await DbContext.Clients.AddRangeAsync(client1, client2, client3);
            await DbContext.SaveChangesAsync();

            var (items, total) = await ClientRepository.GetPagedAsync(1, 10, "002");

            total.Should().Be(1);
            items.Should().HaveCount(1);
            items.First().Mid.Should().Be("MID_002");
        }

        [Fact]
        public async Task GetPagedAsync_ShouldFilterBySearchTerm_InFirstName()
        {
            var client1 = new Client("MID_001", "Ivanov", "Ivan", "Ivanovich");
            var client2 = new Client("MID_002", "Petrov", "Petr", "Petrovich");
            var client3 = new Client("MID_003", "Sidorov", "Sidor", "Sidorovich");
            await DbContext.Clients.AddRangeAsync(client1, client2, client3);
            await DbContext.SaveChangesAsync();

            var (items, total) = await ClientRepository.GetPagedAsync(1, 10, "Petr");

            total.Should().Be(1);
            items.Should().HaveCount(1);
            items.First().Mid.Should().Be("MID_002");
        }

        [Fact]
        public async Task GetPagedAsync_ShouldFilterBySearchTerm_InLastName()
        {
            var client1 = new Client("MID_001", "Ivanov", "Ivan", "Ivanovich");
            var client2 = new Client("MID_002", "Petrov", "Petr", "Petrovich");
            var client3 = new Client("MID_003", "Sidorov", "Sidor", "Sidorovich");
            await DbContext.Clients.AddRangeAsync(client1, client2, client3);
            await DbContext.SaveChangesAsync();

            var (items, total) = await ClientRepository.GetPagedAsync(1, 10, "Sidorov");

            total.Should().Be(1);
            items.Should().HaveCount(1);
            items.First().Mid.Should().Be("MID_003");
        }

        [Fact]
        public async Task GetPagedAsync_ShouldReturnEmpty_WhenNoMatch()
        {
            var client = new Client("MID_001", "Ivanov", "Ivan", "Ivanovich");
            await DbContext.Clients.AddAsync(client);
            await DbContext.SaveChangesAsync();

            var (items, total) = await ClientRepository.GetPagedAsync(1, 10, "Xyz");

            total.Should().Be(0);
            items.Should().BeEmpty();
        }

        #endregion

        #region SaveChangesAsync

        [Fact]
        public async Task SaveChangesAsync_ShouldPersistChanges()
        {
            var client = new Client("MID_004", "Smirnov", "Alex", "Alexandrovich");
            await DbContext.Clients.AddAsync(client);

            var rowsAffected = await ClientRepository.SaveChangesAsync();

            rowsAffected.Should().Be(1);
            var saved = await DbContext.Clients.FirstOrDefaultAsync(c => c.Mid == "MID_004");
            saved.Should().NotBeNull();
        }

        #endregion

        [Fact]
        public async Task Update_ShouldPersistChangesToExistingAggregate()
        {
            var client = new Client("MID_008", "Kuznetsov", "Nikolay", "Nikolaevich");
            var wallet = client.AddWallet("WALLET_008", WalletStatus.Prcs);
            await DbContext.Clients.AddAsync(client);
            await DbContext.SaveChangesAsync();

            var loadedClient = await ClientRepository.GetByMidWithWalletsAsync("MID_008");
            var loadedWallet = loadedClient!.Wallets.First();

            loadedWallet.SetStatus(WalletStatus.Actv);

            var rowsAffected = await ClientRepository.SaveChangesAsync();

            rowsAffected.Should().Be(1); 
            var updatedClient = await ClientRepository.GetByMidWithWalletsAsync("MID_008");
            updatedClient!.Wallets.First().Status.Should().Be(WalletStatus.Actv);
        }

    }
}
