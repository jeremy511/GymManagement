using MediatR;
using GymManagement.Api.Infrastructure.Common;
using GymManagement.Api.Infrastructure.Persistence;
using GymManagement.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Api.Features.Auth.Commands
{
    public record LoginCommand(
        [Required][EmailAddress] string Email, 
        [Required] string Password) : IRequest<Result<AuthResponse>>;

    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
    {
        private readonly GymManagementDbContext _db;
        private readonly IPasswordHasher _hasher;
        private readonly IJwtService _jwt;

        public LoginCommandHandler(GymManagementDbContext db, IPasswordHasher hasher, IJwtService jwt)
        {
            _db = db;
            _hasher = hasher;
            _jwt = jwt;
        }

        public async Task<Result<AuthResponse>> Handle(LoginCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == req.Email, cancellationToken);

            if (user == null || !_hasher.Verify(user.PasswordHash, req.Password))
                return Result<AuthResponse>.Failure("Invalid email or password.", "INVALID_CREDENTIALS");

            var token = _jwt.GenerateToken(user.Id, user.GymId, user.Role.ToString());

            return Result<AuthResponse>.Success(new AuthResponse 
            { 
                UserId = user.Id, 
                GymId = user.GymId, 
                Email = user.Email, 
                Token = token 
            });
        }
    }
}
