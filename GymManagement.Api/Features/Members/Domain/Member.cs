using System;
using GymManagement.Api.Infrastructure.Tenant;

namespace GymManagement.Api.Features.Members.Domain
{
    public class Member : ITenantEntity
    {
        public Guid Id { get; private set; }
        public Guid GymId { get; set; }
        public string FirstName { get; private set; } = default!;
        public string LastName { get; private set; } = default!;
        public string Email { get; private set; } = default!;
        public bool IsActive { get; private set; } = true;
        public DateTime JoinedAt { get; private set; } = DateTime.UtcNow;

        private Member() { }

        public Member(Guid id, Guid gymId, string firstName, string lastName, string email)
        {
            Id = id;
            GymId = gymId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }
    }
}
