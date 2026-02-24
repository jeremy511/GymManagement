using System;
using Microsoft.EntityFrameworkCore;
namespace GymManagement.Api.Infrastructure.Persistence;

{
	public class GymManagementDbContext : DbContext
	{
		public GymManagementDbContext(DbContextOptions<GymManagementDbContext> options)
			: base(options)
		{
		}

		// Add DbSet properties for your aggregates, for example:
		// public DbSet<Member> Members { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Configure entity mappings here
		}
	}
}
