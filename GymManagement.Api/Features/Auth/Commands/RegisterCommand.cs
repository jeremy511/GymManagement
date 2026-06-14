using MediatR;
using GymManagement.Api.Infrastructure.Common;
using GymManagement.Api.Features.Auth.Domain;
using GymManagement.Api.Features.Gyms.Domain;
using GymManagement.Api.Infrastructure.Persistence;
using GymManagement.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Api.Features.Auth.Commands
{
    public record AuthResponse
    {
        public Guid UserId { get; init; }
        public Guid GymId { get; init; }
        public string Email { get; init; } = default!;
        public string Token { get; init; } = default!;
    }

    public record RegisterCommand(
        [Required][StringLength(100)] string GymName, 
        [Required][StringLength(50)] string Slug, 
        [Required][EmailAddress] string Email, 
        [Required][StringLength(100)] string AdminName, 
        [Required][EmailAddress] string AdminEmail, 
        [Required][MinLength(6)] string Password) : IRequest<Result<AuthResponse>>;

    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
    {
        private readonly GymManagementDbContext _db;
        private readonly IPasswordHasher _hasher;
        private readonly IJwtService _jwt;

        public RegisterCommandHandler(GymManagementDbContext db, IPasswordHasher hasher, IJwtService jwt)
        {
            _db = db;
            _hasher = hasher;
            _jwt = jwt;
        }

        public async Task<Result<AuthResponse>> Handle(RegisterCommand req, CancellationToken cancellationToken)
        {
            var normalizedEmail = req.AdminEmail.ToLower();
            var normalizedSlug = req.Slug.ToLower();

            if (await _db.Gyms.AnyAsync(g => g.Slug == normalizedSlug, cancellationToken))
                return Result<AuthResponse>.Failure("Gym slug already exists.", "SLUG_EXISTS");

            if (await _db.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == normalizedEmail, cancellationToken))
                return Result<AuthResponse>.Failure("Email already registered.", "EMAIL_EXISTS");

            var gym = new Gym(req.GymName, normalizedSlug, normalizedEmail);
            await _db.Gyms.AddAsync(gym, cancellationToken);

            var user = new User(
                gym.Id,
                req.AdminName,
                normalizedEmail,
                _hasher.Hash(req.Password),
                UserRole.Admin
            );

            await _db.Users.AddAsync(user, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            var token = _jwt.GenerateToken(user.Id, gym.Id, user.Role.ToString());

            return Result<AuthResponse>.Success(new AuthResponse 
            { 
                UserId = user.Id, 
                GymId = gym.Id, 
                Email = user.Email, 
                Token = token 
            });
        }
    }
}
