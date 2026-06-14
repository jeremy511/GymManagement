using System;

namespace GymManagement.Api.Features.Payments.Controllers
{
    public record PaymentResponse
    {
        public Guid Id { get; init; }
        public Guid MemberId { get; init; }
        public decimal Amount { get; init; }
        public DateTime PaidAt { get; init; }
        public string Method { get; init; } = default!;
        public string? ExternalReference { get; init; }
    }
}
