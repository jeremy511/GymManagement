using MediatR;
using GymManagement.Api.Features.Memberships.Controllers;
using GymManagement.Api.Features.Members.Domain;
using GymManagement.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace GymManagement.Api.Features.Memberships.Queries
{
    public record ListMembershipsQuery(bool OnlyActive = false) : IRequest<List<MembershipResponse>>;

    public class ListMembershipsQueryHandler : IRequestHandler<ListMembershipsQuery, List<MembershipResponse>>
    {
        private readonly GymManagementDbContext _db;
        private readonly IMapper _mapper;

        public ListMembershipsQueryHandler(GymManagementDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<MembershipResponse>> Handle(ListMembershipsQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Memberships.AsQueryable();

            if (request.OnlyActive)
            {
                var now = DateTime.UtcNow;
                query = query.Where(m => m.StartDate <= now && m.EndDate >= now);
            }

            var memberships = await query.ToListAsync(cancellationToken);
            return _mapper.Map<List<MembershipResponse>>(memberships);
        }
    }
}
