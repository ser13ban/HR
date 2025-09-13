using Microsoft.EntityFrameworkCore;
using HrAPI.Models;

namespace HrAPI.Data;

public class HrDbContext : DbContext
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
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Department).HasMaxLength(100);
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
        
        // Seed employees
        modelBuilder.Entity<Employee>().HasData(
            new Employee
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@company.com",
                PhoneNumber = "+1234567890",
                Department = "Engineering",
                Position = "Senior Developer",
                HireDate = new DateTime(2020, 1, 15),
                Bio = "Experienced software developer with expertise in .NET and Angular.",
                Role = EmployeeRole.Manager,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Employee
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@company.com",
                PhoneNumber = "+1234567891",
                Department = "Engineering",
                Position = "Developer",
                HireDate = new DateTime(2021, 3, 10),
                Bio = "Passionate about clean code and user experience.",
                Role = EmployeeRole.Employee,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Employee
            {
                Id = 3,
                FirstName = "Mike",
                LastName = "Johnson",
                Email = "mike.johnson@company.com",
                PhoneNumber = "+1234567892",
                Department = "HR",
                Position = "HR Manager",
                HireDate = new DateTime(2019, 6, 1),
                Bio = "Dedicated to creating a positive work environment.",
                Role = EmployeeRole.Manager,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            }
        );

        // Seed absence requests
        modelBuilder.Entity<AbsenceRequest>().HasData(
            new AbsenceRequest
            {
                Id = 1,
                EmployeeId = 2,
                StartDate = new DateTime(2024, 12, 20),
                EndDate = new DateTime(2024, 12, 22),
                Type = AbsenceType.Vacation,
                Reason = "Family vacation",
                Status = AbsenceStatus.Approved,
                ApprovedById = 1,
                ApprovedAt = new DateTime(2024, 12, 1),
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new AbsenceRequest
            {
                Id = 2,
                EmployeeId = 2,
                StartDate = new DateTime(2024, 12, 25),
                EndDate = new DateTime(2024, 12, 25),
                Type = AbsenceType.SickLeave,
                Reason = "Doctor appointment",
                Status = AbsenceStatus.Pending,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            }
        );

        // Seed feedback
        modelBuilder.Entity<Feedback>().HasData(
            new Feedback
            {
                Id = 1,
                FromEmployeeId = 1,
                ToEmployeeId = 2,
                Content = "Jane is an excellent team player with great communication skills.",
                Type = FeedbackType.Collaboration,
                Rating = 9,
                IsAnonymous = false,
                IsPolished = false,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            }
        );
    }
}
