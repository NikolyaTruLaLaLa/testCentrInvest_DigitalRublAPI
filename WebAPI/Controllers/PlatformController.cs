using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Commands.CreateOrUpdateWalletFromPlatform; 
using Application.Commands.UpdateWalletFromPlatform;         
using WebAPI.Contracts;
using Domain.Enums; 


namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/platform")]
    public class PlatformController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlatformController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("wallet")]
        public async Task<IActionResult> CreateOrUpdateWallet([FromBody] PlatformWalletRequest request)
        {
            if (!Enum.TryParse<WalletStatus>(request.Status, out var status))
            {
                return BadRequest($"Недопустимый статус: {request.Status}. Допустимые: Prcs, Actv, Blck, Clsd.");
            }

            var command = new CreateOrUpdateWalletFromPlatformCommand
            {
                Mid = request.Mid,
                ParticipantDRId = request.ParticipantDRId,
                WalletCode = request.WalletCode,
                Status = status,
                AccountNumber = request.AccountNumber
            };

            await _mediator.Send(command);
            return Ok(new { message = "Кошелёк успешно создан/обновлён" });
        }

        [HttpPut("wallet/{code}")]
        public async Task<IActionResult> UpdateWallet(string code, [FromBody] PlatformUpdateRequest request)
        {
            WalletStatus? newStatus = null;
            if (!string.IsNullOrWhiteSpace(request.NewStatus))
            {
                if (!Enum.TryParse<WalletStatus>(request.NewStatus, out var parsed))
                {
                    return BadRequest($"Недопустимый статус: {request.NewStatus}. Допустимые: Prcs, Actv, Blck, Clsd.");
                }
                newStatus = parsed;
            }

            var command = new UpdateWalletFromPlatformCommand
            {
                Mid = request.Mid,
                WalletCode = code,
                NewStatus = newStatus,
                AccountNumber = request.AccountNumber
            };

            await _mediator.Send(command);
            return Ok(new { message = "Кошелёк обновлён" });
        }
    }
}
