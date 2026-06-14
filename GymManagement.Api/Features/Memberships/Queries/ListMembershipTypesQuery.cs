using MediatR;
using GymManagement.Api.Features.Memberships.Controllers;
using GymManagement.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace GymManagement.Api.Features.Memberships.Queries
{
    public record ListMembershipTypesQuery() : IRequest<List<MembershipTypeResponse>>;

    public class ListMembershipTypesQueryHandler : IRequestHandler<ListMembershipTypesQuery, List<MembershipTypeResponse>>
    {
        private readonly GymManagementDbContext _db;
        private readonly IMapper _mapper;

        public ListMembershipTypesQueryHandler(GymManagementDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<MembershipTypeResponse>> Handle(ListMembershipTypesQuery request, CancellationToken cancellationToken)
        {
            var types = await _db.MembershipTypes.ToListAsync(cancellationToken);
            return _mapper.Map<List<MembershipTypeResponse>>(types);
        }
    }
}
