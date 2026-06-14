using MediatR;
using GymManagement.Api.Features.Memberships.Controllers;
using GymManagement.Api.Features.Members.Domain;
using GymManagement.Api.Infrastructure.Persistence;
using AutoMapper;

namespace GymManagement.Api.Features.Memberships.Queries
{
    public record GetMembershipQuery(Guid Id) : IRequest<MembershipResponse?>;

    public class GetMembershipQueryHandler : IRequestHandler<GetMembershipQuery, MembershipResponse?>
    {
        private readonly GymManagementDbContext _db;
        private readonly IMapper _mapper;

        public GetMembershipQueryHandler(GymManagementDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<MembershipResponse?> Handle(GetMembershipQuery request, CancellationToken cancellationToken)
        {
            var membership = await _db.Memberships.FindAsync(new object[] { request.Id }, cancellationToken);
            if (membership == null) return null;

            return _mapper.Map<MembershipResponse>(membership);
        }
    }
}
