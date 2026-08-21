using Application.Commands.UpdateWalletFromPlatform;
using Application.Tests.Helpers;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Moq;
using Xunit;

namespace Application.Tests.Commands
{
    public class UpdateWalletFromPlatformCommandHandlerTests
    {
        private readonly Mock<IClientRepository> _repoMock;
        private readonly UpdateWalletFromPlatformCommandHandler _handler;

        public UpdateWalletFromPlatformCommandHandlerTests()
        {
            _repoMock = new Mock<IClientRepository>();
            var mapper = TestMapper.Create();
            _handler = new UpdateWalletFromPlatformCommandHandler(_repoMock.Object, mapper);
        }

        [Fact]
        public async Task Handle_ShouldUpdateStatusAndAccountNumber_WhenValid()
        {

            var client = new Client("mid2", "Тёркин", "Василий", "петрович", null);
            client.AddWallet("WALLET001", WalletStatus.Actv);
            var wallet = client.Wallets.First();

            _repoMock.Setup(r => r.GetByMidWithWalletsAsync("mid2", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(client);
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(1);

            var command = new UpdateWalletFromPlatformCommand
            {
                Mid = "mid2",
                WalletCode = "WALLET001",
                NewStatus = WalletStatus.Blck,
                AccountNumber = "ACC999"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal("WALLET001", result.Code);
            Assert.Equal(WalletStatus.Blck, result.Status);
            Assert.Equal("ACC999", result.AccountNumber);
        }

        [Fact]
        public async Task Handle_ShouldOnlySetAccountNumber_WhenStatusNotChanged()
        {
            var client = new Client("mid2", "Тёркин", "Василий", "петрович", null);
            client.AddWallet("WALLET001", WalletStatus.Actv);
            var wallet = client.Wallets.First();

            _repoMock.Setup(r => r.GetByMidWithWalletsAsync("mid2", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(client);
            _repoMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                     .ReturnsAsync(1);

            var command = new UpdateWalletFromPlatformCommand
            {
                Mid = "mid2",
                WalletCode = "WALLET001",
                NewStatus = WalletStatus.Blck, 
                AccountNumber = "ACC888"
            };

            var result = await _handler.Handle(command, CancellationToken.None);

            Assert.Equal(WalletStatus.Blck, result.Status);
            Assert.Equal("ACC888", result.AccountNumber);
        }

        [Fact]
        public async Task Handle_ShouldThrowDomainException_WhenWalletNotFound()
        {
            var client = new Client("mid2", "Тёркин", "Василий", "петрович", null);
            _repoMock.Setup(r => r.GetByMidWithWalletsAsync("mid2", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(client);

            var command = new UpdateWalletFromPlatformCommand
            {
                Mid = "mid2",
                WalletCode = "NONEXISTENT",
                NewStatus = WalletStatus.Blck
            };

            var ex = await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowDomainException_WhenInvalidStatusTransition()
        {
            var client = new Client("mid2", "Тёркин", "Василий", "петрович", null);
            var wal1 = client.AddWallet("WALLET001", WalletStatus.Blck);
            wal1.SetStatus(WalletStatus.Clsd);
            _repoMock.Setup(r => r.GetByMidWithWalletsAsync("mid2", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(client);

            var command = new UpdateWalletFromPlatformCommand
            {
                Mid = "mid2",
                WalletCode = "WALLET001",
                NewStatus = WalletStatus.Actv 
            };

            await Assert.ThrowsAsync<DomainException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}
