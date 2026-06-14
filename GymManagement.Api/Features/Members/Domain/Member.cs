using System;
using System.Collections.Generic;
using GymManagement.Api.Features.Classes.Domain;
using GymManagement.Api.Features.Payments.Domain;
using GymManagement.Api.Infrastructure.Persistence;
using GymManagement.Api.Infrastructure.Tenant;

namespace GymManagement.Api.Features.Members.Domain
{
    public class Member : ITenantEntity, ISoftDeletable
    {
        public Guid Id { get; private set; }
        public Guid GymId { get; set; }
        public string FirstName { get; private set; } = default!;
        public string LastName { get; private set; } = default!;
        public string Email { get; private set; } = default!;
        public bool IsActive { get; private set; } = true;
        public DateTime JoinedAt { get; private set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }

        public virtual ICollection<Membership> Memberships { get; private set; } = new List<Membership>();
        public virtual ICollection<Reservation> Reservations { get; private set; } = new List<Reservation>();
        public virtual ICollection<Payment> Payments { get; private set; } = new List<Payment>();

        private Member() { }

        public Member(Guid id, Guid gymId, string firstName, string lastName, string email)
        {
            Id = id;
            GymId = gymId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }
        public void Update(string firstName, string lastName, string email)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }
    }
}
