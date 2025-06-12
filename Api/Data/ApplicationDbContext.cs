using Microsoft.EntityFrameworkCore;
using Api.Models;

namespace Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Dependent> Dependents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Employee entity
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
            entity.Property(e => e.DateOfBirth).IsRequired();
        });

        // Configure Dependent entity
        modelBuilder.Entity<Dependent>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(d => d.LastName).IsRequired().HasMaxLength(100);
            entity.Property(d => d.DateOfBirth).IsRequired();
            entity.Property(d => d.Relationship).HasConversion<int>().IsRequired();

            // Configure relationship with Employee
            entity.HasOne(d => d.Employee)
                  .WithMany(e => e.Dependents)
                  .HasForeignKey(d => d.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Employees - Updated to match integration test expectations
        modelBuilder.Entity<Employee>().HasData(
            new Employee
            {
                Id = 1,
                FirstName = "LeBron",
                LastName = "James",
                Salary = 75420.99m,
                DateOfBirth = new DateOnly(1984, 12, 30)
            },
            new Employee
            {
                Id = 2,
                FirstName = "Ja",
                LastName = "Morant",
                Salary = 92365.22m,
                DateOfBirth = new DateOnly(1999, 8, 10)
            },
            new Employee
            {
                Id = 3,
                FirstName = "Michael",
                LastName = "Jordan",
                Salary = 143211.12m,
                DateOfBirth = new DateOnly(1963, 2, 17)
            }
        );

        // Seed Dependents - Updated to match the correct logical structure
        modelBuilder.Entity<Dependent>().HasData(
            new Dependent
            {
                Id = 1,
                FirstName = "Spouse",
                LastName = "Morant",
                DateOfBirth = new DateOnly(1998, 3, 3),
                Relationship = Relationship.Spouse,
                EmployeeId = 2 // Ja Morant (logical owner of Morant dependents)
            },
            new Dependent
            {
                Id = 2,
                FirstName = "Child1",
                LastName = "Morant",
                DateOfBirth = new DateOnly(2020, 6, 23),
                Relationship = Relationship.Child,
                EmployeeId = 2 // Ja Morant (logical owner of Morant dependents)
            },
            new Dependent
            {
                Id = 3,
                FirstName = "Child2",
                LastName = "Morant",
                DateOfBirth = new DateOnly(2021, 5, 18),
                Relationship = Relationship.Child,
                EmployeeId = 2 // Ja Morant (logical owner of Morant dependents)
            },
            new Dependent
            {
                Id = 4,
                FirstName = "DP",
                LastName = "Jordan",
                DateOfBirth = new DateOnly(1974, 1, 2),
                Relationship = Relationship.DomesticPartner,
                EmployeeId = 3 // Michael Jordan (logical owner of Jordan dependents)
            }
        );
    }
}
