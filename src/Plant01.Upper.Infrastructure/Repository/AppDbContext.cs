using Microsoft.EntityFrameworkCore;
using Plant01.Upper.Domain.Aggregation;
using Plant01.Upper.Domain.Entities;

namespace Plant01.Upper.Infrastructure.Repository
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<Bag> Bags { get; set; }
        public DbSet<Pallet> Pallets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // WorkOrder Configuration
            modelBuilder.Entity<WorkOrder>(entity =>
            {
                entity.HasKey(e => e.Code);
                entity.OwnsMany(e => e.Items, a =>
                {
                    a.WithOwner().HasForeignKey("WorkOrderCode");
                    a.Property<int>("Id");
                    a.HasKey("Id");
                });
            });

            // Bag Configuration
            modelBuilder.Entity<Bag>(entity =>
            {
                entity.HasKey(e => e.BagCode);
                entity.OwnsMany(e => e.Records, a =>
                {
                    a.WithOwner().HasForeignKey("BagCode");
                    a.HasKey(r => r.Id);
                });
            });

            // Pallet Configuration
            modelBuilder.Entity<Pallet>(entity =>
            {
                entity.HasKey(e => e.PalletCode);
                entity.OwnsMany(e => e.Items, a =>
                {
                    a.WithOwner().HasForeignKey("PalletCode");
                    a.HasKey(i => i.Id);
                });
            });
        }
    }
}
