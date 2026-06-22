using AutoMapper;
using GymManagement.Api.Features.Members.Commands;
using GymManagement.Api.Features.Members.Queries;
using GymManagement.Api.Shared.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace GymManagement.Api.Features.Members.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
    public class MembersController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly IMapper _mapper;

        public MembersController(ISender mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpPost]
        [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateMemberRequest req)
        {
            var command = new CreateMemberCommand(req.FirstName, req.LastName, req.Email);
            var result = await _mediator.Send(command);

            return result.IsSuccess
                ? CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, _mapper.Map<MemberResponse>(result.Value))
                : (result.ErrorCode == "UNAUTHORIZED" ? Unauthorized() : BadRequest(result.Error));
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var query = new ListMembersQuery(search, page, pageSize);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Get(Guid id)
        {
            var query = new GetMemberQuery(id);
            var member = await _mediator.Send(query);

            if (member == null) return NotFound();

            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userIdClaim = User.FindFirst("userId")?.Value;

            if (userIdClaim == null) return Unauthorized("Missing user ID claim.");

            var userId = Guid.Parse(userIdClaim);

            if (userRole == "Member" && userId != id)
                return Forbid();

            return Ok(member);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteMemberCommand(id));
            if (!result.IsSuccess) return NotFound();

            return NoContent();
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMemberRequest req)
        {
            var command = new UpdateMemberCommand(id, req.FirstName, req.LastName, req.Email);
            var result = await _mediator.Send(command);

            return result.IsSuccess
                ? Ok(_mapper.Map<MemberResponse>(result.Value))
                : (result.ErrorCode == "NOT_FOUND" ? NotFound(result.Error) : BadRequest(result.Error));
        }
    }

    #region Requests/Responses
    public record CreateMemberRequest(
        [Required][StringLength(100)] string FirstName,
        [Required][StringLength(100)] string LastName,
        [Required][EmailAddress] string Email);
    public record MemberResponse
    {
        public Guid Id { get; init; }
        public string FirstName { get; init; } = default!;
        public string LastName { get; init; } = default!;
        public string Email { get; init; } = default!;
        public bool IsActive { get; init; }
        public DateTime JoinedAt { get; init; }
        public bool HasActiveMembership { get; init; }
    }

    public record UpdateMemberRequest(
        [Required][StringLength(100)] string FirstName,
        [Required][StringLength(100)] string LastName,
        [Required][EmailAddress] string Email);
    #endregion
}
