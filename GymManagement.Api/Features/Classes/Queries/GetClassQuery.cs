using MediatR;
using GymManagement.Api.Features.Classes.Controllers;
using GymManagement.Api.Infrastructure.Persistence;
using AutoMapper;

namespace GymManagement.Api.Features.Classes.Queries
{
    public record GetClassQuery(Guid Id) : IRequest<ClassResponse?>;

    public class GetClassQueryHandler : IRequestHandler<GetClassQuery, ClassResponse?>
    {
        private readonly GymManagementDbContext _db;
        private readonly IMapper _mapper;

        public GetClassQueryHandler(GymManagementDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<ClassResponse?> Handle(GetClassQuery request, CancellationToken cancellationToken)
        {
            var gymClass = await _db.Classes.FindAsync(new object[] { request.Id }, cancellationToken);
            if (gymClass == null) return null;

            return _mapper.Map<ClassResponse>(gymClass);
        }
    }
}
