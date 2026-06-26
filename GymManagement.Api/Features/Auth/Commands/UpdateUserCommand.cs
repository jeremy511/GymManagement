using MediatR;
using GymManagement.Api.Infrastructure.Common;
using GymManagement.Api.Features.Auth.Domain;
using GymManagement.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Api.Features.Auth.Commands
{
    public record UpdateUserCommand(
        Guid UserId,
        [Required][StringLength(100)] string Name,
        [Required][EmailAddress] string Email) : IRequest<Result<User>>;

    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<User>>
    {
        private readonly GymManagementDbContext _db;

        public UpdateUserCommandHandler(GymManagementDbContext db)
        {
            _db = db;
        }

        public async Task<Result<User>> Handle(UpdateUserCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);

            if (user == null)
                return Result<User>.Failure("User not found.", "NOT_FOUND");

            user.Update(req.Name, req.Email);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<User>.Success(user);
        }
    }
}
