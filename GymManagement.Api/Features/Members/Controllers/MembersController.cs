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
        private readonly ILogger<MembersController> _logger;

        public MembersController(ISender mediator, IMapper mapper, ILogger<MembersController> logger)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateMemberRequest req)
        {
            _logger.LogInformation("Creating new member: {FirstName} {LastName} ({Email})", 
                req.FirstName, req.LastName, req.Email);

            var command = new CreateMemberCommand(req.FirstName, req.LastName, req.Email);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Member {MemberId} created successfully: {FirstName} {LastName}", 
                    result.Value!.Id, req.FirstName, req.LastName);
                return CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, _mapper.Map<MemberResponse>(result.Value));
            }

            if (result.ErrorCode == "UNAUTHORIZED")
            {
                _logger.LogWarning("Unauthorized attempt to create member");
                return Unauthorized();
            }

            _logger.LogError("Failed to create member {Email}: {Error}", req.Email, result.Error);
            return BadRequest(result.Error);
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Listing members - Search: {Search}, Page: {Page}, PageSize: {PageSize}", 
                search ?? "none", page, pageSize);

            var query = new ListMembersQuery(search, page, pageSize);
            var result = await _mediator.Send(query);

            _logger.LogInformation("Retrieved members list: {ResultCount} items", 
                (result as System.Collections.ICollection)?.Count ?? 0);

            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Get(Guid id)
        {
            _logger.LogInformation("Retrieving member {MemberId}", id);

            var query = new GetMemberQuery(id);
            var member = await _mediator.Send(query);

            if (member == null)
            {
                _logger.LogWarning("Member {MemberId} not found", id);
                return NotFound();
            }

            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userIdClaim = User.FindFirst("userId")?.Value;

            if (userIdClaim == null)
            {
                _logger.LogWarning("Missing user ID claim for member retrieval");
                return Unauthorized("Missing user ID claim.");
            }

            var userId = Guid.Parse(userIdClaim);

            if (userRole == "Member" && userId != id)
            {
                _logger.LogWarning("Access denied: User {UserId} tried to access member {MemberId}", userId, id);
                return Forbid();
            }

            _logger.LogInformation("Member {MemberId} retrieved successfully", id);
            return Ok(member);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            _logger.LogInformation("Deleting member {MemberId}", id);

            var result = await _mediator.Send(new DeleteMemberCommand(id));

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to delete member {MemberId}: {Error}", id, result.Error);
                return NotFound();
            }

            _logger.LogInformation("Member {MemberId} deleted successfully", id);
            return NoContent();
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MemberResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMemberRequest req)
        {
            _logger.LogInformation("Updating member {MemberId}: {FirstName} {LastName} ({Email})", 
                id, req.FirstName, req.LastName, req.Email);

            var command = new UpdateMemberCommand(id, req.FirstName, req.LastName, req.Email);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Member {MemberId} updated successfully", id);
                return Ok(_mapper.Map<MemberResponse>(result.Value));
            }

            if (result.ErrorCode == "NOT_FOUND")
            {
                _logger.LogWarning("Member {MemberId} not found for update", id);
                return NotFound(result.Error);
            }

            _logger.LogError("Failed to update member {MemberId}: {Error}", id, result.Error);
            return BadRequest(result.Error);
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
