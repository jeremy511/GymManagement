using GymManagement.Api.Features.Auth.Domain;
using GymManagement.Api.Infrastructure.Persistence;
using GymManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GymManagement.Api.Features.Auth.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly GymManagementDbContext _db;
        private readonly IPasswordHasher _hasher;
        private readonly IJwtService _jwt;

        public AuthController(GymManagementDbContext db, IPasswordHasher hasher, IJwtService jwt)
        {
            _db = db;
            _hasher = hasher;
            _jwt = jwt;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.GymName) ||
                string.IsNullOrWhiteSpace(req.Slug) ||
                string.IsNullOrWhiteSpace(req.AdminName) ||
                string.IsNullOrWhiteSpace(req.AdminEmail) ||
                string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest("All fields are required.");
            }

            var normalizedEmail = req.AdminEmail.ToLower();
            var normalizedSlug = req.Slug.ToLower();

            // Verificar slug único
            var slugExists = await _db.Gyms
                .AnyAsync(g => g.Slug == normalizedSlug);

            if (slugExists)
                return BadRequest("Gym slug already exists.");

            // Verificar email único
            var emailExists = await _db.Users
                .AnyAsync(u => u.Email == normalizedEmail);

            if (emailExists)
                return BadRequest("Email already registered.");

            var gym = new Gym(req.GymName, normalizedSlug, normalizedEmail);
            await _db.Gyms.AddAsync(gym);

            var user = new User(
                gym.Id,
                req.AdminName,
                normalizedEmail,
                _hasher.Hash(req.Password),
                UserRole.Admin
            );

            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();

            var token = _jwt.GenerateToken(user.Id, gym.Id, user.Role.ToString());

            return Ok(new { token });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) ||
                string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest("Email and password are required.");
            }

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == req.Email);

            if (user == null)
                return Unauthorized();

            if (!_hasher.Verify(user.PasswordHash, req.Password))
                return Unauthorized();

            var token = _jwt.GenerateToken(user.Id, user.GymId, user.Role.ToString());

            return Ok(new { token });
        }
    }

    public record RegisterRequest(string GymName, string Slug, string Email, string AdminName, string AdminEmail, string Password);
    public record LoginRequest(string Email, string Password);
}
