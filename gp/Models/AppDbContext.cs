using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace gp.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alert> Alerts { get; set; }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<FinancialGoal> FinancialGoals { get; set; }

    public virtual DbSet<MonthlyBill> MonthlyBills { get; set; }

    public virtual DbSet<PurchasedProduct> PurchasedProducts { get; set; }

    public virtual DbSet<ToBuyList> ToBuyLists { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=BLUEWISE;Database=testgb;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.AlertId).HasName("PK__Alert__EBB16AEDE7665FE4");

            entity.ToTable("Alert");

            entity.Property(e => e.AlertId)
                .ValueGeneratedNever()
                .HasColumnName("AlertID");
            entity.Property(e => e.DateTime).HasColumnType("datetime");
            entity.Property(e => e.Message)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Alerts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Alert__UserID__403A8C7D");
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.ExpenseId).HasName("PK__Expense__1445CFF37DCB1328");

            entity.ToTable("Expense");

            entity.Property(e => e.ExpenseId)
                .ValueGeneratedNever()
                .HasColumnName("ExpenseID");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Details)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Expense__UserID__4316F928");
        });

        modelBuilder.Entity<FinancialGoal>(entity =>
        {
            entity.HasKey(e => e.GoalId).HasName("PK__Financia__8A4FFF31CF69C16C");

            entity.ToTable("FinancialGoal");

            entity.Property(e => e.GoalId)
                .ValueGeneratedNever()
                .HasColumnName("GoalID");
            entity.Property(e => e.Triger).HasColumnName("triger");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.FinancialGoals)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Financial__UserI__3A81B327");
        });

        modelBuilder.Entity<MonthlyBill>(entity =>
        {
            entity.HasKey(e => e.BillId).HasName("PK__MonthlyB__11F2FC4A3EAC0B26");

            entity.ToTable("MonthlyBill");

            entity.Property(e => e.BillId)
                .ValueGeneratedNever()
                .HasColumnName("BillID");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ExpenseId).HasColumnName("ExpenseID");
            entity.Property(e => e.Issuer)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Expense).WithMany(p => p.MonthlyBills)
                .HasForeignKey(d => d.ExpenseId)
                .HasConstraintName("FK__MonthlyBi__Expen__49C3F6B7");

            entity.HasOne(d => d.User).WithMany(p => p.MonthlyBills)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__MonthlyBi__UserI__48CFD27E");
        });

        modelBuilder.Entity<PurchasedProduct>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__Purchase__727E83EBD1B3D8B7");

            entity.Property(e => e.ItemId)
                .ValueGeneratedNever()
                .HasColumnName("ItemID");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ExpenseId).HasColumnName("ExpenseID");
            entity.Property(e => e.ItemName)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ReceiptImage)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ShopName)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Expense).WithMany(p => p.PurchasedProducts)
                .HasForeignKey(d => d.ExpenseId)
                .HasConstraintName("FK__Purchased__Expen__45F365D3");
        });

        modelBuilder.Entity<ToBuyList>(entity =>
        {
            entity.HasKey(e => e.ListId).HasName("PK__ToBuyLis__E38328655126F28C");

            entity.ToTable("ToBuyList");

            entity.Property(e => e.ListId)
                .ValueGeneratedNever()
                .HasColumnName("ListID");
            entity.Property(e => e.ProductImage)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("productImage");
            entity.Property(e => e.ProductName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("productName");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.ToBuyLists)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__ToBuyList__UserI__3D5E1FD2");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CCACEB4473B1");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__A9D10534C921D45B").IsUnique();

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("UserID");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Photo)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
