using MediatR;
using GymManagement.Api.Infrastructure.Common;
using GymManagement.Api.Features.Members.Domain;
using GymManagement.Api.Features.Memberships.Domain;
using GymManagement.Api.Infrastructure.Persistence;
using GymManagement.Api.Infrastructure.Tenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Api.Features.Memberships.Commands
{
    public record CreateMembershipCommand(
        [Required] Guid MemberId, 
        [Required] Guid MembershipTypeId, 
        DateTime? StartDate) : IRequest<Result<Membership>>;

    public class CreateMembershipCommandHandler : IRequestHandler<CreateMembershipCommand, Result<Membership>>
    {
        private readonly GymManagementDbContext _db;
        private readonly ITenantProvider _tenantProvider;

        public CreateMembershipCommandHandler(GymManagementDbContext db, ITenantProvider tenantProvider)
        {
            _db = db;
            _tenantProvider = tenantProvider;
        }

        public async Task<Result<Membership>> Handle(CreateMembershipCommand req, CancellationToken cancellationToken)
        {
            var member = await _db.Members.FindAsync(new object[] { req.MemberId }, cancellationToken);
            if (member == null) return Result<Membership>.Failure("Member not found", "MEMBER_NOT_FOUND");

            var type = await _db.MembershipTypes.FindAsync(new object[] { req.MembershipTypeId }, cancellationToken);
            if (type == null) return Result<Membership>.Failure("Membership type not found", "TYPE_NOT_FOUND");

            var startDate = req.StartDate ?? DateTime.UtcNow;
            var endDate = startDate.AddMonths(type.DurationMonths);

            var membership = new Membership(
                _tenantProvider.GymId,
                req.MemberId,
                req.MembershipTypeId,
                startDate,
                endDate,
                type.Price);

            await _db.Memberships.AddAsync(membership, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<Membership>.Success(membership);
        }
    }
}
