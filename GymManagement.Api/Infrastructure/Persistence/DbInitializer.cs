using GymManagement.Api.Features.Auth.Domain;
using GymManagement.Api.Features.Members.Domain;
using GymManagement.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Api.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(GymManagementDbContext db, IPasswordHasher hasher)
    {
        // 1. Verificar si ya hay datos para no duplicar
        if (await db.Gyms.AnyAsync())
        {
            return;
        }

        #region Create Initial Gym
        var gym = new Gym("Iron Temple", "iron-temple", "contact@irontemple.com");
        await db.Gyms.AddAsync(gym);
        #endregion

        #region Create Admin User
        var adminPassword = hasher.Hash("Admin123!");
        var adminUser = new User(
            gym.Id,
            "Admin User",
            "admin@irontemple.com",
            adminPassword,
            UserRole.Admin
        );
        await db.Users.AddAsync(adminUser);
        #endregion

        #region Create Sample Members (Shared ID Pattern)
        // Miembro 1
        var member1Email = "juan.perez@email.com";
        var member1User = new User(
            gym.Id,
            "Juan Perez",
            member1Email,
            hasher.Hash("Member123!"),
            UserRole.Member
        );
        await db.Users.AddAsync(member1User);

        var member1Profile = new Member(
            member1User.Id, 
            gym.Id, 
            "Juan", 
            "Perez", 
            member1Email
        );
        await db.Members.AddAsync(member1Profile);

        // Miembro 2
        var member2Email = "maria.garcia@email.com";
        var member2User = new User(
            gym.Id,
            "Maria Garcia",
            member2Email,
            hasher.Hash("Member123!"),
            UserRole.Member
        );
        await db.Users.AddAsync(member2User);

        var member2Profile = new Member(
            member2User.Id, 
            gym.Id, 
            "Maria", 
            "Garcia", 
            member2Email
        );
        await db.Members.AddAsync(member2Profile);
        #endregion

        await db.SaveChangesAsync();
    }
}
