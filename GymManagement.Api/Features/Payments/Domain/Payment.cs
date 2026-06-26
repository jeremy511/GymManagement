using System;
using GymManagement.Api.Features.Members.Domain;
using GymManagement.Api.Infrastructure.Tenant;

namespace GymManagement.Api.Features.Payments.Domain
{
    public enum PaymentMethod
    {
        Cash = 1,
        Card = 2,
        Transfer = 3
    }
    public class Payment : ITenantEntity
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid GymId { get; set; }
        public Guid MemberId { get; private set; }
        public decimal Amount { get; private set; }
        public DateTime PaidAt { get; private set; } = DateTime.UtcNow;
        public PaymentMethod Method { get; private set; }
        public string? ExternalReference { get; private set; }

        public virtual Member Member { get; private set; } = default!;

        private Payment() { }

        public Payment(Guid gymId, Guid memberId, decimal amount, PaymentMethod method, string? externalReference = null)
        {

            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

            GymId = gymId;
            MemberId = memberId;
            Amount = amount;
            Method = method;
            ExternalReference = externalReference;
        }
        public void Update(PaymentMethod method, string? externalReference)
        {
            Method = method;
            ExternalReference = externalReference;
        }
    }
}
