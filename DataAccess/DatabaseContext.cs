using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common.Entities;

namespace ZeeAcom.DataAccess;

/// <summary>
/// App data plus the ASP.NET Core Identity tables (AspNetUsers, AspNetRoles, ...).
/// </summary>
public partial class DatabaseContext : IdentityDbContext<ApplicationUser>
{
    public DatabaseContext()
    {
    }

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Property> Properties { get; set; }

    public virtual DbSet<Owner> Owners { get; set; }

    public virtual DbSet<Picture> Pictures { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Identity table mappings must be configured first.
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Property>(entity =>
        {
            entity.ToTable("Property");

            entity.HasIndex(e => e.OwnerId, "IX_Property_OwnerId");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(d => d.Owner).WithMany(p => p.Properties).HasForeignKey(d => d.OwnerId);
        });

        modelBuilder.Entity<Owner>(entity =>
        {
            entity.ToTable("Owner");

            entity.Property(e => e.Id)
            .ValueGeneratedOnAdd();

            // One owner profile per login. Deleting a user keeps the owner (and listings) but unlinks it.
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasOne(e => e.User).WithOne().HasForeignKey<Owner>(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Picture>(entity =>
        {
            entity.ToTable("Picture");

            entity.HasIndex(e => e.PropertyId, "IX_Picture_PropertyId");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(d => d.Property).WithMany(p => p.Pictures).HasForeignKey(d => d.PropertyId);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("Room");

            entity.HasIndex(e => e.PropertyId, "IX_Room_PropertyId");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.MonthlyRent).HasPrecision(10, 2);

            // Deleting a property deletes its rooms.
            entity.HasOne(d => d.Property).WithMany(p => p.Rooms).HasForeignKey(d => d.PropertyId).OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
