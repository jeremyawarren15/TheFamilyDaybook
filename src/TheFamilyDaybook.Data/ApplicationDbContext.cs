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
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Metric> Metrics { get; set; }
    public DbSet<DailyLog> DailyLogs { get; set; }
    public DbSet<DailyLogMetricValue> DailyLogMetricValues { get; set; }
    public DbSet<StudentMetric> StudentMetrics { get; set; }
    public DbSet<StudentSubject> StudentSubjects { get; set; }
    public DbSet<StudentSubjectMetric> StudentSubjectMetrics { get; set; }

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

        // Configure Family-Subject relationship
        // When a Family is deleted, cascade delete all Subjects
        modelBuilder.Entity<Subject>()
            .HasOne(s => s.Family)
            .WithMany(f => f.Subjects)
            .HasForeignKey(s => s.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Family-Metric relationship
        // When a Family is deleted, cascade delete all Metrics
        modelBuilder.Entity<Metric>()
            .HasOne(m => m.Family)
            .WithMany()
            .HasForeignKey(m => m.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Student-DailyLog relationship
        // When a Student is deleted, cascade delete all DailyLogs
        modelBuilder.Entity<DailyLog>()
            .HasOne(dl => dl.Student)
            .WithMany()
            .HasForeignKey(dl => dl.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Subject-DailyLog relationship
        // When a Subject is deleted, cascade delete all DailyLogs
        modelBuilder.Entity<DailyLog>()
            .HasOne(dl => dl.Subject)
            .WithMany()
            .HasForeignKey(dl => dl.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure DailyLog-DailyLogMetricValue relationship
        // When a DailyLog is deleted, cascade delete all DailyLogMetricValues
        modelBuilder.Entity<DailyLogMetricValue>()
            .HasOne(dlmv => dlmv.DailyLog)
            .WithMany(dl => dl.MetricValues)
            .HasForeignKey(dlmv => dlmv.DailyLogId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Metric-DailyLogMetricValue relationship
        // When a Metric is deleted, cascade delete all DailyLogMetricValues
        modelBuilder.Entity<DailyLogMetricValue>()
            .HasOne(dlmv => dlmv.Metric)
            .WithMany()
            .HasForeignKey(dlmv => dlmv.MetricId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Student-StudentMetric relationship
        // When a Student is deleted, cascade delete all StudentMetrics
        modelBuilder.Entity<StudentMetric>()
            .HasOne(sm => sm.Student)
            .WithMany()
            .HasForeignKey(sm => sm.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Metric-StudentMetric relationship
        // When a Metric is deleted, cascade delete all StudentMetrics
        modelBuilder.Entity<StudentMetric>()
            .HasOne(sm => sm.Metric)
            .WithMany()
            .HasForeignKey(sm => sm.MetricId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Student-StudentSubject relationship
        // When a Student is deleted, cascade delete all StudentSubjects
        modelBuilder.Entity<StudentSubject>()
            .HasOne(ss => ss.Student)
            .WithMany()
            .HasForeignKey(ss => ss.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Subject-StudentSubject relationship
        // When a Subject is deleted, cascade delete all StudentSubjects
        modelBuilder.Entity<StudentSubject>()
            .HasOne(ss => ss.Subject)
            .WithMany()
            .HasForeignKey(ss => ss.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Student-StudentSubjectMetric relationship
        // When a Student is deleted, cascade delete all StudentSubjectMetrics
        modelBuilder.Entity<StudentSubjectMetric>()
            .HasOne(ssm => ssm.Student)
            .WithMany()
            .HasForeignKey(ssm => ssm.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Subject-StudentSubjectMetric relationship
        // When a Subject is deleted, cascade delete all StudentSubjectMetrics
        modelBuilder.Entity<StudentSubjectMetric>()
            .HasOne(ssm => ssm.Subject)
            .WithMany()
            .HasForeignKey(ssm => ssm.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Metric-StudentSubjectMetric relationship
        // When a Metric is deleted, cascade delete all StudentSubjectMetrics
        modelBuilder.Entity<StudentSubjectMetric>()
            .HasOne(ssm => ssm.Metric)
            .WithMany()
            .HasForeignKey(ssm => ssm.MetricId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}



