using MediatR;
using GymManagement.Api.Infrastructure.Common;
using GymManagement.Api.Features.Classes.Domain;
using GymManagement.Api.Infrastructure.Persistence;
using GymManagement.Api.Infrastructure.Tenant;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Api.Features.Classes.Commands
{
    public record ReserveClassCommand(Guid ClassId) : IRequest<Result<Reservation>>;

    public class ReserveClassCommandHandler : IRequestHandler<ReserveClassCommand, Result<Reservation>>
    {
        private readonly GymManagementDbContext _db;
        private readonly ITenantProvider _tenantProvider;
        private readonly Guid _memberId;

        public ReserveClassCommandHandler(GymManagementDbContext db, ITenantProvider tenantProvider, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _tenantProvider = tenantProvider;
            
            var userIdClaim = httpContextAccessor.HttpContext?.User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) throw new UnauthorizedAccessException();
            _memberId = Guid.Parse(userIdClaim);
        }

        public async Task<Result<Reservation>> Handle(ReserveClassCommand req, CancellationToken cancellationToken)
        {
            var gymClass = await _db.Classes.FindAsync(new object[] { req.ClassId }, cancellationToken);
            if (gymClass == null) return Result<Reservation>.Failure("Class not found", "NOT_FOUND");

            // Validation 1: Check for duplicate reservation (executes first)
            if (await _db.Reservations.AnyAsync(r => r.ClassId == req.ClassId && r.MemberId == _memberId, cancellationToken))
                return Result<Reservation>.Failure("Already reserved", "DuplicateReservation");

            // Validation 2: Check for active membership (executes second)
            var now = DateTime.UtcNow;
            var hasActiveMembership = await _db.Memberships
                .AnyAsync(m => m.MemberId == _memberId && m.StartDate <= now && m.EndDate >= now, cancellationToken);

            if (!hasActiveMembership)
                return Result<Reservation>.Failure("Usted no posee una membresía activa o su pago está vencido. Por favor, regularice su situación para reservar.", "NoActiveMembership");

            // Validation 3: Check capacity (executes third)
            var existingReservations = await _db.Reservations.CountAsync(r => r.ClassId == req.ClassId, cancellationToken);
            if (existingReservations >= gymClass.Capacity)
                return Result<Reservation>.Failure("Class is full", "ClassAtCapacity");

            var reservation = new Reservation(_tenantProvider.GymId, req.ClassId, _memberId);
            await _db.Reservations.AddAsync(reservation, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<Reservation>.Success(reservation);
        }
    }
}
