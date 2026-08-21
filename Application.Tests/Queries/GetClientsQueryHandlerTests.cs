using Application.DTO;
using Application.Queries.GetClients;
using Application.Tests.Helpers;
using Domain.Entities;
using Domain.Interfaces;
using Moq;
using Xunit;
namespace Application.Tests.Queries
{
    public class GetClientsQueryHandlerTests
    {
        private readonly Mock<IClientRepository> _repoMock;
        private readonly GetClientsQueryHandler _handler;
        public GetClientsQueryHandlerTests()
        {
            _repoMock = new Mock<IClientRepository>();
            var mapper = TestMapper.Create();
            _handler = new GetClientsQueryHandler(_repoMock.Object, mapper);
        }


        [Fact]
        public async Task Handle_ShouldReturnPagedResult_WhenClientsExist()
        {
            var clients = new List<Client>
            {
                new Client("mid1", "Иванов", "Иван", "Иванович", null),
                new Client( "mid2", "Тёркин", "Василий", "петрович", null)
            };
            _repoMock.Setup(r => r.GetPagedAsync(1, 10, null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((clients, 2));

            var query = new GetClientsQuery { PageNumber = 1, PageSize = 10 };
            var result = await _handler.Handle(query, CancellationToken.None);


            Assert.NotNull(result);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(10, result.PageSize);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmpty_WhenNoClients()
        {
            _repoMock.Setup(r => r.GetPagedAsync(1, 10, null, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((new List<Client>(), 0));

            var query = new GetClientsQuery { PageNumber = 1, PageSize = 10 };

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }


        [Fact]
        public async Task Handle_ShouldCallRepositoryOnce()
        {
            var query = new GetClientsQuery { PageNumber = 1, PageSize = 10 };
            _repoMock.Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((new List<Client>(), 0));

            await _handler.Handle(query, CancellationToken.None);

            _repoMock.Verify(r => r.GetPagedAsync(1, 10, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldPassCancellationToken()
        {
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            var query = new GetClientsQuery { PageNumber = 1, PageSize = 10 };
            _repoMock.Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((new List<Client>(), 0));

            await _handler.Handle(query, token);

            _repoMock.Verify(r => r.GetPagedAsync(1, 10, null, token), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldMapCorrectly()
        {
            var client = new Client("mid1", "Иванов", "Иван", "Иванович", null);
            var clients = new List<Client> { client };
            _repoMock.Setup(r => r.GetPagedAsync(1, 10, null, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((clients, 1));

            var query = new GetClientsQuery { PageNumber = 1, PageSize = 10 };

            var result = await _handler.Handle(query, CancellationToken.None);

            var dto = result.Items.First();
            Assert.Equal("Иванов Иван Иванович", dto.FullName);
            Assert.Equal("mid1", dto.Mid);
            Assert.Null(dto.ParticipantDRId);
        }
    }
}
