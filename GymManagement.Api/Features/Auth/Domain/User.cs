namespace GymManagement.Api.Features.Auth.Domain
{

    public enum UserRole
    {
        Admin=1,
        Member=2,
        Staff= 3
    }
    public class User
    {

            public Guid Id { get; private set; } = Guid.NewGuid();
            public Guid GymId { get; private set; }
            public string Name { get; private set; } = default!;
            public string Email { get; private set; } = default!;
            public string PasswordHash { get; private set; } = default!;
            public UserRole Role { get; private set; }
            public bool IsActive { get; private set; } = true;
            public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

            private User() { }

            public User(Guid gymId, string name, string email, string passwordHash, UserRole role)
            {
                GymId = gymId;
                Name = name;
                Email = email;
                PasswordHash = passwordHash;
                Role = role;
            }
        }
}
