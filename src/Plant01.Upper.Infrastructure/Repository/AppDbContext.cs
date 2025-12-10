using Microsoft.EntityFrameworkCore;

using Plant01.Upper.Domain.Aggregation;
using Plant01.Upper.Domain.Entities;

namespace Plant01.Upper.Infrastructure.Repository;

public class AppDbContext : DbContext
{
    public DbSet<WorkOrder> WorkOrders { get; set; }
    public DbSet<Bag> Bags { get; set; }
    public DbSet<Pallet> Pallets { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure DateTime properties to use UTC
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                        v => v.ToUniversalTime(),
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));
                }
            }
        }

        // WorkOrder Configuration
        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.HasKey(e => e.Code);
            entity.ToTable(e => e.HasComment("生产工单表"));
            entity.Property(e => e.Id).HasColumnOrder(1);
            entity.Property(e => e.Code).HasComment("生产工单号").HasColumnOrder(2);
            entity.Property(e => e.OrderDate).HasComment("工单日期").HasColumnOrder(3);
            entity.Property(e => e.LineNo).HasComment("产线编号").HasColumnOrder(4);
            entity.Property(e => e.ProductCode).HasComment("产品编号").HasColumnOrder(5);
            entity.Property(e => e.ProductName).HasComment("产品名称").HasColumnOrder(6);
            entity.Property(e => e.ProductSpec).HasComment("规格型号").HasColumnOrder(7);
            entity.Property(e => e.Quantity).HasComment("计划生产数量").HasColumnOrder(8);
            entity.Property(e => e.Unit).HasComment("单位").HasColumnOrder(9);
            entity.Property(e => e.BatchNumber).HasComment("批号").HasColumnOrder(10);
            entity.Property(e => e.LabelTemplateCode).HasComment("标签模板编号").HasColumnOrder(11);
            entity.Property(e => e.Status).HasComment("工单状态，1开工 99完工").HasColumnOrder(12);
            entity.Property(e => e.UpdatedAt).HasColumnOrder(13);
            entity.Property(e => e.CreatedAt).HasColumnOrder(14);
            entity.Property(e => e.IsDeleted).HasColumnOrder(15);
            entity.Property(e => e.Version).HasColumnOrder(16);
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
            entity.ToTable(e => e.HasComment("包装袋表"));
            entity.Property(e => e.LineNo).HasComment("产线").HasColumnOrder(1);
            entity.Property(e => e.StationNo).HasComment("工位").HasColumnOrder(2);
            entity.Property(e => e.BagCode).HasComment("袋码").HasColumnOrder(3);
            entity.Property(e => e.OrderCode).HasComment("工单号").HasColumnOrder(4);
            entity.Property(e => e.ProductCode).HasComment("配方").HasColumnOrder(5);
            entity.Property(e => e.ProductAlias).HasComment("牌号").HasColumnOrder(6);
            entity.Property(e => e.ProductWeight).HasComment("标称重量").HasColumnOrder(7);
            entity.Property(e => e.ProductActualWeight).HasComment("实际重量").HasColumnOrder(8);
            entity.Property(e => e.ProductWeightUnit).HasComment("重量单位").HasColumnOrder(9);
            entity.Property(e => e.ProductHeight).HasComment("袋偏").HasColumnOrder(10);
            entity.Property(e => e.ProductHeightUnit).HasComment("袋偏单位").HasColumnOrder(11);
            entity.Property(e => e.BatchCode).HasComment("批号").HasColumnOrder(12);
            entity.Property(e => e.SeqDigits).HasComment("批次补位").HasColumnOrder(13);
            entity.Property(e => e.LoadShape).HasComment("垛型").HasColumnOrder(14);
            entity.Property(e => e.LoadQuantity).HasComment("垛量").HasColumnOrder(15);
            entity.Property(e => e.IsNeedPrint).HasComment("是否打印").HasColumnOrder(16);
            entity.Property(e => e.SerialNo).HasComment("序列号").HasColumnOrder(17);
            entity.Property(e => e.LoadPosition).HasComment("垛位").HasColumnOrder(18);
            entity.Property(e => e.PalletCode).HasComment("托盘码").HasColumnOrder(19);
            entity.Property(e => e.PrintedAt).HasComment("喷墨时间").HasColumnOrder(20);
            entity.Property(e => e.PalletizedAt).HasComment("码垛时间").HasColumnOrder(21);
            entity.Property(e => e.ProductionAt).HasComment("生产日期").HasColumnOrder(22);
            entity.Property(e => e.Id).HasColumnOrder(23);
            entity.Property(e => e.UpdatedAt).HasColumnOrder(24);
            entity.Property(e => e.CreatedAt).HasColumnOrder(25);
            entity.Property(e => e.IsDeleted).HasColumnOrder(26);
            entity.Property(e => e.Version).HasColumnOrder(27);
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
            entity.HasComment("托盘聚合根表");
            entity.Property(e => e.PalletCode).HasComment("托盘码 (唯一标识)").HasColumnOrder(1);
            entity.Property(e => e.WorkOrderCode).HasComment("关联工单号").HasColumnOrder(2);
            entity.Property(e => e.IsFull).HasComment("是否满垛").HasColumnOrder(3);
            entity.Property(e => e.OutTime).HasComment("出垛时间").HasColumnOrder(4);
            entity.Property(e => e.CurrentPalletizerId).HasComment("当前所在的码垛机编号").HasColumnOrder(5);
            entity.OwnsMany(e => e.Items, a =>
            {
                a.WithOwner().HasForeignKey("PalletCode");
                a.HasKey(i => i.Id);
            });
        });
    }
}
