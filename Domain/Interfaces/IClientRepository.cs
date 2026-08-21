using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IClientRepository
    {
        Task<Client?> GetByMidWithWalletsAsync(string mid, CancellationToken cancellationToken = default);
        Task<(IEnumerable<Client> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm,
            CancellationToken cancellationToken = default);
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
