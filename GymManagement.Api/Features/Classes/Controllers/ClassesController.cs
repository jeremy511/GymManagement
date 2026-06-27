using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using GymManagement.Api.Features.Classes.Commands;
using GymManagement.Api.Features.Classes.Queries;
using GymManagement.Api.Shared.Security;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Api.Features.Classes.Controllers
{
    [ApiController]
    [Route("api/classes")]
    public class ClassesController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<ClassesController> _logger;

        public ClassesController(ISender mediator, IMapper mapper, ILogger<ClassesController> logger)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
        [ProducesResponseType(typeof(ClassResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateClassRequest req)
        {
            _logger.LogInformation("Creating class: {Title}, start: {StartAt}, end: {EndAt}, capacity: {Capacity}", 
                req.Title, req.StartAt, req.EndAt, req.Capacity);

            var command = new CreateClassCommand(req.Title, req.StartAt, req.EndAt, req.Capacity);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Class {ClassId} created successfully: {Title}", result.Value!.Id, req.Title);
                return CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, _mapper.Map<ClassResponse>(result.Value));
            }

            _logger.LogError("Failed to create class {Title}: {Error}", req.Title, result.Error);
            return BadRequest(result.Error);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> List()
        {
            _logger.LogInformation("Listing all classes");

            var result = await _mediator.Send(new ListClassesQuery());
            _logger.LogInformation("Retrieved {ClassCount} classes", 
                (result as System.Collections.ICollection)?.Count ?? 0);

            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ClassResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id)
        {
            _logger.LogInformation("Retrieving class {ClassId}", id);

            var result = await _mediator.Send(new GetClassQuery(id));

            if (result == null)
            {
                _logger.LogWarning("Class {ClassId} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Class {ClassId} retrieved successfully", id);
            return Ok(result);
        }

        [HttpPost("{id}/reserve")]
        [Authorize(Roles = Roles.Member)]
        [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Reserve(Guid id)
        {
            _logger.LogInformation("Member reserving class {ClassId}", id);

            var result = await _mediator.Send(new ReserveClassCommand(id));

            if (result.IsSuccess)
            {
                _logger.LogInformation("Reservation {ReservationId} created for class {ClassId}", 
                    result.Value!.Id, id);
                return Ok(_mapper.Map<ReservationResponse>(result.Value));
            }

            _logger.LogWarning("Failed to reserve class {ClassId}: {Error}", id, result.Error);
            return BadRequest(new { error = result.Error, code = result.ErrorCode });
        }

        [HttpDelete("reservations/{reservationId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CancelReservation(Guid reservationId)
        {
            _logger.LogInformation("Canceling reservation {ReservationId}", reservationId);

            var result = await _mediator.Send(new CancelReservationCommand(reservationId));

            if (!result.IsSuccess)
            {
                if (result.ErrorCode == "NOT_FOUND")
                {
                    _logger.LogWarning("Reservation {ReservationId} not found", reservationId);
                    return NotFound();
                }

                if (result.ErrorCode == "FORBIDDEN")
                {
                    _logger.LogWarning("Access denied to cancel reservation {ReservationId}", reservationId);
                    return Forbid();
                }

                _logger.LogError("Failed to cancel reservation {ReservationId}: {Error}", reservationId, result.Error);
                return BadRequest(result.Error);
            }

            _logger.LogInformation("Reservation {ReservationId} cancelled successfully", reservationId);
            return NoContent();
        }

        [HttpGet("{id}/reservations")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
        public async Task<IActionResult> GetReservations(Guid id)
        {
            _logger.LogInformation("Retrieving reservations for class {ClassId}", id);

            var result = await _mediator.Send(new GetClassReservationsQuery(id));
            _logger.LogInformation("Retrieved {ReservationCount} reservations for class {ClassId}", 
                (result as System.Collections.ICollection)?.Count ?? 0, id);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteClassCommand(id));
            if (!result.IsSuccess) return NotFound();

            return NoContent();
        }
    }

    public record CreateClassRequest(
        [Required][StringLength(100)] string Title,
        [Required] DateTime StartAt,
        [Required] DateTime EndAt,
        [System.ComponentModel.DataAnnotations.Range(1, 1000)] int Capacity);
}
