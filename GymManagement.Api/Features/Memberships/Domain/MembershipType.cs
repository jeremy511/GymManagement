using System;
using System.Collections.Generic;
using GymManagement.Api.Features.Members.Domain;
using GymManagement.Api.Infrastructure.Persistence;
using GymManagement.Api.Infrastructure.Tenant;

namespace GymManagement.Api.Features.Memberships.Domain
{
    public class MembershipType : ITenantEntity, ISoftDeletable
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid GymId { get; set; }
        public string Name { get; private set; } = default!;
        public string Description { get; private set; } = default!;
        public decimal Price { get; private set; }
        public int DurationMonths { get; private set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<Membership> Memberships { get; private set; } = new List<Membership>();

        private MembershipType() { }

        public MembershipType(Guid gymId, string name, string description, decimal price, int durationMonths)
        {
            GymId = gymId;
            Name = name;
            Description = description;
            Price = price;
            DurationMonths = durationMonths;
        }
        public void Update(string name, string description, decimal price, int durationMonths)
        {
            Name = name;
            Description = description;
            Price = price;
            DurationMonths = durationMonths;
        }
    }
}
