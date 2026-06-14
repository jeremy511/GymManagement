using System;
using GymManagement.Api.Features.Members.Domain;
using GymManagement.Api.Infrastructure.Tenant;

namespace GymManagement.Api.Features.Classes.Domain
{
    public class Reservation : ITenantEntity
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid GymId { get; set; }
        public Guid ClassId { get; private set; }
        public Guid MemberId { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public virtual Member Member { get; private set; } = default!;
        public virtual Class Class { get; private set; } = default!;

        private Reservation() { }

        public Reservation(Guid gymId, Guid classId, Guid memberId)
        {
            GymId = gymId;
            ClassId = classId;
            MemberId = memberId;
        }
    }
}
