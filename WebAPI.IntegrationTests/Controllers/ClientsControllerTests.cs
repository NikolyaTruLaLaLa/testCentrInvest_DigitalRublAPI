using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using WebAPI.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace WebAPI.IntegrationTests.Controllers;

public class ClientsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ClientsControllerTests(CustomWebApplicationFactory factory)
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
        if (!db.Clients.Any())
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
    }


    [Fact]
    public async Task GetClients_ShouldReturnListOfClients()
    {
        ResetAndSeedDatabase();

        var response = await _client.GetAsync("/api/clients");
        response.EnsureSuccessStatusCode();

        var clients = await response.Content.ReadFromJsonAsync<IEnumerable<ClientResponse>>();
        clients.Should().NotBeEmpty();
        var client = clients!.FirstOrDefault(c => c.Mid == "12345");
        client.Should().NotBeNull();
        client!.Mid.Should().Be("12345");
        clients!.First().FullName.Should().Be("Иванов Иван Иванович");
    }


    [Fact]
    public async Task GetWallets_ShouldReturnEmpty_WhenNoWallets()
    {
        ResetAndSeedDatabase();

        var response = await _client.GetAsync("/api/clients/12345/wallets");
        response.EnsureSuccessStatusCode();

        var wallets = await response.Content.ReadFromJsonAsync<IEnumerable<WalletResponse>>();
        wallets.Should().BeEmpty();
    }


    [Fact]
    public async Task GetWallets_WithExistingMid_ShouldReturnWallets()
    {
        ResetAndSeedDatabase();

        var createRequest = new PlatformWalletRequest
        {
            Mid = "12345",
            ParticipantDRId = "p-001",
            WalletCode = "WALLET-001",
            Status = "Actv",
            AccountNumber = "40702810123456789012"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/platform/wallet", createRequest);
        createResponse.EnsureSuccessStatusCode();

        var response = await _client.GetAsync("/api/clients/12345/wallets");
        response.EnsureSuccessStatusCode();
        var wallets = await response.Content.ReadFromJsonAsync<IEnumerable<WalletResponse>>();

        wallets.Should().NotBeEmpty();
        var createdWallet = wallets!.FirstOrDefault(w => w.Code == "WALLET-001");
        createdWallet.Should().NotBeNull();
        createdWallet!.Code.Should().Be("WALLET-001");
        createdWallet!.Status.Should().Be("Actv");
        createdWallet!.AccountNumber.Should().Be("40702810123456789012");
    }

    [Fact]
    public async Task GetWallets_ShouldReturnAllWallets_WhenMultiple()
    {
        ResetAndSeedDatabase();

        var create1 = new PlatformWalletRequest
        {
            Mid = "12345",
            ParticipantDRId = "p-001",
            WalletCode = "WALLET-ACTIVE",
            Status = "Actv",
            AccountNumber = "40702810123456789012"
        };
        await _client.PostAsJsonAsync("/api/platform/wallet", create1);

        await _client.PutAsJsonAsync("/api/platform/wallet/WALLET-ACTIVE", new PlatformUpdateRequest { Mid = "12345", NewStatus = "Blck" });
        await _client.PutAsJsonAsync("/api/platform/wallet/WALLET-ACTIVE", new PlatformUpdateRequest { Mid = "12345", NewStatus = "Clsd" });

        var create2 = new PlatformWalletRequest
        {
            Mid = "12345",
            ParticipantDRId = "p-001",
            WalletCode = "WALLET-NEW",
            Status = "Actv",
            AccountNumber = "40702810987654321098"
        };
        await _client.PostAsJsonAsync("/api/platform/wallet", create2);


        var response = await _client.GetAsync("/api/clients/12345/wallets");
        response.EnsureSuccessStatusCode();
        var wallets = await response.Content.ReadFromJsonAsync<IEnumerable<WalletResponse>>();

        wallets.Should().HaveCount(2);
        var closedWallet = wallets!.FirstOrDefault(w => w.Code == "WALLET-ACTIVE");
        closedWallet.Should().NotBeNull();
        closedWallet!.Status.Should().Be("Clsd");
        closedWallet.AccountNumber.Should().Be("40702810123456789012");

        var activeWallet = wallets!.FirstOrDefault(w => w.Code == "WALLET-NEW");
        activeWallet.Should().NotBeNull();
        activeWallet!.Status.Should().Be("Actv");
        activeWallet.AccountNumber.Should().Be("40702810987654321098");
    }

    [Fact]
    public async Task GetWallets_WithNonExistingMid_ShouldReturnNotFound()
    {
        ResetAndSeedDatabase();

        var response = await _client.GetAsync("/api/clients/99999/wallets");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    [Fact]
    public async Task GetWallets_ShouldReturnClsdWallet_WhenClosed()
    {
        ResetAndSeedDatabase();

        var createRequest = new PlatformWalletRequest
        {
            Mid = "12345",
            ParticipantDRId = "p-001",
            WalletCode = "WALLET-CLOSED",
            Status = "Actv",
            AccountNumber = "40702810123456789012"
        };
        await _client.PostAsJsonAsync("/api/platform/wallet", createRequest);

        await _client.PutAsJsonAsync("/api/platform/wallet/WALLET-CLOSED", new PlatformUpdateRequest { Mid = "12345", NewStatus = "Blck" });
        await _client.PutAsJsonAsync("/api/platform/wallet/WALLET-CLOSED", new PlatformUpdateRequest { Mid = "12345", NewStatus = "Clsd" });

        var response = await _client.GetAsync("/api/clients/12345/wallets");
        response.EnsureSuccessStatusCode();
        var wallets = await response.Content.ReadFromJsonAsync<IEnumerable<WalletResponse>>();

        var closedWallet = wallets!.FirstOrDefault(w => w.Code == "WALLET-CLOSED");
        closedWallet.Should().NotBeNull();
        closedWallet!.Status.Should().Be("Clsd");
    }
}