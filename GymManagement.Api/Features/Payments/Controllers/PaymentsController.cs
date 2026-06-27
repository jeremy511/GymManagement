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
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(ILogger<PaymentsController> logger, ISender mediator, IMapper mapper)
        {
            _logger = logger;
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpPost]
        [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreatePaymentRequest req)
        {
            _logger.LogInformation("Creating payment for member {MemberId}, amount: ${Amount}, method: {Method}", 
                req.MemberId, req.Amount, req.Method);

            if (!Enum.TryParse<PaymentMethod>(req.Method, true, out var paymentmethod))
            {
                _logger.LogWarning("Invalid payment method provided: {Method}", req.Method);
                return BadRequest($"Invalid payment method: {req.Method}");
            }

            var command = new CreatePaymentCommand(req.MemberId, req.Amount, paymentmethod, req.ExternalReference);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Payment {PaymentId} created successfully for member {MemberId}", 
                    result.Value!.Id, req.MemberId);
                return CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, _mapper.Map<PaymentResponse>(result.Value));
            }

            _logger.LogError("Failed to create payment for member {MemberId}: {Error}", 
                req.MemberId, result.Error);
            return BadRequest(result.Error);
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            _logger.LogInformation("Listing all payments");
            var result = await _mediator.Send(new ListPaymentsQuery());
            _logger.LogInformation("Retrieved {PaymentCount} payments", result?.Count() ?? 0);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id)
        {
            _logger.LogInformation("Getting payment {PaymentId}", id);
            var result = await _mediator.Send(new GetPaymentQuery(id));

            if (result == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Payment {PaymentId} retrieved successfully", id);
            return Ok(result);
        }

        [HttpGet("member/{memberId}")]
        public async Task<IActionResult> ListByMember(Guid memberId)
        {
            _logger.LogInformation("Listing payments for member {MemberId}", memberId);
            var result = await _mediator.Send(new ListPaymentsByMemberQuery(memberId));
            _logger.LogInformation("Retrieved {PaymentCount} payments for member {MemberId}", 
                result?.Count() ?? 0, memberId);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Deleting payment {PaymentId}", id);
            var result = await _mediator.Send(new DeletePaymentCommand(id));

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to delete payment {PaymentId}: {Error}", id, result.Error);
                return NotFound();
            }

            _logger.LogInformation("Payment {PaymentId} deleted successfully", id);
            return NoContent();
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaymentRequest req)
        {
            _logger.LogInformation("Updating payment {PaymentId}, method: {Method}", id, req.Method);

            if (!Enum.TryParse<PaymentMethod>(req.Method, true, out var paymentmethod))
            {
                _logger.LogWarning("Invalid payment method provided for update: {Method}", req.Method);
                return BadRequest($"Invalid payment method: {req.Method}");
            }

            var command = new UpdatePaymentCommand(id, paymentmethod, req.ExternalReference);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Payment {PaymentId} updated successfully", id);
                return Ok(_mapper.Map<PaymentResponse>(result.Value));
            }

            if (result.ErrorCode == "NOT_FOUND")
            {
                _logger.LogWarning("Payment {PaymentId} not found for update", id);
                return NotFound(result.Error);
            }

            _logger.LogError("Failed to update payment {PaymentId}: {Error}", id, result.Error);
            return BadRequest(result.Error);
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
