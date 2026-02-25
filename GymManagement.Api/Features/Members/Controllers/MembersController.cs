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
    [Authorize]
    public class MembersController : ControllerBase
    {
        private readonly GymManagementDbContext _db;
        private readonly ITenantProvider _tenantProvider;

        public MembersController(GymManagementDbContext db, ITenantProvider tenantProvider)
        {
            _db = db;
            _tenantProvider = tenantProvider;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMemberRequest req)
        {
            // simple validation
            if (string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName))
                return BadRequest("Invalid name");

            var member = new Member(_tenantProvider.GymId, req.FirstName, req.LastName, req.Email);
            await _db.Members.AddAsync(member);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = member.Id }, member);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == id);
            if (member == null) return NotFound();
            return Ok(member);
        }
    }

    public record CreateMemberRequest(string FirstName, string LastName, string Email);
}
