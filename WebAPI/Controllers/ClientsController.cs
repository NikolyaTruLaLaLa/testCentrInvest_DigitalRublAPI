using Application.DTO;
using Application.Queries.GetClients;   
using Application.Queries.GetClientWallets;   
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Contracts;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController: ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public ClientsController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ClientResponse>>> GetClients(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            var query = new GetClientsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            };
            var result = await _mediator.Send(query);

            var response = new PagedResult<ClientResponse>
            {
                Items = _mapper.Map<IEnumerable<ClientResponse>>(result.Items),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
            return Ok(response);
        }

        [HttpGet("{mid}/wallets")]
        public async Task<ActionResult<IEnumerable<WalletResponse>>> GetWallets(string mid)
        {
            var query = new GetClientWalletsQuery { Mid = mid };
            var wallets = await _mediator.Send(query);
            return Ok(_mapper.Map<IEnumerable<WalletResponse>>(wallets));
        }
    }
}

