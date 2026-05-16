namespace GLMS.Web_POE.Data
{
	using GLMS.Web_POE.Models;
	using Microsoft.EntityFrameworkCore;

	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public DbSet<Client> Clients => Set<Client>();
		public DbSet<Contract> Contracts => Set<Contract>();
		public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Client>()
				.HasMany(c => c.Contracts)
				.WithOne(c => c.Client)
				.HasForeignKey(c => c.ClientId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Contract>()
				.HasMany(c => c.ServiceRequests)
				.WithOne(sr => sr.Contract)
				.HasForeignKey(sr => sr.ContractId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<ServiceRequest>()
				.Property(sr => sr.Cost)
				.HasPrecision(18, 2);

			modelBuilder.Entity<ServiceRequest>()
				.Property(sr => sr.AmountUsd)
				.HasPrecision(18, 2);

			modelBuilder.Entity<ServiceRequest>()
				.Property(sr => sr.ExchangeRate)
				.HasPrecision(18, 6);
		}
	}
}