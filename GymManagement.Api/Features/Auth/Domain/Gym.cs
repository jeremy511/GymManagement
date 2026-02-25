namespace GymManagement.Api.Features.Auth.Domain
{
    public class Gym
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; private set; }
        public string Slug { get; private set; } = default!;
        public string Email { get; private set; } = default!;
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        private Gym() { }

        public Gym(string name, string slug, string email)
        {
            Name = name;
            Slug = slug;
            Email = email;
        }
    }
}
