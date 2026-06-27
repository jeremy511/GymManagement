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
        private readonly ILogger<AuthController> _logger;

        public AuthController(ISender mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            _logger.LogInformation("New gym registration attempt: {GymName} ({Slug}), admin: {AdminEmail}", 
                req.GymName, req.Slug, req.AdminEmail);

            var command = new RegisterCommand(
                req.GymName, 
                req.Slug, 
                req.Email, 
                req.AdminName, 
                req.AdminEmail, 
                req.Password);

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Gym {GymName} registered successfully", req.GymName);
                return Ok(result.Value);
            }

            _logger.LogWarning("Gym registration failed for {GymName}: {Error}", req.GymName, result.Error);
            return BadRequest(new { error = result.Error, code = result.ErrorCode });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            _logger.LogInformation("Login attempt for user {Email}", req.Email);

            var command = new LoginCommand(req.Email, req.Password);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User {Email} logged in successfully", req.Email);
                return Ok(result.Value);
            }

            _logger.LogWarning("Failed login attempt for {Email}", req.Email);
            return Unauthorized(new { error = result.Error, code = result.ErrorCode });
        }

        [HttpPut("profile/{userId:guid}")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile(Guid userId, [FromBody] UpdateUserRequest req)
        {
            _logger.LogInformation("Updating user profile {UserId}: {Email}", userId, req.Email);

            var command = new UpdateUserCommand(userId, req.Name, req.Email);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                _logger.LogInformation("User {UserId} profile updated successfully", userId);
                return Ok(new AuthResponse { UserId = result.Value.Id, Email = result.Value.Email, GymId = result.Value.GymId });
            }

            if (result.ErrorCode == "NOT_FOUND")
            {
                _logger.LogWarning("User {UserId} not found for profile update", userId);
                return NotFound(result.Error);
            }

            _logger.LogError("Failed to update user {UserId} profile: {Error}", userId, result.Error);
            return BadRequest(result.Error);
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
