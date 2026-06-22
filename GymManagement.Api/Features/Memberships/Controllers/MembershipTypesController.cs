using AutoMapper;
using GymManagement.Api.Features.Memberships.Commands;
using GymManagement.Api.Features.Memberships.Controllers;
using GymManagement.Api.Features.Memberships.Queries;
using GymManagement.Api.Shared.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace GymManagement.Api.Features.Memberships.Controllers
{
    [ApiController]
    [Route("api/membership-types")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
    public class MembershipTypesController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly IMapper _mapper;

        public MembershipTypesController(ISender mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpPost]
        [ProducesResponseType(typeof(MembershipTypeResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateMembershipTypeRequest req)
        {
            var command = new CreateMembershipTypeCommand(req.Name, req.Description, req.Price, req.DurationMonths);
            var result = await _mediator.Send(command);

            return result.IsSuccess
                ? CreatedAtAction(nameof(List), new { }, _mapper.Map<MembershipTypeResponse>(result.Value))
                : BadRequest(result.Error);
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var result = await _mediator.Send(new ListMembershipTypesQuery());
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteMembershipTypeCommand(id));
            if (!result.IsSuccess) return NotFound();

            return NoContent();
        }
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(MembershipTypeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMembershipType(Guid id, [FromBody] UpdateMembershipTypeRequest req)
        {
            var command = new UpdateMembershipTypeCommand(id, req.Name, req.Description, req.Price, req.DurationMonths);
            var result = await _mediator.Send(command);

            return result.IsSuccess
                ? Ok(_mapper.Map<MembershipTypeResponse>(result.Value))
                : (result.ErrorCode == "NOT_FOUND" ? NotFound(result.Error) : BadRequest(result.Error));
        }

    }

    public record CreateMembershipTypeRequest(
        [Required][StringLength(100)] string Name,
        [Required][StringLength(500)] string Description,
        [Range(0, 1000000)] decimal Price,
        [Range(1, 120)] int DurationMonths);

    public record UpdateMembershipTypeRequest(
        [Required][StringLength(100)] string Name,
        [Required] string Description,
        [Required][Range(0.01, 1000000)] decimal Price,
        [Required][Range(1, 120)] int DurationMonths);
}
