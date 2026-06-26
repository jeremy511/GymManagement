using AutoMapper;
using GymManagement.Api.Features.Payments.Commands;
using GymManagement.Api.Features.Payments.Queries;
using GymManagement.Api.Shared.Security;
using GymManagement.Api.Features.Payments.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Api.Features.Payments.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
    public class PaymentsController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly IMapper _mapper;

        public PaymentsController(ISender mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpPost]
        [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreatePaymentRequest req)
        {
            if (!Enum.TryParse<PaymentMethod>(req.Method, true, out var paymentmethod))
            {
                return BadRequest($"Invalid payment method: {req.Method}");
            }
            var command = new CreatePaymentCommand(req.MemberId, req.Amount, paymentmethod, req.ExternalReference);
            var result = await _mediator.Send(command);

            return result.IsSuccess
                ? CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, _mapper.Map<PaymentResponse>(result.Value))
                : BadRequest(result.Error);
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var result = await _mediator.Send(new ListPaymentsQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _mediator.Send(new GetPaymentQuery(id));
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("member/{memberId}")]
        public async Task<IActionResult> ListByMember(Guid memberId)
        {
            var result = await _mediator.Send(new ListPaymentsByMemberQuery(memberId));
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeletePaymentCommand(id));
            if (!result.IsSuccess) return NotFound();

            return NoContent();
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentRequest req)
        {
            if (!Enum.TryParse<PaymentMethod>(req.Method, true, out var paymentmethod))
            {
                return BadRequest($"Invalid payment method: {req.Method}");
            }
            var command = new UpdatePaymentCommand(id, paymentmethod, req.ExternalReference);
            var result = await _mediator.Send(command);

            return result.IsSuccess
                ? Ok(_mapper.Map<PaymentResponse>(result.Value))
                : (result.ErrorCode == "NOT_FOUND" ? NotFound(result.Error) : BadRequest(result.Error));
        }
    }

    public record CreatePaymentRequest(
        [Required] Guid MemberId,
        [System.ComponentModel.DataAnnotations.Range(0.01, 1000000)] decimal Amount,
        [Required][StringLength(50)] string Method,
        [StringLength(100)] string? ExternalReference);

    public record UpdatePaymentRequest(
        [Required][StringLength(50)] string Method,
        [StringLength(100)] string? ExternalReference);
}
