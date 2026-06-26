using MediatR;
using GymManagement.Api.Features.Payments.Domain;
using GymManagement.Api.Infrastructure.Persistence;
using GymManagement.Api.Infrastructure.Tenant;
using Microsoft.EntityFrameworkCore;
using GymManagement.Api.Infrastructure.Common;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Api.Features.Payments.Commands
{
    public record CreatePaymentCommand(
        [Required] Guid MemberId,
        [System.ComponentModel.DataAnnotations.Range(0.01, 1000000)] decimal Amount,
        [Required] PaymentMethod Method,
        [StringLength(100)] string? ExternalReference) : IRequest<Result<Payment>>;

    public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Result<Payment>>
    {
        private readonly GymManagementDbContext _db;
        private readonly ITenantProvider _tenantProvider;

        public CreatePaymentCommandHandler(GymManagementDbContext db, ITenantProvider tenantProvider)
        {
            _db = db;
            _tenantProvider = tenantProvider;
        }

        public async Task<Result<Payment>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
        {
            if (_tenantProvider.GymId == Guid.Empty)
                return Result<Payment>.Failure("Gym context is missing.", "GYM_CONTEXT_MISSING");

            var memberExists = await _db.Members.AnyAsync(m => m.Id == request.MemberId, cancellationToken);
            if (!memberExists) return Result<Payment>.Failure("Member not found", "MEMBER_NOT_FOUND");

            var payment = new Payment(
                _tenantProvider.GymId,
                request.MemberId,
                request.Amount,
                request.Method,
                request.ExternalReference);

            await _db.Payments.AddAsync(payment, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<Payment>.Success(payment);
        }
    }
}
