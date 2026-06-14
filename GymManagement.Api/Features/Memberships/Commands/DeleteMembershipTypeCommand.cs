using MediatR;
using GymManagement.Api.Infrastructure.Common;
using GymManagement.Api.Infrastructure.Persistence;

namespace GymManagement.Api.Features.Memberships.Commands
{
    public record DeleteMembershipTypeCommand(Guid Id) : IRequest<Result>;

    public class DeleteMembershipTypeCommandHandler : IRequestHandler<DeleteMembershipTypeCommand, Result>
    {
        private readonly GymManagementDbContext _db;

        public DeleteMembershipTypeCommandHandler(GymManagementDbContext db)
        {
            _db = db;
        }

        public async Task<Result> Handle(DeleteMembershipTypeCommand request, CancellationToken cancellationToken)
        {
            var type = await _db.MembershipTypes.FindAsync(new object[] { request.Id }, cancellationToken);
            if (type == null) return Result.Failure("Membership type not found", "NOT_FOUND");

            _db.MembershipTypes.Remove(type);
            await _db.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
