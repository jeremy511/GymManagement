using MediatR;
using GymManagement.Api.Features.Members.Domain;
using GymManagement.Api.Infrastructure.Persistence;
using GymManagement.Api.Infrastructure.Tenant;
using Microsoft.EntityFrameworkCore;
using GymManagement.Api.Infrastructure.Common;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Api.Features.Members.Commands
{
    public record CreateMemberCommand(
        [Required][StringLength(100)] string FirstName, 
        [Required][StringLength(100)] string LastName, 
        [Required][EmailAddress] string Email) : IRequest<Result<Member>>;

    public class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, Result<Member>>
    {
        private readonly GymManagementDbContext _db;
        private readonly ITenantProvider _tenantProvider;
        private readonly Guid _userId;

        public CreateMemberCommandHandler(GymManagementDbContext db, ITenantProvider tenantProvider, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _tenantProvider = tenantProvider;
            
            var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) throw new UnauthorizedAccessException();
            _userId = Guid.Parse(userIdClaim);
        }

        public async Task<Result<Member>> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
        {
            if (await _db.Members.AnyAsync(m => m.Id == _userId, cancellationToken))
                return Result<Member>.Failure("Member profile already exists for this user.", "ALREADY_EXISTS");

            var member = new Member(_userId, _tenantProvider.GymId, request.FirstName, request.LastName, request.Email);
            await _db.Members.AddAsync(member, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<Member>.Success(member);
        }
    }
}
