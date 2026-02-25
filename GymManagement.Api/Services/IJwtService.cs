using System;

namespace GymManagement.Api.Services
{
    public interface IJwtService
    {
        string GenerateToken(Guid userId, Guid gymId, string role, int expiresMinutes = 60);
    }
}
