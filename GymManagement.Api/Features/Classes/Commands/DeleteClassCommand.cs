using MediatR;
using GymManagement.Api.Infrastructure.Common;
using GymManagement.Api.Infrastructure.Persistence;

namespace GymManagement.Api.Features.Classes.Commands
{
    public record DeleteClassCommand(Guid Id) : IRequest<Result>;

    public class DeleteClassCommandHandler : IRequestHandler<DeleteClassCommand, Result>
    {
        private readonly GymManagementDbContext _db;

        public DeleteClassCommandHandler(GymManagementDbContext db)
        {
            _db = db;
        }

        public async Task<Result> Handle(DeleteClassCommand request, CancellationToken cancellationToken)
        {
            var gymClass = await _db.Classes.FindAsync(new object[] { request.Id }, cancellationToken);
            if (gymClass == null) return Result.Failure("Class not found", "NOT_FOUND");

            _db.Classes.Remove(gymClass);
            await _db.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
