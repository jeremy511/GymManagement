using MediatR;
using GymManagement.Api.Features.Classes.Controllers;
using GymManagement.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace GymManagement.Api.Features.Classes.Queries
{
    public record GetClassReservationsQuery(Guid ClassId) : IRequest<List<ReservationResponse>>;

    public class GetClassReservationsQueryHandler : IRequestHandler<GetClassReservationsQuery, List<ReservationResponse>>
    {
        private readonly GymManagementDbContext _db;
        private readonly IMapper _mapper;

        public GetClassReservationsQueryHandler(GymManagementDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<ReservationResponse>> Handle(GetClassReservationsQuery request, CancellationToken cancellationToken)
        {
            var reservations = await _db.Reservations
                .Where(r => r.ClassId == request.ClassId)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<ReservationResponse>>(reservations);
        }
    }
}
