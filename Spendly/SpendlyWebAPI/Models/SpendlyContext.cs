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

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Invitation> Invitations { get; set; }

    public virtual DbSet<Revenue> Revenues { get; set; }

    public virtual DbSet<RevenueType> RevenueTypes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserGroup> UserGroups { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DatabaseConnStr");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");

        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Budget__3214EC07460FAA69");

            entity.ToTable("Budget");

            entity.HasIndex(e => new { e.UserGroupId, e.Month, e.Year }, "UQ_Budget").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.Currency).WithMany(p => p.Budgets)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Budget_Currency");

            entity.HasOne(d => d.UserGroup).WithMany(p => p.Budgets)
                .HasForeignKey(d => d.UserGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Budget_UserGroup");
        });

        modelBuilder.Entity<Cost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cost__3214EC0727A8CD48");

            entity.ToTable("Cost");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CostType).WithMany(p => p.Costs)
                .HasForeignKey(d => d.CostTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cost_CostType");

            entity.HasOne(d => d.Currency).WithMany(p => p.Costs)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cost_Currency");

            entity.HasOne(d => d.User).WithMany(p => p.Costs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cost_UserGroup");
        });

        modelBuilder.Entity<CostType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CostType__3214EC0755C91D1D");

            entity.ToTable("CostType");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Currency__3214EC078C8715B3");

            entity.ToTable("Currency");

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Group__3214EC075E6B6C55");

            entity.ToTable("Group");

            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasMany(d => d.Costs).WithMany(p => p.Groups)
                .UsingEntity<Dictionary<string, object>>(
                    "GroupCost",
                    r => r.HasOne<Cost>().WithMany()
                        .HasForeignKey("CostId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_GroupCost_Cost"),
                    l => l.HasOne<Group>().WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_GroupCost_Group"),
                    j =>
                    {
                        j.HasKey("GroupId", "CostId");
                        j.ToTable("GroupCost");
                    });

            entity.HasMany(d => d.Revenues).WithMany(p => p.Groups)
                .UsingEntity<Dictionary<string, object>>(
                    "GroupRevenue",
                    r => r.HasOne<Revenue>().WithMany()
                        .HasForeignKey("RevenueId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_GroupRevenue_Cost"),
                    l => l.HasOne<Group>().WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_GroupRevenue_Group"),
                    j =>
                    {
                        j.HasKey("GroupId", "RevenueId");
                        j.ToTable("GroupRevenue");
                    });
        });

        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Invitati__3214EC07BC956480");

            entity.ToTable("Invitation");

            entity.Property(e => e.ClaimedAt).HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.ExpiredAt).HasColumnType("datetime");
            entity.Property(e => e.Token).HasMaxLength(150);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Invitations)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invitation_CreatedBy");

            entity.HasOne(d => d.Group).WithMany(p => p.Invitations)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invitation_Group");
        });

        modelBuilder.Entity<Revenue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Revenue__3214EC0764502BB6");

            entity.ToTable("Revenue");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Currency).WithMany(p => p.Revenues)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Revenue_Currency");

            entity.HasOne(d => d.RevenueType).WithMany(p => p.Revenues)
                .HasForeignKey(d => d.RevenueTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Revenue_RevenueType");

            entity.HasOne(d => d.User).WithMany(p => p.Revenues)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Revenue_UserGroup");
        });

        modelBuilder.Entity<RevenueType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RevenueT__3214EC07E9B64159");

            entity.ToTable("RevenueType");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Role__3214EC076EA0D0D8");

            entity.ToTable("Role");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC0713E71E54");

            entity.ToTable("User");

            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FistName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(200);
            entity.Property(e => e.PasswordSalt).HasMaxLength(200);
            entity.Property(e => e.Username).HasMaxLength(100);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserRole_Role"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_UserRole_Person"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("UserRole");
                    });
        });

        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserGrou__3214EC07F947E70B");

            entity.ToTable("UserGroup");

            entity.HasIndex(e => e.InvitationId, "UQ__UserGrou__033C8DCEBCBC08B5").IsUnique();

            entity.Property(e => e.JoinedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Group).WithMany(p => p.UserGroups)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserGroup_Group");

            entity.HasOne(d => d.Invitation).WithOne(p => p.UserGroup)
                .HasForeignKey<UserGroup>(d => d.InvitationId)
                .HasConstraintName("FK_UserGroup_Invitation");


            entity.HasOne(d => d.User).WithMany(p => p.UserGroups)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserGroup_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
