using System;
using GymManagement.Api.Infrastructure.Tenant;

namespace GymManagement.Api.Features.Classes.Domain
{
    public class Class : ITenantEntity
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid GymId { get; set; }
        public string Title { get; private set; } = default!;
        public DateTime StartAt { get; private set; }
        public DateTime EndAt { get; private set; }
        public int Capacity { get; private set; }

        private Class() { }

        public Class(Guid gymId, string title, DateTime startAt, DateTime endAt, int capacity)
        {
            GymId = gymId;
            Title = title;
            StartAt = startAt;
            EndAt = endAt;
            Capacity = capacity;
        }
    }
}
