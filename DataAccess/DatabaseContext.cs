using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ZeeAcom.DataAccess;

public partial class DatabaseContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DatabaseContext()
    {
    }

    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Entity> Entities { get; set; }

    public virtual DbSet<Owner> Owners { get; set; }

    public virtual DbSet<Picture> Pictures { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entity>(entity =>
        {
            entity.ToTable("Entity");

            entity.HasIndex(e => e.OwnerId, "IX_Entity_OwnerId");

            entity.Property(e => e.Id).ValueGeneratedOnAdd(); 

            entity.HasOne(d => d.Owner).WithMany(p => p.Entities).HasForeignKey(d => d.OwnerId);
        });

        modelBuilder.Entity<Owner>(entity =>
        {
            entity.ToTable("Owner");

            entity.Property(e => e.Id)
            .ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Picture>(entity =>
        {
            entity.ToTable("Picture");

            entity.HasIndex(e => e.EntityId, "IX_Picture_EntityId");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(d => d.Entity).WithMany(p => p.Pictures).HasForeignKey(d => d.EntityId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
