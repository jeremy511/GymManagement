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

        public ClassesController(ISender mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
        [ProducesResponseType(typeof(ClassResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateClassRequest req)
        {
            var command = new CreateClassCommand(req.Title, req.StartAt, req.EndAt, req.Capacity);
            var result = await _mediator.Send(command);

            return result.IsSuccess
                ? CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, _mapper.Map<ClassResponse>(result.Value))
                : BadRequest(result.Error);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> List()
        {
            var result = await _mediator.Send(new ListClassesQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(ClassResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _mediator.Send(new GetClassQuery(id));
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("{id}/reserve")]
        [Authorize(Roles = Roles.Member)]
        [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Reserve(Guid id)
        {
            var result = await _mediator.Send(new ReserveClassCommand(id));

            return result.IsSuccess
                ? Ok(_mapper.Map<ReservationResponse>(result.Value))
                : BadRequest(new { error = result.Error, code = result.ErrorCode });
        }

        [HttpDelete("reservations/{reservationId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CancelReservation(Guid reservationId)
        {
            var result = await _mediator.Send(new CancelReservationCommand(reservationId));

            if (!result.IsSuccess)
            {
                if (result.ErrorCode == "NOT_FOUND") return NotFound();
                if (result.ErrorCode == "FORBIDDEN") return Forbid();
                return BadRequest(result.Error);
            }

            return NoContent();
        }

        [HttpGet("{id}/reservations")]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
        public async Task<IActionResult> GetReservations(Guid id)
        {
            var result = await _mediator.Send(new GetClassReservationsQuery(id));
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
