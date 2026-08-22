using Application.DTO;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
using Domain.Exceptions;
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
            Wallet resultWallet;
            if (activeWallet == null)
            {
                if (client.Wallets.Any(w => w.Code == request.WalletCode))
                    throw new ApplicationLayerException($"Wallet with code '{request.WalletCode}' already exists for this client.");
                var newWallet = new Wallet(client, request.WalletCode, request.Status);
                await _clientRepository.AddWalletAsync(newWallet, cancellationToken);
                
                if (!string.IsNullOrWhiteSpace(request.AccountNumber))
                    newWallet.SetAccountNumber(request.AccountNumber);
                await _clientRepository.SaveChangesAsync(cancellationToken);
                resultWallet = newWallet;
            }
            else
            {
                activeWallet.SetStatus(request.Status);
                if (!string.IsNullOrWhiteSpace(request.AccountNumber))
                    activeWallet.SetAccountNumber(request.AccountNumber);
                await _clientRepository.SaveChangesAsync(cancellationToken);
                resultWallet = activeWallet;
            }

     
            return _mapper.Map<WalletDto>(resultWallet);
        }
    }
}
