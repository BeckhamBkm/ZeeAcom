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

    public virtual DbSet<Enquiry> Enquiries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Identity table mappings must be configured first.
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Property>(entity =>
        {
            entity.ToTable("Property");

            entity.HasIndex(e => e.OwnerId, "IX_Property_OwnerId");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.MonthlyRent).HasPrecision(10, 2);

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

            // Stored as text ("Pending", "Approved"...) so the table reads clearly.
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<Picture>(entity =>
        {
            entity.ToTable("Picture");

            entity.HasIndex(e => e.PropertyId, "IX_Picture_PropertyId");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Caption).HasMaxLength(100);
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(400);

            entity.HasOne(d => d.Property).WithMany(p => p.Pictures).HasForeignKey(d => d.PropertyId);
        });

        modelBuilder.Entity<Enquiry>(entity =>
        {
            entity.ToTable("Enquiry");

            entity.HasIndex(e => e.PropertyId, "IX_Enquiry_PropertyId");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.CellPhone).HasMaxLength(30);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Message).HasMaxLength(1000);

            // Enquiries go with their property.
            entity.HasOne(d => d.Property).WithMany().HasForeignKey(d => d.PropertyId).OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
