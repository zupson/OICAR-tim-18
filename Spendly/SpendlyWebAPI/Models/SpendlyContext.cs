using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SpendlyWebAPI.Models;

public partial class SpendlyContext : DbContext
{
    public SpendlyContext()
    {
    }

    public SpendlyContext(DbContextOptions<SpendlyContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Budget> Budgets { get; set; }

    public virtual DbSet<Cost> Costs { get; set; }

    public virtual DbSet<CostType> CostTypes { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Invitation> Invitations { get; set; }

    public virtual DbSet<Revenue> Revenues { get; set; }

    public virtual DbSet<RevenueType> RevenueTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserGroup> UserGroups { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DatabaseConnStr");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Budget__3214EC071DA6EF55");

            entity.ToTable("Budget");

            entity.HasIndex(e => new { e.UserGroupId, e.Month, e.Year }, "UQ_Budget").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.UserGroup).WithMany(p => p.Budgets)
                .HasForeignKey(d => d.UserGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Budget_UserGroup");
        });

        modelBuilder.Entity<Cost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cost__3214EC07F72660F9");

            entity.ToTable("Cost");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CostType).WithMany(p => p.Costs)
                .HasForeignKey(d => d.CostTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cost_CostType");

            entity.HasOne(d => d.Group).WithMany(p => p.Costs)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cost_Group");

            entity.HasOne(d => d.User).WithMany(p => p.Costs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cost_User");
        });

        modelBuilder.Entity<CostType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CostType__3214EC0756EFCC45");

            entity.ToTable("CostType");

            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.Group).WithMany(p => p.CostTypes)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CostType_Group");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Group__3214EC07E5F8D593");

            entity.ToTable("Group");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Invitati__3214EC074DC3A565");

            entity.ToTable("Invitation");

            entity.HasIndex(e => e.Token, "UQ_Token").IsUnique();

            entity.Property(e => e.ClaimedAt).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.ExpiredAt).HasColumnType("datetime");
            entity.Property(e => e.Token).HasMaxLength(150);

            entity.HasOne(d => d.Group).WithMany(p => p.Invitations)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invitation_Group");
        });

        modelBuilder.Entity<Revenue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Revenue__3214EC07DF215DB3");

            entity.ToTable("Revenue");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Group).WithMany(p => p.Revenues)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Revenue_Group");

            entity.HasOne(d => d.RevenueType).WithMany(p => p.Revenues)
                .HasForeignKey(d => d.RevenueTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Revenue_RevenueType");

            entity.HasOne(d => d.User).WithMany(p => p.Revenues)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Revenue_User");
        });

        modelBuilder.Entity<RevenueType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RevenueT__3214EC07839FB7D5");

            entity.ToTable("RevenueType");

            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.Group).WithMany(p => p.RevenueTypes)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RevenueType_Group");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC072ECB7D3D");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ_User_Email").IsUnique();

            entity.HasIndex(e => e.Username, "UQ_User_Username").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(200);
            entity.Property(e => e.PasswordSalt).HasMaxLength(200);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserGrou__3214EC07A7D47450");

            entity.ToTable("UserGroup");

            entity.HasIndex(e => new { e.UserId, e.GroupId }, "UQ_UserGroup").IsUnique();

            entity.Property(e => e.JoinedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Group).WithMany(p => p.UserGroups)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserGroup_Group");

            entity.HasOne(d => d.User).WithMany(p => p.UserGroups)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserGroup_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
