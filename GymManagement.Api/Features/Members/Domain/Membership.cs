using System;
using GymManagement.Api.Features.Memberships.Domain;
using GymManagement.Api.Infrastructure.Tenant;

namespace GymManagement.Api.Features.Members.Domain
{
    public class Membership : ITenantEntity
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid GymId { get; set; }
        public Guid MemberId { get; private set; }
        public Guid MembershipTypeId { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public decimal PricePaid { get; private set; }

        public virtual Member Member { get; private set; } = default!;
        public virtual MembershipType MembershipType { get; private set; } = default!;

        private Membership() { }

        public Membership(Guid gymId, Guid memberId, Guid membershipTypeId, DateTime startDate, DateTime endDate, decimal pricePaid)
        {
            GymId = gymId;
            MemberId = memberId;
            MembershipTypeId = membershipTypeId;
            StartDate = startDate;
            EndDate = endDate;
            PricePaid = pricePaid;
        }
    }
}
