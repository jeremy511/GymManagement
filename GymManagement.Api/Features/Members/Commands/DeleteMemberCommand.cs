using MediatR;
using GymManagement.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using GymManagement.Api.Infrastructure.Common;

namespace GymManagement.Api.Features.Members.Commands
{
    public record DeleteMemberCommand(Guid Id) : IRequest<Result>;

    public class DeleteMemberCommandHandler : IRequestHandler<DeleteMemberCommand, Result>
    {
        private readonly GymManagementDbContext _db;

        public DeleteMemberCommandHandler(GymManagementDbContext db)
        {
            _db = db;
        }

        public async Task<Result> Handle(DeleteMemberCommand request, CancellationToken cancellationToken)
        {
            var member = await _db.Members.FindAsync(new object[] { request.Id }, cancellationToken);
            if (member == null) return Result.Failure("Member not found", "NOT_FOUND");

            _db.Members.Remove(member);
            await _db.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
