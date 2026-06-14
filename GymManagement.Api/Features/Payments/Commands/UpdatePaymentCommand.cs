using MediatR;
using GymManagement.Api.Infrastructure.Common;
using GymManagement.Api.Features.Payments.Domain;
using GymManagement.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Api.Features.Payments.Commands
{
    public record UpdatePaymentCommand(
        Guid PaymentId,
        [Required][StringLength(50)] string Method,
        [StringLength(100)] string? ExternalReference) : IRequest<Result<Payment>>;

    public class UpdatePaymentCommandHandler : IRequestHandler<UpdatePaymentCommand, Result<Payment>>
    {
        private readonly GymManagementDbContext _db;

        public UpdatePaymentCommandHandler(GymManagementDbContext db)
        {
            _db = db;
        }

        public async Task<Result<Payment>> Handle(UpdatePaymentCommand req, CancellationToken cancellationToken)
        {
            var payment = await _db.Payments
                .FirstOrDefaultAsync(p => p.Id == req.PaymentId, cancellationToken);

            if (payment == null)
                return Result<Payment>.Failure("Payment not found.", "NOT_FOUND");

            payment.Update(req.Method, req.ExternalReference);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<Payment>.Success(payment);
        }
    }
}
