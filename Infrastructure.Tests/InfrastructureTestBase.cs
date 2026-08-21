using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Infrastructure.Tests
{
    [Collection("Database collection")]
    public abstract class InfrastructureTestBase : IAsyncLifetime
    {
        protected AppDbContext DbContext { get; private set; } = null!;
        protected ClientRepository ClientRepository { get; private set; } = null!;
        protected readonly PostgreSqlContainerFixture Fixture;

        protected InfrastructureTestBase(PostgreSqlContainerFixture fixture)
        {
            Fixture = fixture;
        }

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(Fixture.ConnectionString)
                .Options;

            DbContext = new AppDbContext(options);
            ClientRepository = new ClientRepository(DbContext);

            await DbContext.Database.MigrateAsync();
            await ClearDatabaseAsync();
        }

        public async Task DisposeAsync()
        {
            await DbContext.DisposeAsync();
        }

        protected async Task ClearDatabaseAsync()
        {
            await DbContext.Wallets.ExecuteDeleteAsync();
            await DbContext.Clients.ExecuteDeleteAsync();
            await DbContext.SaveChangesAsync();
        }
    }
}