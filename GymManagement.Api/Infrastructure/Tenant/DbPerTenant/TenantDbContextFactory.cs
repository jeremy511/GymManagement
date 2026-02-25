using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using GymManagement.Api.Infrastructure.Persistence;

namespace GymManagement.Api.Infrastructure.Tenant.DbPerTenant
{
    // PoC factory that creates DbContextOptions using a per-tenant connection string
    public class TenantDbContextFactory
    {
        private readonly IDbPerTenantProvider _provider;

        public TenantDbContextFactory(IDbPerTenantProvider provider)
        {
            _provider = provider;
        }

        public GymManagementDbContext Create(Guid gymId)
        {
            var cs = _provider.GetConnectionStringFor(gymId);
            var options = new DbContextOptionsBuilder<GymManagementDbContext>()
                .UseSqlServer(cs)
                .Options;

            return new GymManagementDbContext(options);
        }
    }
}
