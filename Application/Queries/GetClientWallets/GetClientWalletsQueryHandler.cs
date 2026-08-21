using Application.DTO;
using Application.Exceptions;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Queries.GetClientWallets
{
    public class GetClientWalletsQueryHandler : IRequestHandler<GetClientWalletsQuery, IEnumerable<WalletDto>>
    {
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;

        public GetClientWalletsQueryHandler(IClientRepository clientRepository, IMapper mapper)
        {
            _clientRepository = clientRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WalletDto>> Handle(GetClientWalletsQuery request, CancellationToken cancellationToken)
        {
            var client = await _clientRepository.GetByMidWithWalletsAsync(request.Mid, cancellationToken);
            if (client == null)
                throw new ApplicationLayerException($"Client with mid '{request.Mid}' isn't founded.");

            return _mapper.Map<IEnumerable<WalletDto>>(client.Wallets);
        }
    }
}
