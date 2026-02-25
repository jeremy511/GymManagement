using GymManagement.Api.Features.Auth.Domain;
using GymManagement.Api.Infrastructure.Tenant;
using Microsoft.EntityFrameworkCore;
using System;

namespace GymManagement.Api.Infrastructure.Persistence;

public class GymManagementDbContext : DbContext
{
    private readonly ITenantProvider? _tenantProvider;

    public GymManagementDbContext(DbContextOptions<GymManagementDbContext> options, ITenantProvider? tenantProvider = null)
        : base(options)
    {
        _tenantProvider = tenantProvider;
        CurrentGymId = _tenantProvider?.GymId ?? Guid.Empty;
    }

    // CurrentGymId is used by query filters. When empty, filters allow all (useful for migrations/admins).
    public Guid CurrentGymId { get; private set; }

    public DbSet<Gym> Gyms => Set<Gym>();
    public DbSet<User> Users => Set<User>();
    public DbSet<GymManagement.Api.Features.Members.Domain.Member> Members => Set<GymManagement.Api.Features.Members.Domain.Member>();
    public DbSet<GymManagement.Api.Features.Members.Domain.Membership> Memberships => Set<GymManagement.Api.Features.Members.Domain.Membership>();
    public DbSet<GymManagement.Api.Features.Payments.Domain.Payment> Payments => Set<GymManagement.Api.Features.Payments.Domain.Payment>();
    public DbSet<GymManagement.Api.Features.Classes.Domain.Class> Classes => Set<GymManagement.Api.Features.Classes.Domain.Class>();
    public DbSet<GymManagement.Api.Features.Classes.Domain.Reservation> Reservations => Set<GymManagement.Api.Features.Classes.Domain.Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Example global query filter for tenant-scoped entities that have a GymId property.
        // When CurrentGymId == Guid.Empty the filter is disabled (useful for admin operations and migrations).
        modelBuilder.Entity<User>().HasQueryFilter(u => CurrentGymId == Guid.Empty || u.GymId == CurrentGymId);

        modelBuilder.Entity<GymManagement.Api.Features.Members.Domain.Member>().HasQueryFilter(m => CurrentGymId == Guid.Empty || m.GymId == CurrentGymId);
        modelBuilder.Entity<GymManagement.Api.Features.Members.Domain.Membership>().HasQueryFilter(m => CurrentGymId == Guid.Empty || m.GymId == CurrentGymId);
        modelBuilder.Entity<GymManagement.Api.Features.Payments.Domain.Payment>().HasQueryFilter(p => CurrentGymId == Guid.Empty || p.GymId == CurrentGymId);
        modelBuilder.Entity<GymManagement.Api.Features.Classes.Domain.Class>().HasQueryFilter(c => CurrentGymId == Guid.Empty || c.GymId == CurrentGymId);
        modelBuilder.Entity<GymManagement.Api.Features.Classes.Domain.Reservation>().HasQueryFilter(r => CurrentGymId == Guid.Empty || r.GymId == CurrentGymId);

        // Configure other entity mappings here
    }

    public override int SaveChanges()
    {
        AssignTenantToNewEntities();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AssignTenantToNewEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void AssignTenantToNewEntities()
    {
        if (CurrentGymId == Guid.Empty) return;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added && entry.Entity is ITenantEntity tenantEntity)
            {
                tenantEntity.GymId = CurrentGymId;
            }
        }
    }
}

