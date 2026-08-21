using Application.DTO;
using MediatR;


namespace Application.Queries.GetClientWallets
{
    public class GetClientWalletsQuery : IRequest<IEnumerable<WalletDto>>
    {
        public string Mid { get; set; }
    }
}
