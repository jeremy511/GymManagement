using System;
using GymManagement.Api.Infrastructure.Tenant;

namespace GymManagement.Api.Features.Members.Domain
{
    public class Membership : ITenantEntity
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid GymId { get; set; }
        public Guid MemberId { get; private set; }
        public string Type { get; private set; } = default!;
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
        public decimal Price { get; private set; }

        private Membership() { }

        public Membership(Guid gymId, Guid memberId, string type, DateTime start, DateTime end, decimal price)
        {
            GymId = gymId;
            MemberId = memberId;
            Type = type;
            Start = start;
            End = end;
            Price = price;
        }
    }
}
