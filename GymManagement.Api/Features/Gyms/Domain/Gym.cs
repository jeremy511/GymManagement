using System;

namespace GymManagement.Api.Features.Gyms.Domain
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

        public void Update(string name, string email, string slug)
        {
            Name = name;
            Email = email;
            Slug = slug;
        }
    }
}
