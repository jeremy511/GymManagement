using System;

namespace GymManagement.Api.Infrastructure.Tenant
{
    public interface ITenantProvider
    {
        Guid GymId { get; }
    }
}
