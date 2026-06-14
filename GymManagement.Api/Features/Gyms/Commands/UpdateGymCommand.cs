using MediatR;
using GymManagement.Api.Infrastructure.Common;
using GymManagement.Api.Features.Gyms.Domain;
using GymManagement.Api.Infrastructure.Persistence;
using GymManagement.Api.Infrastructure.Tenant;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Api.Features.Gyms.Commands
{
    public record UpdateGymCommand(
        [Required][StringLength(100)] string Name, 
        [Required][EmailAddress] string Email, 
        [Required][StringLength(50)] string Slug) : IRequest<Result<Gym>>;

    public class UpdateGymCommandHandler : IRequestHandler<UpdateGymCommand, Result<Gym>>
    {
        private readonly GymManagementDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public UpdateGymCommandHandler(GymManagementDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        public async Task<Result<Gym>> Handle(UpdateGymCommand request, CancellationToken cancellationToken)
        {
            var gymId = _tenantProvider.GymId;
            if (gymId == Guid.Empty) return Result<Gym>.Failure("Tenant not found", "NOT_FOUND");

            var gym = await _context.Gyms.FirstOrDefaultAsync(g => g.Id == gymId, cancellationToken);
            if (gym == null) return Result<Gym>.Failure("Gym not found", "NOT_FOUND");

            gym.Update(request.Name, request.Email, request.Slug);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Gym>.Success(gym);
        }
    }
}
