using Domain.Enums;
using Domain.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace Domain.Entities
{
    public class Client
    {
        public Guid Id { get; private set; }
        public string Mid { get; private set; }
        public string LastName { get; private set; }
        public string FirstName { get; private set; }
        public string Patronymic { get; private set; }
        public string? ParticipantDRId { get; private set; }

        private readonly List<Wallet> _wallets = new();
        public IReadOnlyCollection<Wallet> Wallets => _wallets.AsReadOnly();

        private Client() {
            Mid = string.Empty;
            LastName = string.Empty;
            FirstName = string.Empty;
            Patronymic = string.Empty;
        }
        public Client(string mid, string lastName, string firstName, string patronymic, string? participantDRId = null)
        {
            ValidateNullOrWhiteSpace(mid, nameof(mid));
            ValidateNullOrWhiteSpace(lastName, nameof(lastName));
            ValidateNullOrWhiteSpace(firstName, nameof(firstName));
            ValidateNullOrWhiteSpace(patronymic, nameof(patronymic));

            Id = Guid.NewGuid();
            Mid = mid;
            LastName = lastName; FirstName = firstName; Patronymic = patronymic;
            ParticipantDRId = participantDRId;
        }

        private static void ValidateNullOrWhiteSpace(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{paramName} is required", paramName);
        }

        public Wallet AddWallet(string code, WalletStatus initialStatus)
        {
            if (_wallets.Any(w => w.Code == code))
                throw new DomainException($"Wallet with code '{code}' already exists.");
            Wallet newWallet = new Wallet(this, code, initialStatus);
            // добавить функицю для проверки enum и тогда поднять проверку выше
            if (newWallet.IsActive && _wallets.Any(w => w.IsActive))
                throw new DomainException("Client already has an active wallet.");
            
            _wallets.Add(newWallet);
            return newWallet;
        }

        // может  сделать так, чтобы его установить только однажды?
        public void SetParticipantDRId(string? participantDRId)
        {
            ParticipantDRId = participantDRId;
        }

        public bool HasActiveWallet()
        {
            return _wallets.Any(v => v.IsActive);
        }

        public Wallet getWalletByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentNullException("Code cannot be null or empty.");
            var wallet = _wallets.FirstOrDefault(w => w.Code == code);
            if (wallet == null)
                throw new DomainException($"Wallet with code '{code}' not found for client {Mid}.");
            return wallet;
        }
    }
}
