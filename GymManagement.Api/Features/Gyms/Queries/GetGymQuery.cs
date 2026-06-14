using MediatR;
using GymManagement.Api.Infrastructure.Common;
using GymManagement.Api.Features.Gyms.Domain;
using GymManagement.Api.Infrastructure.Persistence;
using GymManagement.Api.Infrastructure.Tenant;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Api.Features.Gyms.Queries
{
    public record GetGymQuery() : IRequest<Result<Gym>>;

    public class GetGymQueryHandler : IRequestHandler<GetGymQuery, Result<Gym>>
    {
        private readonly GymManagementDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public GetGymQueryHandler(GymManagementDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<Result<Gym>> Handle(GetGymQuery request, CancellationToken cancellationToken)
        {
            var gymId = _tenantProvider.GymId;
            if (gymId == Guid.Empty) return Result<Gym>.Failure("Tenant not found", "NOT_FOUND");

            var gym = await _context.Gyms.FirstOrDefaultAsync(g => g.Id == gymId, cancellationToken);
            
            return gym != null 
                ? Result<Gym>.Success(gym) 
                : Result<Gym>.Failure("Gym not found", "NOT_FOUND");
        }
    }
}
