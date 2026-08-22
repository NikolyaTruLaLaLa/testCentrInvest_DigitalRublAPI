using Application.Commands.CreateOrUpdateWalletFromPlatform;
using Application.DTO;
using Application.Exceptions;
using Application.Tests.Helpers;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Moq;
using Xunit;
namespace Application.Tests.Commands
{
    public class CreateOrUpdateWalletFromPlatformCommandHandlerTests
    {
        private readonly Mock<IClientRepository> _repoMock;
        private readonly CreateOrUpdateWalletFromPlatformCommandHandler _handler;

        public CreateOrUpdateWalletFromPlatformCommandHandlerTests()
        {
            _repoMock = new Mock<IClientRepository>();
            var mapper = TestMapper.Create();
            _handler = new CreateOrUpdateWalletFromPlatformCommandHandler(_repoMock.Object, mapper);
        }

        [Fact]
        public async Task Handle_ShouldCreateNewWallet_WhenNoActiveWallet()
        {
            var client = new Client("mid2", "Тёркин", "Василий", "петрович", null);
            _repoMock.Setup(r => r.GetByMidWithWalletsAsync("mid2", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(client);
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(1);

            var command = new CreateOrUpdateWalletFromPlatformCommand
            {
                Mid = "mid2",
                ParticipantDRId = "participant1",
                WalletCode = "WALLET001",
                Status = WalletStatus.Actv,
                AccountNumber = "ACC123"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("WALLET001", result.Code);
            Assert.Equal(WalletStatus.Actv, result.Status);
            Assert.Equal("ACC123", result.AccountNumber);
            Assert.True(result.IsActive);
            _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldUpdateExistingActiveWallet_WhenExists()
        {
            var client = new Client("mid2", "Тёркин", "Василий", "петрович", null);
            client.AddWallet("EXISTING", WalletStatus.Prcs);
            var activeWallet = client.Wallets.First();

            _repoMock.Setup(r => r.GetByMidWithWalletsAsync("mid2", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(client);
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(1);

            var command = new CreateOrUpdateWalletFromPlatformCommand
            {
                Mid = "mid2",
                ParticipantDRId = "participant1",
                WalletCode = "NEWCODE", 
                Status = WalletStatus.Actv,
                AccountNumber = "ACC456"
            };

  
            var result = await _handler.Handle(command, CancellationToken.None);

     
            Assert.Equal("EXISTING", result.Code);
            Assert.Equal(WalletStatus.Actv, result.Status);
            Assert.Equal("ACC456", result.AccountNumber);
            Assert.True(result.IsActive);
        }

        [Fact]
        public async Task Handle_ShouldThrowDomainException_WhenClientNotFound()
        {
            _repoMock.Setup(r => r.GetByMidWithWalletsAsync("999", It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Client?)null);

            var command = new CreateOrUpdateWalletFromPlatformCommand { Mid = "999", ParticipantDRId = "p", WalletCode = "c", Status = WalletStatus.Actv };

            await Assert.ThrowsAsync<ApplicationLayerException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowDomainException_WhenDuplicateWalletCode()
        {
            var client = new Client("mid2", "Тёркин", "Василий", "петрович", null);
            var wal1 = client.AddWallet("DUPLICATE", WalletStatus.Blck);
            wal1.SetStatus(WalletStatus.Clsd);
            _repoMock.Setup(r => r.GetByMidWithWalletsAsync("mid2", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(client);

            var command = new CreateOrUpdateWalletFromPlatformCommand
            {
                Mid = "mid2",
                ParticipantDRId = "p",
                WalletCode = "DUPLICATE",
                Status = WalletStatus.Actv
            };

            var ex = await Assert.ThrowsAsync<ApplicationLayerException>(() => _handler.Handle(command, CancellationToken.None));
          
        }


    }
}
