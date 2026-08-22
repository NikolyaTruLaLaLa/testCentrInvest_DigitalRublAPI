using Application.DTO;
using Application.Exceptions;
using AutoMapper;
using Domain.Entities;
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
                throw new KeyNotFoundException($"client with mid '{request.Mid}' isn't found.");

            Wallet wallet;
            try
            {
                wallet = client.getWalletByCode(request.WalletCode);
            }
            catch (DomainException ex) when (ex.Message.Contains("not found"))
            {
                throw new KeyNotFoundException($"Wallet with code '{request.WalletCode}' not found.");
            }

            if (request.NewStatus.HasValue)
            {
                wallet.SetStatus(request.NewStatus.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.AccountNumber))
                wallet.SetAccountNumber(request.AccountNumber);

            await _clientRepository.SaveChangesAsync(cancellationToken);
            return _mapper.Map<WalletDto>(wallet);
        }
    }
}
