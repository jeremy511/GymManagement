using MediatR;
using GymManagement.Api.Infrastructure.Common;
using GymManagement.Api.Features.Memberships.Domain;
using GymManagement.Api.Infrastructure.Persistence;
using GymManagement.Api.Infrastructure.Tenant;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Api.Features.Memberships.Commands
{
    public record CreateMembershipTypeCommand(
        [Required][StringLength(100)] string Name, 
        [Required][StringLength(500)] string Description, 
        [System.ComponentModel.DataAnnotations.Range(0, 1000000)] decimal Price, 
        [System.ComponentModel.DataAnnotations.Range(1, 120)] int DurationMonths) : IRequest<Result<MembershipType>>;

    public class CreateMembershipTypeCommandHandler : IRequestHandler<CreateMembershipTypeCommand, Result<MembershipType>>
    {
        private readonly GymManagementDbContext _db;
        private readonly ITenantProvider _tenantProvider;

        public CreateMembershipTypeCommandHandler(GymManagementDbContext db, ITenantProvider tenantProvider)
        {
            _db = db;
            _tenantProvider = tenantProvider;
        }

        public async Task<Result<MembershipType>> Handle(CreateMembershipTypeCommand req, CancellationToken cancellationToken)
        {
            var type = new MembershipType(
                _tenantProvider.GymId,
                req.Name,
                req.Description,
                req.Price,
                req.DurationMonths);

            await _db.MembershipTypes.AddAsync(type, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<MembershipType>.Success(type);
        }
    }
}
