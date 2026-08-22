using Application.DTO;
using Application.Exceptions;
using Application.Queries.GetClientWallets;
using Application.Tests.Helpers;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Moq;
using Xunit;

namespace Application.Tests.Queries
{
    public class GetClientWalletsQueryHandlerTests
    {
        private readonly Mock<IClientRepository> _repoMock;
        private readonly GetClientWalletsQueryHandler _handler;

        public GetClientWalletsQueryHandlerTests()
        {
            _repoMock = new Mock<IClientRepository>();
            var mapper = TestMapper.Create();
            _handler = new GetClientWalletsQueryHandler(_repoMock.Object, mapper);
        }

        [Fact]
        public async Task Handle_ShouldReturnWallets_WhenClientExists()
        {
            var client = new Client("mid2", "Тёркин", "Василий", "петрович", null);
            var wal1 = client.AddWallet("Wasdasdasd1", WalletStatus.Blck);
            wal1.SetStatus(WalletStatus.Clsd);
            client.AddWallet("asdasdasdasdW2", WalletStatus.Prcs);

            _repoMock.Setup(r => r.GetByMidWithWalletsAsync("mid2", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(client);

            var query = new GetClientWalletsQuery { Mid = "mid2" };
   
            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.Equal(2, result.Count());
            Assert.Contains(result, w => w.Code == "Wasdasdasd1" && !w.IsActive);
            Assert.Contains(result, w => w.Code == "asdasdasdasdW2" && w.IsActive);
        }

        [Fact]
        public async Task Handle_ShouldThrowApplicationLayerException_WhenClientNotFound()
        {
            _repoMock.Setup(r => r.GetByMidWithWalletsAsync("999", It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Client?)null);

            var query = new GetClientWalletsQuery { Mid = "999" };

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(query, CancellationToken.None));
      
        }

    }
}
