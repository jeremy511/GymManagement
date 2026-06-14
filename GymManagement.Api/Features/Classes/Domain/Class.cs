using System;
using System.Collections.Generic;
using GymManagement.Api.Infrastructure.Persistence;
using GymManagement.Api.Infrastructure.Tenant;

namespace GymManagement.Api.Features.Classes.Domain
{
    public class Class : ITenantEntity, ISoftDeletable
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid GymId { get; set; }
        public string Title { get; private set; } = default!;
        public DateTime StartAt { get; private set; }
        public DateTime EndAt { get; private set; }
        public int Capacity { get; private set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<Reservation> Reservations { get; private set; } = new List<Reservation>();

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
