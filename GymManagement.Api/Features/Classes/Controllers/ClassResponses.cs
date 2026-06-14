using System;

namespace GymManagement.Api.Features.Classes.Controllers
{
    public record ClassResponse
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = default!;
        public DateTime StartAt { get; init; }
        public DateTime EndAt { get; init; }
        public int Capacity { get; init; }
        public int RegisteredCount { get; init; }
    }

    public record ReservationResponse
    {
        public Guid Id { get; init; }
        public Guid ClassId { get; init; }
        public Guid MemberId { get; init; }
        public DateTime ReservedAt { get; init; }
    }
}
