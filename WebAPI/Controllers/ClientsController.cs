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
        public async Task<ActionResult<IEnumerable<ClientResponse>>> GetClients()
        {
            var query = new GetClientsQuery();
            var result = await _mediator.Send(query);
            var clientDtos = result.Items;
            return Ok(_mapper.Map<IEnumerable<ClientResponse>>(clientDtos));
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

