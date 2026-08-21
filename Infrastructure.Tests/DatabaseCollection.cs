using Infrastructure.Tests.Fixtures;
using Xunit;

namespace Infrastructure.Tests
{
    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<PostgreSqlContainerFixture>
    {
    }
}