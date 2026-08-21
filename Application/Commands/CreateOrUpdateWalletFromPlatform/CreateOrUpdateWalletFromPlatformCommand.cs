using Application.DTO;
using Domain.Enums;
using MediatR;

namespace Application.Commands.CreateOrUpdateWalletFromPlatform
{
    public class CreateOrUpdateWalletFromPlatformCommand : IRequest<WalletDto>
    {
        public string Mid { get; set; }
        public string ParticipantDRId { get; set; }
        public string WalletCode { get; set; }
        public WalletStatus Status { get; set; }
        public string? AccountNumber { get; set; }
    }
}
