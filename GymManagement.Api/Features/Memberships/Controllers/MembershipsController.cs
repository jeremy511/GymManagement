using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using GymManagement.Api.Features.Memberships.Commands;
using GymManagement.Api.Features.Memberships.Queries;
using AutoMapper;
using GymManagement.Api.Features.Memberships.Controllers;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Api.Features.Memberships.Controllers
{
    [ApiController]
    [Route("api/memberships")]
    [Authorize(Roles = "Admin,Staff")]
    public class MembershipsController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly IMapper _mapper;

        public MembershipsController(ISender mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpPost]
        [ProducesResponseType(typeof(MembershipResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateMembershipRequest req)
        {
            var command = new CreateMembershipCommand(req.MemberId, req.MembershipTypeId, req.StartDate);
            var result = await _mediator.Send(command);

            return result.IsSuccess
                ? Ok(_mapper.Map<MembershipResponse>(result.Value))
                : (result.ErrorCode == "MEMBER_NOT_FOUND" || result.ErrorCode == "TYPE_NOT_FOUND" ? NotFound(result.Error) : BadRequest(result.Error));
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] bool onlyActive = false)
        {
            var result = await _mediator.Send(new ListMembershipsQuery(onlyActive));
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _mediator.Send(new GetMembershipQuery(id));
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("member/{memberId}")]
        public async Task<IActionResult> ListByMember(Guid memberId)
        {
            var result = await _mediator.Send(new ListMembershipsByMemberQuery(memberId));
            return Ok(result);
        }

    }

    public record CreateMembershipRequest(
        [Required] Guid MemberId, 
        [Required] Guid MembershipTypeId, 
        DateTime? StartDate);
}
