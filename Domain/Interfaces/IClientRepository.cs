using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IClientRepository
    {
        Task<Client?> GetByMidAsync(string mid, CancellationToken cancellationToken = default);
        Task<IEnumerable<Client>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Client client, CancellationToken cancellationToken = default);
    }
}
