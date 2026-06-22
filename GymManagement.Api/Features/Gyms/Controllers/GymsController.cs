using AutoMapper;
using GymManagement.Api.Features.Gyms.Commands;
using GymManagement.Api.Features.Gyms.Domain;
using GymManagement.Api.Features.Gyms.Queries;
using GymManagement.Api.Shared.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Api.Features.Gyms.Controllers
{
    [ApiController]
    [Route("api/gyms")]
    [Authorize]
    public class GymsController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly IMapper _mapper;

        public GymsController(ISender mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(GymResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get()
        {
            var result = await _mediator.Send(new GetGymQuery());

            return result.IsSuccess
                ? Ok(_mapper.Map<GymResponse>(result.Value))
                : (result.ErrorCode == "NOT_FOUND" ? NotFound(result.Error) : BadRequest(result.Error));
        }

        [HttpPut]
        [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
        [ProducesResponseType(typeof(GymResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromBody] UpdateGymRequest req)
        {
            var command = new UpdateGymCommand(req.Name, req.Email, req.Slug);
            var result = await _mediator.Send(command);

            return result.IsSuccess
                ? Ok(_mapper.Map<GymResponse>(result.Value))
                : (result.ErrorCode == "NOT_FOUND" ? NotFound(result.Error) : BadRequest(result.Error));
        }
    }

    public record UpdateGymRequest(
        [Required][StringLength(100)] string Name,
        [Required][EmailAddress] string Email,
        [Required][StringLength(50)] string Slug);

    public record GymResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = default!;
        public string Slug { get; init; } = default!;
        public string Email { get; init; } = default!;
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
