using MediatR;
using GymManagement.Api.Features.Memberships.Controllers;
using GymManagement.Api.Features.Members.Domain;
using GymManagement.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace GymManagement.Api.Features.Memberships.Queries
{
    public record ListMembershipsByMemberQuery(Guid MemberId) : IRequest<List<MembershipResponse>>;

    public class ListMembershipsByMemberQueryHandler : IRequestHandler<ListMembershipsByMemberQuery, List<MembershipResponse>>
    {
        private readonly GymManagementDbContext _db;
        private readonly IMapper _mapper;

        public ListMembershipsByMemberQueryHandler(GymManagementDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<MembershipResponse>> Handle(ListMembershipsByMemberQuery request, CancellationToken cancellationToken)
        {
            var memberships = await _db.Memberships
                .Where(m => m.MemberId == request.MemberId)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<MembershipResponse>>(memberships);
        }
    }
}
