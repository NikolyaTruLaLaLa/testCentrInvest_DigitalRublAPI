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

        private readonly Dictionary<string, Wallet> _wallets = new();
        public IReadOnlyCollection<Wallet> Wallets => _wallets.Values;

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

        public void AddWallet(string code, WalletStatus initialStatus)
        {
            if (_wallets.ContainsKey(code))
                throw new DomainException($"wallet {code} is already binded to client {this.Mid}");

            Wallet newWallet = new Wallet(this, code, initialStatus);
            
            // добавить функицю для проверки enum и тогда поднять проверку выше
            if (this.HasActiveWallet() && newWallet.IsActive)
                throw new DomainException($"wallet {code} is active and couldn't be added to client {this.Mid} with active wallet");
            
            _wallets.Add(code, newWallet);
        }

        // может  сделать так, чтобы его установить только однажды?
        public void SetParticipantDRId(string? participantDRId)
        {
            ParticipantDRId = participantDRId;
        }

        public bool HasActiveWallet()
        {
            return _wallets.Any(kvp => kvp.Value.IsActive);
        }
    }
}
