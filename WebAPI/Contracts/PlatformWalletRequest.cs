using System.ComponentModel.DataAnnotations;

namespace WebAPI.Contracts
{
    public class PlatformWalletRequest
    {
        [Required]
        public string Mid { get; set; }

        public string? ParticipantDRId { get; set; }

        [Required]
        public string WalletCode { get; set; }

        [Required]
        public string Status { get; set; } // Prcs, Actv, Blck

        public string? AccountNumber { get; set; }
    }
}
