using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetActiveByClientIDAsync(Guid clientId, CancellationToken cancellationToken = default);
        Task AddAsync(Wallet wallet, CancellationToken cancellationToken = default);
        Task UpdateAsync(Wallet wallet, CancellationToken cancellationToken = default);
        Task<IEnumerable<Wallet>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    }
}
