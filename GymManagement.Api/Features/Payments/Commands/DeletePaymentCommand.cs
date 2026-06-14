using MediatR;
using GymManagement.Api.Infrastructure.Persistence;
using GymManagement.Api.Infrastructure.Common;

namespace GymManagement.Api.Features.Payments.Commands
{
    public record DeletePaymentCommand(Guid Id) : IRequest<Result>;

    public class DeletePaymentCommandHandler : IRequestHandler<DeletePaymentCommand, Result>
    {
        private readonly GymManagementDbContext _db;

        public DeletePaymentCommandHandler(GymManagementDbContext db)
        {
            _db = db;
        }

        public async Task<Result> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = await _db.Payments.FindAsync(new object[] { request.Id }, cancellationToken);
            if (payment == null) return Result.Failure("Payment not found", "NOT_FOUND");

            _db.Payments.Remove(payment);
            await _db.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
