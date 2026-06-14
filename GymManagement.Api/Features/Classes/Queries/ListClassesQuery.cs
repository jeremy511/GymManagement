using MediatR;
using GymManagement.Api.Infrastructure.Common;
using GymManagement.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using GymManagement.Api.Features.Classes.Controllers;

namespace GymManagement.Api.Features.Classes.Queries
{
    public record ListClassesQuery() : IRequest<List<ClassResponse>>;

    public class ListClassesQueryHandler : IRequestHandler<ListClassesQuery, List<ClassResponse>>
    {
        private readonly GymManagementDbContext _db;
        private readonly IMapper _mapper;

        public ListClassesQueryHandler(GymManagementDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<ClassResponse>> Handle(ListClassesQuery request, CancellationToken cancellationToken)
        {
            var classes = await _db.Classes
                .Include(c => c.Reservations)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<ClassResponse>>(classes);
        }
    }
}
