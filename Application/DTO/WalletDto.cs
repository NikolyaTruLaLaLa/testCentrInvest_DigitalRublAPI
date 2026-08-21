using Domain.Enums;

namespace Application.DTO
{
    public class WalletDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public WalletStatus Status { get; set; }
        public string? AccountNumber { get; set; }
        public bool IsActive { get; set; }
    }
}
