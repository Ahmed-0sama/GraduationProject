using System;
using System.Collections.Generic;
using Graduation_Project.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace gp.Models;

public partial class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alert> Alerts { get; set; }

    public virtual DbSet<BestPriceProduct> BestPriceProducts { get; set; }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<FinancialGoal> FinancialGoals { get; set; }

    public virtual DbSet<MonthlyBill> MonthlyBills { get; set; }

    public virtual DbSet<PurchasedProduct> PurchasedProducts { get; set; }
	public DbSet<ProductPriceHistory> ProductPriceHistories { get; set; }

	public virtual DbSet<ToBuyList> ToBuyLists { get; set; }

    public virtual DbSet<User> Users { get; set; }

    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    modelBuilder.Entity<Alert>(entity =>
    //    {
    //        entity.HasKey(e => e.AlertId).HasName("PK__Alert__EBB16AED01FBFC2F");

    //        entity.ToTable("Alert");

    //        entity.Property(e => e.AlertId).HasColumnName("AlertID");
    //        entity.Property(e => e.DateTime).HasColumnType("datetime");
    //        entity.Property(e => e.Type).HasMaxLength(255);
    //        entity.Property(e => e.UserId).HasColumnName("UserID");

    //        entity.HasOne(d => d.User).WithMany(p => p.Alerts)
    //            .HasForeignKey(d => d.UserId)
    //            .HasConstraintName("FK__Alert__UserID__4316F928");
    //    });

    //    modelBuilder.Entity<BestPriceProduct>(entity =>
    //    {
    //        entity.HasKey(e => e.ItemId).HasName("PK__BestPric__727E83EB6DEE9C9F");

    //        entity.ToTable("BestPriceProduct");

    //        entity.Property(e => e.ItemId).HasColumnName("ItemID");
    //        entity.Property(e => e.Category).HasMaxLength(255);
    //        entity.Property(e => e.ListId).HasColumnName("ListID");
    //        entity.Property(e => e.ProductName).HasMaxLength(255);
    //        entity.Property(e => e.ShopName).HasMaxLength(255);

    //        entity.HasOne(d => d.List).WithMany(p => p.BestPriceProducts)
    //            .HasForeignKey(d => d.ListId)
    //            .HasConstraintName("FK__BestPrice__ListI__403A8C7D");
    //    });

    //    modelBuilder.Entity<Expense>(entity =>
    //    {
    //        entity.HasKey(e => e.ExpenseId).HasName("PK__Expense__1445CFF3C0E94FB1");

    //        entity.ToTable("Expense");

    //        entity.Property(e => e.ExpenseId).HasColumnName("ExpenseID");
    //        entity.Property(e => e.BillId).HasColumnName("BillID");
    //        entity.Property(e => e.PurchasedId).HasColumnName("PurchasedID");
    //        entity.Property(e => e.UserId).HasColumnName("UserID");

    //        entity.HasOne(d => d.Bill).WithMany(p => p.Expenses)
    //            .HasForeignKey(d => d.BillId)
    //            .HasConstraintName("FK__Expense__BillID__4CA06362");

    //        entity.HasOne(d => d.Purchased).WithMany(p => p.Expenses)
    //            .HasForeignKey(d => d.PurchasedId)
    //            .HasConstraintName("FK__Expense__Purchas__4BAC3F29");

    //        entity.HasOne(d => d.User).WithMany(p => p.Expenses)
    //            .HasForeignKey(d => d.UserId)
    //            .HasConstraintName("FK__Expense__UserID__4D94879B");
    //    });

    //    modelBuilder.Entity<FinancialGoal>(entity =>
    //    {
    //        entity.HasKey(e => e.GoalId).HasName("PK__Financia__8A4FFF31B51EE9B9");

    //        entity.ToTable("FinancialGoal");

    //        entity.Property(e => e.GoalId).HasColumnName("GoalID");
    //        entity.Property(e => e.UserId).HasColumnName("UserID");

    //        entity.HasOne(d => d.User).WithMany(p => p.FinancialGoals)
    //            .HasForeignKey(d => d.UserId)
    //            .HasConstraintName("FK__Financial__UserI__3A81B327");
    //    });

    //    modelBuilder.Entity<MonthlyBill>(entity =>
    //    {
    //        entity.HasKey(e => e.BillId).HasName("PK__MonthlyB__11F2FC4A88D5FB32");

    //        entity.ToTable("MonthlyBill");

    //        entity.Property(e => e.BillId).HasColumnName("BillID");
    //        entity.Property(e => e.Category).HasMaxLength(255);
    //        entity.Property(e => e.Issuer).HasMaxLength(255);
    //        entity.Property(e => e.Name).HasMaxLength(255);
    //        entity.Property(e => e.UserId).HasColumnName("UserID");

    //        entity.HasOne(d => d.User).WithMany(p => p.MonthlyBills)
    //            .HasForeignKey(d => d.UserId)
    //            .HasConstraintName("FK__MonthlyBi__UserI__45F365D3");
    //    });

    //    modelBuilder.Entity<PurchasedProduct>(entity =>
    //    {
    //        entity.HasKey(e => e.PurchasedId).HasName("PK__Purchase__2B7C245C710664C4");

    //        entity.Property(e => e.PurchasedId).HasColumnName("PurchasedID");
    //        entity.Property(e => e.Category).HasMaxLength(255);
    //        entity.Property(e => e.ItemName).HasMaxLength(255);
    //        entity.Property(e => e.ShopName).HasMaxLength(255);
    //        entity.Property(e => e.UserId).HasColumnName("UserID");

    //        entity.HasOne(d => d.User).WithMany(p => p.PurchasedProducts)
    //            .HasForeignKey(d => d.UserId)
    //            .HasConstraintName("FK__Purchased__UserI__48CFD27E");
    //    });

    //    modelBuilder.Entity<ToBuyList>(entity =>
    //    {
    //        entity.HasKey(e => e.ListId).HasName("PK__ToBuyLis__E38328651100FCD2");

    //        entity.ToTable("ToBuyList");

    //        entity.Property(e => e.ListId).HasColumnName("ListID");
    //        entity.Property(e => e.Date).HasColumnType("datetime");
    //        entity.Property(e => e.ProductName).HasMaxLength(255);
    //        entity.Property(e => e.UserId).HasColumnName("UserID");

    //        entity.HasOne(d => d.User).WithMany(p => p.ToBuyLists)
    //            .HasForeignKey(d => d.UserId)
    //            .HasConstraintName("FK__ToBuyList__UserI__3D5E1FD2");
    //    });

    //    modelBuilder.Entity<User>(entity =>
    //    {
    //        entity.HasKey(e => e.UserId).HasName("PK__User__1788CCAC89F71645");

    //        entity.ToTable("User");

    //        entity.HasIndex(e => e.Email, "UQ__User__A9D10534CFA727C6").IsUnique();

    //        entity.Property(e => e.UserId).HasColumnName("UserID");
    //        entity.Property(e => e.Email).HasMaxLength(255);
    //        entity.Property(e => e.Name).HasMaxLength(255);
    //        entity.Property(e => e.Password).HasMaxLength(255);
    //    });

    //    OnModelCreatingPartial(modelBuilder);
    //}

    //partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
