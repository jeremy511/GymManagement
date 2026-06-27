using GymManagement.Api.Features.Auth.Domain;
using GymManagement.Api.Features.Gyms.Domain;
using GymManagement.Api.Features.Memberships.Domain;
using GymManagement.Api.Infrastructure.Tenant;
using Microsoft.EntityFrameworkCore;
using System;

namespace GymManagement.Api.Infrastructure.Persistence;

public class GymManagementDbContext : DbContext
{
    #region Configuration & Constructor
    private readonly ITenantProvider? _tenantProvider;

    public GymManagementDbContext(DbContextOptions<GymManagementDbContext> options, ITenantProvider? tenantProvider = null)
        : base(options)
    {
        _tenantProvider = tenantProvider;
        CurrentGymId = _tenantProvider?.GymId ?? Guid.Empty;
    }

    // CurrentGymId is used by query filters. When empty, filters allow all (useful for migrations/admins).
    public Guid CurrentGymId { get; private set; }
    #endregion

    #region DbSets
    public DbSet<Gym> Gyms => Set<Gym>();
    public DbSet<User> Users => Set<User>();
    public DbSet<GymManagement.Api.Features.Members.Domain.Member> Members => Set<GymManagement.Api.Features.Members.Domain.Member>();
    public DbSet<GymManagement.Api.Features.Members.Domain.Membership> Memberships => Set<GymManagement.Api.Features.Members.Domain.Membership>();
    public DbSet<MembershipType> MembershipTypes => Set<MembershipType>();
    public DbSet<GymManagement.Api.Features.Payments.Domain.Payment> Payments => Set<GymManagement.Api.Features.Payments.Domain.Payment>();
    public DbSet<GymManagement.Api.Features.Classes.Domain.Class> Classes => Set<GymManagement.Api.Features.Classes.Domain.Class>();
    public DbSet<GymManagement.Api.Features.Classes.Domain.Reservation> Reservations => Set<GymManagement.Api.Features.Classes.Domain.Reservation>();
    #endregion

    # region Model Configuration
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Global query filters for multi-tenancy
        modelBuilder.Entity<User>().HasQueryFilter(u => u.GymId == CurrentGymId);
        modelBuilder.Entity<GymManagement.Api.Features.Members.Domain.Member>().HasQueryFilter(m => m.GymId == CurrentGymId && !m.IsDeleted);
        modelBuilder.Entity<GymManagement.Api.Features.Members.Domain.Membership>().HasQueryFilter(m => m.GymId == CurrentGymId);
        modelBuilder.Entity<MembershipType>().HasQueryFilter(mt => mt.GymId == CurrentGymId && !mt.IsDeleted);
        modelBuilder.Entity<GymManagement.Api.Features.Payments.Domain.Payment>().HasQueryFilter(p => p.GymId == CurrentGymId);
        modelBuilder.Entity<GymManagement.Api.Features.Classes.Domain.Class>().HasQueryFilter(c => c.GymId == CurrentGymId && !c.IsDeleted);
        modelBuilder.Entity<GymManagement.Api.Features.Classes.Domain.Reservation>().HasQueryFilter(r => r.GymId == CurrentGymId);

        // Foreign Key Relationships
        modelBuilder.Entity<GymManagement.Api.Features.Members.Domain.Member>(entity =>
        {
            entity.HasMany(m => m.Memberships).WithOne(ms => ms.Member).HasForeignKey(ms => ms.MemberId);
            entity.HasMany(m => m.Reservations).WithOne(r => r.Member).HasForeignKey(r => r.MemberId);
            entity.HasMany(m => m.Payments).WithOne(p => p.Member).HasForeignKey(p => p.MemberId);
        });

        modelBuilder.Entity<GymManagement.Api.Features.Classes.Domain.Class>(entity =>
        {
            entity.HasMany(c => c.Reservations).WithOne(r => r.Class).HasForeignKey(r => r.ClassId);
        });

        modelBuilder.Entity<GymManagement.Api.Features.Memberships.Domain.MembershipType>(entity =>
        {
            entity.HasMany(mt => mt.Memberships).WithOne(m => m.MembershipType).HasForeignKey(m => m.MembershipTypeId);
            entity.Property(mt => mt.Price).HasPrecision(18, 2);
        });

        // Decimal precision configuration
        modelBuilder.Entity<GymManagement.Api.Features.Payments.Domain.Payment>(entity =>
        {
            entity.Property(p => p.Amount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<GymManagement.Api.Features.Members.Domain.Membership>(entity =>
        {
            entity.Property(m => m.PricePaid).HasPrecision(18, 2);
        });
    }
    #endregion

    #region Save Changes logic
    public override int SaveChanges()
    {
        OnBeforeSaving();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        OnBeforeSaving();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void OnBeforeSaving()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            // Assign Tenant
            if (entry.State == EntityState.Added && entry.Entity is ITenantEntity tenantEntity && CurrentGymId != Guid.Empty)
            {
                tenantEntity.GymId = CurrentGymId;
            }

            // Handle Soft Delete
            if (entry.Entity is ISoftDeletable softDeletable)
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    softDeletable.IsDeleted = true;
                }
            }
        }
    }
    #endregion
}
