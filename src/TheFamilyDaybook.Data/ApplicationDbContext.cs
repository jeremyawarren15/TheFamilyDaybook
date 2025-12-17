using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TheFamilyDaybook.Models;

namespace TheFamilyDaybook.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Family> Families { get; set; }
    public DbSet<Student> Students { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Family-User relationship
        // EF Core can infer most of this from conventions, but we need to specify
        // the delete behavior to set FamilyId to null when Family is deleted
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Family)
            .WithMany(f => f.Users)
            .HasForeignKey(u => u.FamilyId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure Family-Student relationship
        // When a Family is deleted, cascade delete all Students
        modelBuilder.Entity<Student>()
            .HasOne(s => s.Family)
            .WithMany(f => f.Students)
            .HasForeignKey(s => s.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}



