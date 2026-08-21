using Application.DTO;
using AutoMapper;
using Application.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Commands.CreateOrUpdateWalletFromPlatform
{
    public class CreateOrUpdateWalletFromPlatformCommandHandler : IRequestHandler<CreateOrUpdateWalletFromPlatformCommand, WalletDto>
    {
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;

        public CreateOrUpdateWalletFromPlatformCommandHandler(
            IClientRepository clientRepository,
            IMapper mapper)
        {
            _clientRepository = clientRepository;
            _mapper = mapper;
        }

        public async Task<WalletDto> Handle(
            CreateOrUpdateWalletFromPlatformCommand request,
            CancellationToken cancellationToken)
        {
            var client = await _clientRepository.GetByMidWithWalletsAsync(request.Mid, cancellationToken);
            if (client == null)
                throw new ApplicationLayerException($"Client with mid '{request.Mid}' is not found");

            client.SetParticipantDRId(request.ParticipantDRId); 
            // здесь если будет ошибка, кт её поймает ^^^. Вообще, нужно ли тут его устанавливать
            var activeWallet = client.Wallets.FirstOrDefault(w => w.IsActive);
            // стоит пересмотреть ядро и добавить одн ополе в ачесстве активного кошелька ^^^
            if (activeWallet == null)
            {
                var newWallet = client.AddWallet(request.WalletCode, request.Status);

                if (!string.IsNullOrWhiteSpace(request.AccountNumber))
                    newWallet.SetAccountNumber(request.AccountNumber);

            }
            else
            {
                activeWallet.SetStatus(request.Status);
                if (!string.IsNullOrWhiteSpace(request.AccountNumber))
                    activeWallet.SetAccountNumber(request.AccountNumber);
            }

            await _clientRepository.SaveChangesAsync(cancellationToken);

            var resultWallet = client.Wallets.FirstOrDefault(w => w.IsActive)
                          ?? client.Wallets.LastOrDefault();
            return _mapper.Map<WalletDto>(resultWallet);
        }
    }
}
