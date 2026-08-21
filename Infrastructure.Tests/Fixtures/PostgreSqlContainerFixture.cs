using System;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Xunit;

namespace Infrastructure.Tests.Fixtures
{
    public class PostgreSqlContainerFixture : IAsyncLifetime
    {
        public PostgreSqlContainer Container { get; private set; } = null!;
        public string ConnectionString => Container.GetConnectionString();
        public async Task InitializeAsync()
        {
            Container = new PostgreSqlBuilder()
                .WithImage("postgres:latest")
                .WithDatabase("testdb")
                .WithUsername("postgres")
                .WithPassword("testpassword")
                .WithCleanUp(true)
                .Build();

            await Container.StartAsync();
        }
        public async Task DisposeAsync()
        {
            await Container.DisposeAsync();
        }

    }
}