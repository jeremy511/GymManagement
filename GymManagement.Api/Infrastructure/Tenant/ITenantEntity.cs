using System;

namespace GymManagement.Api.Infrastructure.Tenant
{
    // Entities that are tenant-scoped implement this to allow DbContext to auto-assign GymId
    public interface ITenantEntity
    {
        Guid GymId { get; set; }
    }
}
