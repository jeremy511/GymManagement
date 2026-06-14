using MediatR;
using GymManagement.Api.Infrastructure.Common;
using GymManagement.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymManagement.Api.Features.Classes.Commands
{
    public record CancelReservationCommand(Guid ReservationId) : IRequest<Result>;

    public class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommand, Result>
    {
        private readonly GymManagementDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CancelReservationCommandHandler(GymManagementDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Result> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
        {
            var reservation = await _db.Reservations.FindAsync(new object[] { request.ReservationId }, cancellationToken);
            if (reservation == null) return Result.Failure("Reservation not found", "NOT_FOUND");

            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst("userId")?.Value;
            var userRole = user?.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole == "Member" && (string.IsNullOrEmpty(userIdClaim) || Guid.Parse(userIdClaim) != reservation.MemberId))
                return Result.Failure("Unauthorized to cancel this reservation", "FORBIDDEN");

            _db.Reservations.Remove(reservation);
            await _db.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
