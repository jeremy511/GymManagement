using Microsoft.AspNetCore.Mvc;
using GymManagement.Api.Infrastructure.Persistence;
using GymManagement.Api.Features.Members.Domain;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Authorization;
using GymManagement.Api.Infrastructure.Tenant;

namespace GymManagement.Api.Features.Members.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")]
    public class MembersController : ControllerBase
    {
        #region Initialization
        private readonly GymManagementDbContext _db;
        private readonly ITenantProvider _tenantProvider;

        public MembersController(GymManagementDbContext db, ITenantProvider tenantProvider)
        {
            _db = db;
            _tenantProvider = tenantProvider;
        }
        #endregion

        #region Endpoints
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMemberRequest req)
        {
            // simple validation
            if (string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName))
                return BadRequest("Invalid name");

            var userIdClaim = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            var userId = Guid.Parse(userIdClaim);

            // Verificar si ya existe un perfil de miembro para este usuario
            if (await _db.Members.AnyAsync(m => m.Id == userId))
                return BadRequest("Member profile already exists for this user.");

            var member = new Member(userId, _tenantProvider.GymId, req.FirstName, req.LastName, req.Email);
            await _db.Members.AddAsync(member);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = member.Id }, member);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(Guid id)
        {
            var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == id);
            if (member == null) return NotFound();
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userId = Guid.Parse(User.FindFirst("userId")!.Value);

            if (userRole == "Member" && userId != id)
                return Forbid();
            return Ok(member);
        }
        #endregion
    }

    #region Requests
    public record CreateMemberRequest(string FirstName, string LastName, string Email);
    #endregion
}
