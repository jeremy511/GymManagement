using System;
using GymManagement.Api.Infrastructure.Tenant;

namespace GymManagement.Api.Features.Payments.Domain
{
    public class Payment : ITenantEntity
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid GymId { get; set; }
        public Guid MemberId { get; private set; }
        public decimal Amount { get; private set; }
        public DateTime PaidAt { get; private set; } = DateTime.UtcNow;
        public string Method { get; private set; } = default!;
        public string? ExternalReference { get; private set; }

        private Payment() { }

        public Payment(Guid gymId, Guid memberId, decimal amount, string method, string? externalReference = null)
        {
            GymId = gymId;
            MemberId = memberId;
            Amount = amount;
            Method = method;
            ExternalReference = externalReference;
        }
    }
}
