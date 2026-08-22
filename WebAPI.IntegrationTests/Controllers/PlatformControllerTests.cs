using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using WebAPI.Contracts;
using Xunit;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.IntegrationTests.Controllers
{
    public class PlatformControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public PlatformControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private void ResetAndSeedDatabase()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            SeedData(db);
        }

        private void SeedData(AppDbContext db)
        {
            var client = new Client(
                mid: "12345",
                lastName: "Иванов",
                firstName: "Иван",
                patronymic: "Иванович",
                participantDRId: "p-001"
            );
            db.Clients.Add(client);
            db.SaveChanges();
        }

        [Fact]
        public async Task CreateOrUpdateWallet_ShouldCreateNewWallet_WhenNoActiveWalletExists()
        {
            ResetAndSeedDatabase();

            var request = new PlatformWalletRequest
            {
                Mid = "12345",
                ParticipantDRId = "p-001",
                WalletCode = "WALLET-002",
                Status = "Prcs",
                AccountNumber = null
            };

            var response = await _client.PostAsJsonAsync("/api/platform/wallet", request);
            var content = await response.Content.ReadAsStringAsync();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("успешно создан/обновлён");
        }

        [Fact]
        public async Task CreateOrUpdateWallet_ShouldReturnBadRequest_WhenStatusInvalid()
        {
            ResetAndSeedDatabase();

            var request = new PlatformWalletRequest
            {
                Mid = "12345",
                ParticipantDRId = "p-001",
                WalletCode = "WALLET-003",
                Status = "InvalidStatus",
                AccountNumber = null
            };

            var response = await _client.PostAsJsonAsync("/api/platform/wallet", request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadAsStringAsync();
            error.Should().Contain("Status is not allowed");
        }

        [Fact]
        public async Task UpdateWallet_ShouldChangeStatus_WhenTransitionAllowed()
        {
            ResetAndSeedDatabase();

            var createRequest = new PlatformWalletRequest
            {
                Mid = "12345",
                ParticipantDRId = "p-001",
                WalletCode = "WALLET-004",
                Status = "Actv",
                AccountNumber = "40702810123456789012"
            };
            var createResponse = await _client.PostAsJsonAsync("/api/platform/wallet", createRequest);
            createResponse.EnsureSuccessStatusCode();

            // Проверяем, что кошелёк появился в GET-запросе (ждём, пока InMemory "устаканится")
            var getAfterCreate = await _client.GetAsync("/api/clients/12345/wallets");
            getAfterCreate.EnsureSuccessStatusCode();
            var walletsAfterCreate = await getAfterCreate.Content.ReadFromJsonAsync<IEnumerable<WalletResponse>>();
            Assert.NotNull(walletsAfterCreate);
            Assert.Contains(walletsAfterCreate, w => w.Code == "WALLET-004");

            var updateRequest = new PlatformUpdateRequest
            {
                Mid = "12345",
                NewStatus = "Blck",
                AccountNumber = null
            };
            var response = await _client.PutAsJsonAsync("/api/platform/wallet/WALLET-004", updateRequest);
            response.EnsureSuccessStatusCode();

            var walletsResponse = await _client.GetAsync("/api/clients/12345/wallets");
            var wallets = await walletsResponse.Content.ReadFromJsonAsync<IEnumerable<WalletResponse>>();
            var wallet = wallets!.First(w => w.Code == "WALLET-004");
            wallet.Status.Should().Be("Blck");
            wallet.AccountNumber.Should().Be("40702810123456789012");
        }

        [Fact]
        public async Task UpdateWallet_ShouldReturnBadRequest_WhenTransitionNotAllowed()
        {
            ResetAndSeedDatabase();

            var createRequest = new PlatformWalletRequest
            {
                Mid = "12345",
                ParticipantDRId = "p-001",
                WalletCode = "WALLET-005",
                Status = "Prcs",
                AccountNumber = null
            };
            await _client.PostAsJsonAsync("/api/platform/wallet", createRequest);

            var updateRequest = new PlatformUpdateRequest
            {
                Mid = "12345",
                NewStatus = "Clsd",
                AccountNumber = null
            };
            var response = await _client.PutAsJsonAsync("/api/platform/wallet/WALLET-005", updateRequest);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadAsStringAsync();
            error.Should().Contain("Cannot transition");
        }

        [Fact]
        public async Task CreateOrUpdateWallet_ShouldUpdateExistingActiveWallet_WhenActiveExists()
        {
            ResetAndSeedDatabase();

            var createRequest1 = new PlatformWalletRequest
            {
                Mid = "12345",
                ParticipantDRId = "p-001",
                WalletCode = "WALLET-ACTIVE",
                Status = "Actv",
                AccountNumber = "40702810123456789012"
            };
            var createResponse1 = await _client.PostAsJsonAsync("/api/platform/wallet", createRequest1);
            createResponse1.EnsureSuccessStatusCode();
            var updateRequest = new PlatformWalletRequest
            {
                Mid = "12345",
                ParticipantDRId = "p-001",
                WalletCode = "WALLET-ACTIVE",
                Status = "Blck",
                AccountNumber = null 
            };
            var updateResponse = await _client.PostAsJsonAsync("/api/platform/wallet", updateRequest);
            updateResponse.EnsureSuccessStatusCode();

            
            var walletsResponse = await _client.GetAsync("/api/clients/12345/wallets");
            var wallets = await walletsResponse.Content.ReadFromJsonAsync<IEnumerable<WalletResponse>>();
            var wallet = wallets!.First(w => w.Code == "WALLET-ACTIVE");
            wallet.Status.Should().Be("Blck");
            wallet.AccountNumber.Should().Be("40702810123456789012");
        }

        [Fact]
        public async Task CreateOrUpdateWallet_ShouldReturnBadRequest_WhenAccountNumberAlreadySet()
        {
            ResetAndSeedDatabase();

            var createRequest = new PlatformWalletRequest
            {
                Mid = "12345",
                ParticipantDRId = "p-001",
                WalletCode = "WALLET-NUMBER",
                Status = "Actv",
                AccountNumber = "40702810123456789012"
            };
            await _client.PostAsJsonAsync("/api/platform/wallet", createRequest);

            var updateRequest = new PlatformWalletRequest
            {
                Mid = "12345",
                ParticipantDRId = "p-001",
                WalletCode = "WALLET-NUMBER",
                Status = "Blck",
                AccountNumber = "40702810987654321098"

            };
            var response = await _client.PostAsJsonAsync("/api/platform/wallet", updateRequest);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadAsStringAsync();
            error.Should().Contain("Account number already set");
        }

        [Fact]
        public async Task CreateOrUpdateWallet_ShouldReturnBadRequest_WhenWalletCodeAlreadyExistsForClient()
        {
            ResetAndSeedDatabase();

            
            var createRequest1 = new PlatformWalletRequest
            {
                Mid = "12345",
                ParticipantDRId = "p-001",
                WalletCode = "DUPLICATE",
                Status = "Actv",
                AccountNumber = null
            };
            await _client.PostAsJsonAsync("/api/platform/wallet", createRequest1);

            var closeToBlck = new PlatformUpdateRequest { Mid = "12345", NewStatus = "Blck", AccountNumber = null };
            await _client.PutAsJsonAsync("/api/platform/wallet/DUPLICATE", closeToBlck);
            var closeToClsd = new PlatformUpdateRequest { Mid = "12345", NewStatus = "Clsd", AccountNumber = null };
            await _client.PutAsJsonAsync("/api/platform/wallet/DUPLICATE", closeToClsd);
            var createRequest2 = new PlatformWalletRequest
            {
                Mid = "12345",
                ParticipantDRId = "p-001",
                WalletCode = "DUPLICATE",
                Status = "Actv",
                AccountNumber = null
            };
            var response = await _client.PostAsJsonAsync("/api/platform/wallet", createRequest2);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadAsStringAsync();
            error.Should().Contain("already exists");
        }
        [Fact]
        public async Task CreateOrUpdateWallet_ShouldReturnBadRequest_WhenStatusIsClsd()
        {
            ResetAndSeedDatabase();

            var request = new PlatformWalletRequest
            {
                Mid = "12345",
                ParticipantDRId = "p-001",
                WalletCode = "WALLET-CLSD",
                Status = "Clsd",
                AccountNumber = null
            };
            var response = await _client.PostAsJsonAsync("/api/platform/wallet", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadAsStringAsync();
            error.Should().Contain("Invalid initial status");
        }

        [Fact]
        public async Task UpdateWallet_ShouldChangeStatus_PrcsToActv()
        {
            ResetAndSeedDatabase();

            var createRequest = new PlatformWalletRequest
            {
                Mid = "12345",
                ParticipantDRId = "p-001",
                WalletCode = "WALLET-PRCS",
                Status = "Prcs",
                AccountNumber = null
            };
            await _client.PostAsJsonAsync("/api/platform/wallet", createRequest);

            var updateRequest = new PlatformUpdateRequest
            {
                Mid = "12345",
                NewStatus = "Actv",
                AccountNumber = null
            };
            var response = await _client.PutAsJsonAsync("/api/platform/wallet/WALLET-PRCS", updateRequest);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var walletsResponse = await _client.GetAsync("/api/clients/12345/wallets");
            var wallets = await walletsResponse.Content.ReadFromJsonAsync<IEnumerable<WalletResponse>>();
            var wallet = wallets!.First(w => w.Code == "WALLET-PRCS");
            wallet.Status.Should().Be("Actv");
        }

        [Fact]
        public async Task UpdateWallet_ShouldChangeStatus_BlckToActv()
        {
            ResetAndSeedDatabase();

            var createRequest = new PlatformWalletRequest
            {
                Mid = "12345",
                ParticipantDRId = "p-001",
                WalletCode = "WALLET-BLCK",
                Status = "Actv",
                AccountNumber = null
            };
            await _client.PostAsJsonAsync("/api/platform/wallet", createRequest);
            var updateToBlck = new PlatformUpdateRequest { Mid = "12345", NewStatus = "Blck", AccountNumber = null };
            await _client.PutAsJsonAsync("/api/platform/wallet/WALLET-BLCK", updateToBlck);

            var updateToActv = new PlatformUpdateRequest { Mid = "12345", NewStatus = "Actv", AccountNumber = null };
            var response = await _client.PutAsJsonAsync("/api/platform/wallet/WALLET-BLCK", updateToActv);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var walletsResponse = await _client.GetAsync("/api/clients/12345/wallets");
            var wallets = await walletsResponse.Content.ReadFromJsonAsync<IEnumerable<WalletResponse>>();
            var wallet = wallets!.First(w => w.Code == "WALLET-BLCK");
            wallet.Status.Should().Be("Actv");
        }

        [Fact]
        public async Task UpdateWallet_ShouldChangeStatus_BlckToClsd()
        {
            ResetAndSeedDatabase();

            var createRequest = new PlatformWalletRequest
            {
                Mid = "12345",
                ParticipantDRId = "p-001",
                WalletCode = "WALLET-BLCK2",
                Status = "Actv",
                AccountNumber = null
            };
            await _client.PostAsJsonAsync("/api/platform/wallet", createRequest);
            var updateToBlck = new PlatformUpdateRequest { Mid = "12345", NewStatus = "Blck", AccountNumber = null };
            await _client.PutAsJsonAsync("/api/platform/wallet/WALLET-BLCK2", updateToBlck);

            var updateToClsd = new PlatformUpdateRequest { Mid = "12345", NewStatus = "Clsd", AccountNumber = null };
            var response = await _client.PutAsJsonAsync("/api/platform/wallet/WALLET-BLCK2", updateToClsd);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var walletsResponse = await _client.GetAsync("/api/clients/12345/wallets");
            var wallets = await walletsResponse.Content.ReadFromJsonAsync<IEnumerable<WalletResponse>>();
            var wallet = wallets!.First(w => w.Code == "WALLET-BLCK2");
            wallet.Status.Should().Be("Clsd");
        }

        [Fact]
        public async Task UpdateWallet_ShouldReturnBadRequest_WhenTransitionFromClsd()
        {
            ResetAndSeedDatabase();

            var createRequest = new PlatformWalletRequest
            {
                Mid = "12345",
                ParticipantDRId = "p-001",
                WalletCode = "WALLET-CLSD-FINAL",
                Status = "Actv",
                AccountNumber = null
            };
            await _client.PostAsJsonAsync("/api/platform/wallet", createRequest);
            var updateToBlck = new PlatformUpdateRequest { Mid = "12345", NewStatus = "Blck", AccountNumber = null };
            await _client.PutAsJsonAsync("/api/platform/wallet/WALLET-CLSD-FINAL", updateToBlck);
            var updateToClsd = new PlatformUpdateRequest { Mid = "12345", NewStatus = "Clsd", AccountNumber = null };
            await _client.PutAsJsonAsync("/api/platform/wallet/WALLET-CLSD-FINAL", updateToClsd);

            var updateInvalid = new PlatformUpdateRequest { Mid = "12345", NewStatus = "Actv", AccountNumber = null };
            var response = await _client.PutAsJsonAsync("/api/platform/wallet/WALLET-CLSD-FINAL", updateInvalid);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadAsStringAsync();
            error.Should().Contain("Cannot transition");
        }

        [Fact]
        public async Task UpdateWallet_ShouldReturnBadRequest_WhenAccountNumberAlreadySet()
        {
            ResetAndSeedDatabase();

            var createRequest = new PlatformWalletRequest
            {
                Mid = "12345",
                ParticipantDRId = "p-001",
                WalletCode = "WALLET-ACC",
                Status = "Actv",
                AccountNumber = "40702810123456789012"
            };
            await _client.PostAsJsonAsync("/api/platform/wallet", createRequest);

            var updateRequest = new PlatformUpdateRequest
            {
                Mid = "12345",
                NewStatus = "Blck",
                AccountNumber = "40702810987654321098"
            };
            var response = await _client.PutAsJsonAsync("/api/platform/wallet/WALLET-ACC", updateRequest);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var error = await response.Content.ReadAsStringAsync();
            error.Should().Contain("Account number already set")
                .And.Contain("cannot be changed");
        }

        [Fact]
        public async Task UpdateWallet_ShouldReturnNotFound_WhenWalletCodeNotFound()
        {
            ResetAndSeedDatabase();

            var updateRequest = new PlatformUpdateRequest
            {
                Mid = "12345",
                NewStatus = "Actv",
                AccountNumber = null
            };
            var response = await _client.PutAsJsonAsync("/api/platform/wallet/NONEXISTENT", updateRequest);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}