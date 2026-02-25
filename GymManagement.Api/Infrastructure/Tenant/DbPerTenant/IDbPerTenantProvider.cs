using System;

namespace GymManagement.Api.Infrastructure.Tenant.DbPerTenant
{
    public interface IDbPerTenantProvider
    {
        string GetConnectionStringFor(Guid gymId);
    }
}
