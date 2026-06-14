using MediatR;
using GymManagement.Api.Features.Members.Controllers;
using GymManagement.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace GymManagement.Api.Features.Members.Queries
{
    public record ListMembersQuery(string? Search, int Page = 1, int PageSize = 10) : IRequest<ListMembersResult>;

    public record ListMembersResult(int Total, int Page, int PageSize, IEnumerable<MemberResponse> Data);

    public class ListMembersQueryHandler : IRequestHandler<ListMembersQuery, ListMembersResult>
    {
        private readonly GymManagementDbContext _db;
        private readonly IMapper _mapper;

        public ListMembersQueryHandler(GymManagementDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ListMembersResult> Handle(ListMembersQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var query = _db.Members.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                query = query.Where(m => 
                    m.FirstName.ToLower().Contains(s) || 
                    m.LastName.ToLower().Contains(s) || 
                    m.Email.ToLower().Contains(s));
            }

            var total = await query.CountAsync(cancellationToken);
            var membersList = await query
                .OrderBy(m => m.LastName)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(m => m.Memberships)
                .ToListAsync(cancellationToken);

            var members = membersList.Select(m => {
                var resp = _mapper.Map<MemberResponse>(m);
                return resp with { HasActiveMembership = m.Memberships.Any(ms => ms.StartDate <= now && ms.EndDate >= now) };
            }).ToList();

            return new ListMembersResult(total, request.Page, request.PageSize, members);
        }
    }
}
