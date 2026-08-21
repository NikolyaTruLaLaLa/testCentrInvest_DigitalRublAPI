using Application.DTO;
using MediatR;

namespace Application.Queries.GetClients
{
    public class GetClientsQuery : IRequest<PagedResult<ClientDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
    }
}
