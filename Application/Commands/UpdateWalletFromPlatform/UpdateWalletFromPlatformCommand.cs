using Application.DTO;
using Domain.Enums;
using MediatR;

namespace Application.Commands.UpdateWalletFromPlatform;

public class UpdateWalletFromPlatformCommand : IRequest<WalletDto>
{
    public string Mid { get; set; }
    public string WalletCode { get; set; }
    public WalletStatus? NewStatus { get; set; }
    public string? AccountNumber { get; set; }
}