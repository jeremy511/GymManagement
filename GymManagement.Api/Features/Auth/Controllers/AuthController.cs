using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using GymManagement.Api.Features.Auth.Commands;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Api.Features.Auth.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ISender _mediator;

        public AuthController(ISender mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var command = new RegisterCommand(
                req.GymName, 
                req.Slug, 
                req.Email, 
                req.AdminName, 
                req.AdminEmail, 
                req.Password);

            var result = await _mediator.Send(command);

            return result.IsSuccess 
                ? Ok(result.Value) 
                : BadRequest(new { error = result.Error, code = result.ErrorCode });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var command = new LoginCommand(req.Email, req.Password);
            var result = await _mediator.Send(command);

            return result.IsSuccess 
                ? Ok(result.Value) 
                : Unauthorized(new { error = result.Error, code = result.ErrorCode });
        }

        [HttpPut("profile/{userId:guid}")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile(Guid userId, [FromBody] UpdateUserRequest req)
        {
            var command = new UpdateUserCommand(userId, req.Name, req.Email);
            var result = await _mediator.Send(command);

            return result.IsSuccess 
                ? Ok(new AuthResponse { UserId = result.Value.Id, Email = result.Value.Email, GymId = result.Value.GymId }) 
                : (result.ErrorCode == "NOT_FOUND" ? NotFound(result.Error) : BadRequest(result.Error));
        }
    }

    public record RegisterRequest(
        [Required][StringLength(100)] string GymName, 
        [Required][StringLength(50)] string Slug, 
        [Required][EmailAddress] string Email, 
        [Required][StringLength(100)] string AdminName, 
        [Required][EmailAddress] string AdminEmail, 
        [Required][MinLength(6)] string Password);

    public record LoginRequest(
        [Required][EmailAddress] string Email, 
        [Required] string Password);

    public record UpdateUserRequest(
        [Required][StringLength(100)] string Name, 
        [Required][EmailAddress] string Email);
}
