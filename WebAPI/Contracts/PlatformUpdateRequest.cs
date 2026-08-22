using System.ComponentModel.DataAnnotations;
namespace WebAPI.Contracts
{
    public class PlatformUpdateRequest
    {
        [Required]
        public string Mid { get; set; }
        [Required]
        public string NewStatus { get; set; }

        public string? AccountNumber { get; set; }
    }
}
