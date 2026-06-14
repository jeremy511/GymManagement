using System;

namespace GymManagement.Api.Features.Memberships.Controllers
{
    public record MembershipTypeResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = default!;
        public string Description { get; init; } = default!;
        public decimal Price { get; init; }
        public int DurationMonths { get; init; }
    }

    public record MembershipResponse
    {
        public Guid Id { get; init; }
        public Guid MemberId { get; init; }
        public Guid MembershipTypeId { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndAt { get; init; }
        public decimal PricePaid { get; init; }
    }
}
