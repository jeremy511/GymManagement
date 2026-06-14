using MediatR;
using GymManagement.Api.Features.Members.Controllers;
using GymManagement.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace GymManagement.Api.Features.Members.Queries
{
    public record GetMemberQuery(Guid Id) : IRequest<MemberResponse?>;

    public class GetMemberQueryHandler : IRequestHandler<GetMemberQuery, MemberResponse?>
    {
        private readonly GymManagementDbContext _db;
        private readonly IMapper _mapper;

        public GetMemberQueryHandler(GymManagementDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<MemberResponse?> Handle(GetMemberQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var memberEntity = await _db.Members
                .Include(m => m.Memberships)
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (memberEntity == null) return null;

            var response = _mapper.Map<MemberResponse>(memberEntity);
            return response with {
                HasActiveMembership = memberEntity.Memberships.Any(ms => ms.StartDate <= now && ms.EndDate >= now)
            };
        }
    }
}
