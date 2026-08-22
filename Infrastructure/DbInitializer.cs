using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();


        await context.Database.MigrateAsync();

 
        if (await context.Clients.AnyAsync())
            return;


        var client1 = new Client(
            mid: "MID001",
            firstName: "Иван",
            lastName: "Иванов",
            patronymic: "Иванович",
            participantDRId: "PART001"
        );

        var client2 = new Client(
            mid: "MID002",
            firstName: "Пётр",
            lastName: "Петров",
            patronymic: "Петрович",
            participantDRId: null
        );

        var client3 = new Client(
            mid: "MID003",
            firstName: "Сидор",
            lastName: "Сидоров",
            patronymic: "Сидорович",
            participantDRId: null
        );

        client1.AddWallet("WALLET001", WalletStatus.Actv);

        var wal = client2.AddWallet("WALLET003", WalletStatus.Blck);
        wal.SetStatus(WalletStatus.Clsd);
        client2.AddWallet("WALLET004", WalletStatus.Actv);

        client3.AddWallet("WALLET005", WalletStatus.Actv);

        await context.Clients.AddRangeAsync(client1, client2, client3);
        await context.SaveChangesAsync();
    }
}