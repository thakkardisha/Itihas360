using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Itihas360.Models;

public partial class Itihas360Context : DbContext
{
    public Itihas360Context()
    {
    }

    public Itihas360Context(DbContextOptions<Itihas360Context> options)
        : base(options)
    {
    }

    // Yahan saari DbSet properties add ki gayi hain jo controllers dhoond rahe hain
    public virtual DbSet<Article> Articles { get; set; }
    public virtual DbSet<AuditLog> AuditLogs { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Contact> Contacts { get; set; }
    public virtual DbSet<Mcqoption> Mcqoptions { get; set; }
    public virtual DbSet<Mcqquestion> Mcqquestions { get; set; }
    public virtual DbSet<NewsFeedCache> NewsFeedCaches { get; set; }
    public virtual DbSet<Newsletter> Newsletters { get; set; }
    public virtual DbSet<Organization> Organizations { get; set; }
    public virtual DbSet<Tag> Tags { get; set; }
    public virtual DbSet<User> Users { get; set; }

    // AspNet Identity Tables (Agar zaroorat ho toh)
    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Connection string Program.cs se aa rahi hai, yahan khali rakhein.
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- 1. AuditLog Fix ---
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.LogId);
            entity.Property(e => e.PerformedAt).HasDefaultValueSql("(getdate())");

            // Link UserId (string) to User.UserId
            entity.HasOne(d => d.User)
                  .WithMany(p => p.AuditLogs)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_AuditLog_Users");
        });

        // --- 2. Articles Fix (As we discussed) ---
        modelBuilder.Entity<Article>(entity =>
        {
            // Specify that this links to IdentityUser explicitly
            entity.HasOne(d => d.CreatedByNavigation)
                .WithMany()
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Article_CreatedBy");

            entity.HasOne(d => d.UpdatedByNavigation)
                .WithMany()
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Article_UpdatedBy");
        });

        // --- 3. MCQ Questions Fix ---
        // If your MCQQuestions table tracks who created the question
        modelBuilder.Entity<Mcqquestion>(entity =>
        {
            entity.HasOne(d => d.CreatedByNavigation)
                  .WithMany(p => p.Mcqquestions) // Ensure this exists in User.cs
                  .HasForeignKey(d => d.CreatedBy)
                  .OnDelete(DeleteBehavior.ClientSetNull);
        });
            
        // 4. Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.CategoryName).IsUnique();
            entity.HasIndex(e => e.CategorySlug).IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}