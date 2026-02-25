using System;
using System.Collections.Concurrent;

namespace GymManagement.Api.Infrastructure.Tenant.DbPerTenant
{
    // PoC provider that stores connection strings in-memory per gymId.
    public class InMemoryDbPerTenantProvider : IDbPerTenantProvider
    {
        private readonly ConcurrentDictionary<Guid, string> _map = new();

        public void Set(Guid gymId, string connectionString) => _map[gymId] = connectionString;

        public string GetConnectionStringFor(Guid gymId)
        {
            if (_map.TryGetValue(gymId, out var cs)) return cs;
            throw new InvalidOperationException("No connection string configured for tenant");
        }
    }
}
