using MediatR;
using System.ComponentModel.DataAnnotations;
using GymManagement.Api.Infrastructure.Common;
using GymManagement.Api.Features.Classes.Domain;
using GymManagement.Api.Infrastructure.Persistence;
using GymManagement.Api.Infrastructure.Tenant;

namespace GymManagement.Api.Features.Classes.Commands
{
    public record CreateClassCommand(
        [Required][StringLength(100)] string Title, 
        [Required] DateTime StartAt, 
        [Required] DateTime EndAt, 
        [System.ComponentModel.DataAnnotations.Range(1, 1000)] int Capacity) : IRequest<Result<Class>>;

    public class CreateClassCommandHandler : IRequestHandler<CreateClassCommand, Result<Class>>
    {
        private readonly GymManagementDbContext _db;
        private readonly ITenantProvider _tenantProvider;

        public CreateClassCommandHandler(GymManagementDbContext db, ITenantProvider tenantProvider)
        {
            _db = db;
            _tenantProvider = tenantProvider;
        }

        public async Task<Result<Class>> Handle(CreateClassCommand req, CancellationToken cancellationToken)
        {
            var gymClass = new Class(
                _tenantProvider.GymId,
                req.Title,
                req.StartAt,
                req.EndAt,
                req.Capacity);

            await _db.Classes.AddAsync(gymClass, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<Class>.Success(gymClass);
        }
    }
}
