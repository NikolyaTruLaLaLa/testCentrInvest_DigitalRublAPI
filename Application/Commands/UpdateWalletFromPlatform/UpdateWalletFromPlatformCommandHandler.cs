using Application.DTO;
using Application.Exceptions;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Commands.UpdateWalletFromPlatform
{
    public class UpdateWalletFromPlatformCommandHandler : IRequestHandler<UpdateWalletFromPlatformCommand, WalletDto>
    {
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;

        public UpdateWalletFromPlatformCommandHandler(
        IClientRepository clientRepository,
        IMapper mapper)
        {
            _clientRepository = clientRepository;
            _mapper = mapper;
        }

        public async Task<WalletDto> Handle(
        UpdateWalletFromPlatformCommand request,
        CancellationToken cancellationToken)
        {
            var client = await _clientRepository.GetByMidWithWalletsAsync(request.Mid, cancellationToken);
            if (client == null)
                throw new ApplicationLayerException($"client with mid '{request.Mid}' isn't found.");

            var wallet = client.getWalletByCode(request.WalletCode);
            if (wallet == null)
                throw new ApplicationLayerException($"Wallet with code '{request.WalletCode}' isn't in client {client.Mid} wallets.");

            wallet.SetStatus(request.NewStatus);

            if (!string.IsNullOrWhiteSpace(request.AccountNumber))
                wallet.SetAccountNumber(request.AccountNumber);

            await _clientRepository.SaveChangesAsync(cancellationToken);
            return _mapper.Map<WalletDto>(wallet);
        }
    }
}
