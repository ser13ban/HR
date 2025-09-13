using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using HrAPI.Models;

namespace HrAPI.Data;

public class HrDbContext : IdentityDbContext<Employee, IdentityRole<int>, int>
{
    public HrDbContext(DbContextOptions<HrDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<AbsenceRequest> AbsenceRequests { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Employee configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.Team).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.Bio).HasMaxLength(500);
            entity.Property(e => e.ProfilePictureUrl).HasMaxLength(255);
            entity.Property(e => e.Role).HasConversion<int>();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW(6)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW(6) ON UPDATE NOW(6)");
        });

        // AbsenceRequest configuration
        modelBuilder.Entity<AbsenceRequest>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Reason).HasMaxLength(500);
            entity.Property(a => a.ApprovalNotes).HasMaxLength(500);
            entity.Property(a => a.Type).HasConversion<int>();
            entity.Property(a => a.Status).HasConversion<int>();
            entity.Property(a => a.CreatedAt).HasDefaultValueSql("NOW(6)");
            entity.Property(a => a.UpdatedAt).HasDefaultValueSql("NOW(6) ON UPDATE NOW(6)");

            // Relationships
            entity.HasOne(a => a.Employee)
                  .WithMany(e => e.AbsenceRequests)
                  .HasForeignKey(a => a.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.ApprovedBy)
                  .WithMany()
                  .HasForeignKey(a => a.ApprovedById)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Feedback configuration
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Content).IsRequired().HasMaxLength(1000);
            entity.Property(f => f.PolishedContent).HasMaxLength(1000);
            entity.Property(f => f.Type).HasConversion<int>();
            entity.Property(f => f.CreatedAt).HasDefaultValueSql("NOW(6)");
            entity.Property(f => f.UpdatedAt).HasDefaultValueSql("NOW(6) ON UPDATE NOW(6)");

            // Relationships
            entity.HasOne(f => f.FromEmployee)
                  .WithMany(e => e.GivenFeedback)
                  .HasForeignKey(f => f.FromEmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.ToEmployee)
                  .WithMany(e => e.ReceivedFeedback)
                  .HasForeignKey(f => f.ToEmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Ensure an employee cannot give feedback to themselves
            entity.HasCheckConstraint("CK_Feedback_SelfReference", "FromEmployeeId != ToEmployeeId");
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        // Seed default roles
        modelBuilder.Entity<IdentityRole<int>>().HasData(
            new IdentityRole<int> { Id = 1, Name = "Employee", NormalizedName = "EMPLOYEE" },
            new IdentityRole<int> { Id = 2, Name = "Manager", NormalizedName = "MANAGER" },
            new IdentityRole<int> { Id = 3, Name = "Admin", NormalizedName = "ADMIN" }
        );

        // Note: Employee seeding will be handled through the AuthService during registration
        // or through a separate data seeding service to properly handle password hashing
        
        // Note: AbsenceRequest and Feedback seed data removed since they depend on employees
        // These can be added later once employees are properly seeded with Identity
    }
}
