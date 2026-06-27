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
    [Route("api/memberships")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
    public class MembershipsController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly IMapper _mapper;
        private readonly ILogger<MembershipsController> _logger;

        public MembershipsController(ISender mediator, IMapper mapper, ILogger<MembershipsController> logger)
        {
            _mediator = mediator;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(MembershipResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateMembershipRequest req)
        {
            _logger.LogInformation("Creating membership for member {MemberId}, type: {MembershipTypeId}, start: {StartDate}", 
                req.MemberId, req.MembershipTypeId, req.StartDate);

            var command = new CreateMembershipCommand(req.MemberId, req.MembershipTypeId, req.StartDate);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Membership {MembershipId} created successfully for member {MemberId}", 
                    result.Value!.Id, req.MemberId);
                return Ok(_mapper.Map<MembershipResponse>(result.Value));
            }

            if (result.ErrorCode == "MEMBER_NOT_FOUND" || result.ErrorCode == "TYPE_NOT_FOUND")
            {
                _logger.LogWarning("Failed to create membership - {ErrorCode}: {Error}", result.ErrorCode, result.Error);
                return NotFound(result.Error);
            }

            _logger.LogError("Failed to create membership for member {MemberId}: {Error}", req.MemberId, result.Error);
            return BadRequest(result.Error);
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] bool onlyActive = false)
        {
            _logger.LogInformation("Listing memberships - OnlyActive: {OnlyActive}", onlyActive);

            var result = await _mediator.Send(new ListMembershipsQuery(onlyActive));
            _logger.LogInformation("Retrieved {MembershipCount} memberships", 
                (result as System.Collections.ICollection)?.Count ?? 0);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            _logger.LogInformation("Retrieving membership {MembershipId}", id);

            var result = await _mediator.Send(new GetMembershipQuery(id));

            if (result == null)
            {
                _logger.LogWarning("Membership {MembershipId} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Membership {MembershipId} retrieved successfully", id);
            return Ok(result);
        }

        [HttpGet("member/{memberId}")]
        public async Task<IActionResult> ListByMember(Guid memberId)
        {
            _logger.LogInformation("Listing memberships for member {MemberId}", memberId);

            var result = await _mediator.Send(new ListMembershipsByMemberQuery(memberId));
            _logger.LogInformation("Retrieved {MembershipCount} memberships for member {MemberId}", 
                (result as System.Collections.ICollection)?.Count ?? 0, memberId);

            return Ok(result);
        }

    }

    public record CreateMembershipRequest(
        [Required] Guid MemberId,
        [Required] Guid MembershipTypeId,
        DateTime? StartDate);
}
