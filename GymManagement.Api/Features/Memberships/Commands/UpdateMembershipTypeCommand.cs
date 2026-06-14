using MediatR;
using GymManagement.Api.Infrastructure.Common;
using GymManagement.Api.Features.Memberships.Domain;
using GymManagement.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Api.Features.Memberships.Commands
{
    public record UpdateMembershipTypeCommand(
        Guid MembershipTypeId,
        [Required][StringLength(100)] string Name,
        [Required] string Description,
        [Required][Range(0, 1000000)] decimal Price,
        [Required][Range(1, 120)] int DurationMonths) : IRequest<Result<MembershipType>>;

    public class UpdateMembershipTypeCommandHandler : IRequestHandler<UpdateMembershipTypeCommand, Result<MembershipType>>
    {
        private readonly GymManagementDbContext _db;

        public UpdateMembershipTypeCommandHandler(GymManagementDbContext db)
        {
            _db = db;
        }

        public async Task<Result<MembershipType>> Handle(UpdateMembershipTypeCommand req, CancellationToken cancellationToken)
        {
            var membershipType = await _db.MembershipTypes
                .FirstOrDefaultAsync(mt => mt.Id == req.MembershipTypeId, cancellationToken);

            if (membershipType == null)
                return Result<MembershipType>.Failure("Membership type not found.", "NOT_FOUND");

            membershipType.Update(req.Name, req.Description, req.Price, req.DurationMonths);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<MembershipType>.Success(membershipType);
        }
    }
}
