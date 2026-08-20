
using Domain.Enums;
using Domain.Exceptions;
using System.Net.NetworkInformation;

namespace Domain.Entities
{
    public class Wallet
    {
        public Guid Id { get; private set; }
        public Guid ClientId { get; private set; }
        public Client Client { get; private set; } = null!;
        public string Code { get; private set; } = null!;
        public WalletStatus Status { get; private set; }
        public string? AccountNumber { get; private set; }
        private bool _isInitialized; // нужен, чтобы не допускать изменения статуса в случае, если статус не инициализован

        private Wallet() { _isInitialized = false; }

        public Wallet(Client client, string code, WalletStatus initialStatus)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required", nameof(code));
            if (!IsValidInitialStatus(initialStatus))
                throw new DomainException($"Invalid initial status '{initialStatus}'. Allowed: Prcs, Actv, Blck, Clsd.");

            Id = Guid.NewGuid();
            ClientId = client.Id;
            Client = client;
            Code = code;
            _isInitialized = true;
            Status = initialStatus;
        }

        private static bool IsValidInitialStatus(WalletStatus status) => status == WalletStatus.Prcs || status == WalletStatus.Actv ||
                                                                         status == WalletStatus.Blck;

        private static readonly Dictionary<WalletStatus, List<WalletStatus>> Transitions = new()
        {
            { WalletStatus.Prcs, new List<WalletStatus> {WalletStatus.Actv}},
            { WalletStatus.Actv, new List<WalletStatus> {WalletStatus.Blck} },
            { WalletStatus.Blck, new List<WalletStatus> { WalletStatus.Actv , WalletStatus.Clsd } },
            { WalletStatus.Clsd, new List<WalletStatus>() }
        };

        public void SetStatus(WalletStatus newStatus)
        {
            if (!_isInitialized)
                throw new DomainException("Cannot change status of uninitialized wallet");
            if (!Transitions[Status].Contains(newStatus))
                throw new DomainException($"Cannot transition from {Status} to {newStatus}");
            Status = newStatus;
        }
        
        public void SetAccountNumber(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                throw new ArgumentException("Account number cannot be empty", nameof(accountNumber));
            if (!string.IsNullOrEmpty(AccountNumber))
                throw new DomainException($"Account number already set to '{AccountNumber}' and cannot be changed.");
            AccountNumber = accountNumber;
        }

        public bool IsActive => Status == WalletStatus.Prcs || Status == WalletStatus.Actv || Status == WalletStatus.Blck;

    }
}
