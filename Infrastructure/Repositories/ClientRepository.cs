using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class ClientRepository: IClientRepository
    {
        private readonly AppDbContext _context;
        public ClientRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Client?> GetByMidWithWalletsAsync(string mid, CancellationToken cancellationToken = default)
        {
            return await _context.Clients.Include(c => c.Wallets).FirstOrDefaultAsync(c => c.Mid == mid, cancellationToken);
        }

        public async Task<(IEnumerable<Client> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm, CancellationToken cancellationToken = default)
        {
            var query = _context.Clients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(c => c.Mid.Contains(searchTerm) || c.LastName.Contains(searchTerm)
                                        || c.FirstName.Contains(searchTerm) || c.Patronymic.Contains(searchTerm));
            
            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query.OrderBy(c => c.Mid).Skip((pageNumber - 1) * pageSize).Take(pageSize)
                                    .ToListAsync(cancellationToken);
            
            return (items, totalCount);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Before SaveChanges");
            var result = await _context.SaveChangesAsync(cancellationToken);
            Console.WriteLine($"After SaveChanges: {result} entries saved");
            return result;
        }

        public async Task AddWalletAsync(Wallet wallet, CancellationToken cancellationToken)
        {
            var entry = _context.Entry(wallet);
            Console.WriteLine($"Before Add: {entry.State}");
            await _context.Wallets.AddAsync(wallet, cancellationToken);
            Console.WriteLine($"After Add: {_context.Entry(wallet).State}"); // должно быть Added
        }


    }
}
