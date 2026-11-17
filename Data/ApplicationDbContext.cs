using ExamGuard.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExamGuard.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Exam> Exams { get; set; }
        public DbSet<InvigilationDuty> InvigilationDuties { get; set; }
        public DbSet<SwapRequest> SwapRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Prevent multiple cascade path cycles in SQL Server for SwapRequests
            builder.Entity<SwapRequest>()
                .HasOne(s => s.OriginalDuty)
                .WithMany()
                .HasForeignKey(s => s.OriginalDutyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SwapRequest>()
                .HasOne(s => s.TargetDuty)
                .WithMany()
                .HasForeignKey(s => s.TargetDutyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SwapRequest>()
                .HasOne(s => s.RequesterUser)
                .WithMany()
                .HasForeignKey(s => s.RequesterUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SwapRequest>()
                .HasOne(s => s.TargetUser)
                .WithMany()
                .HasForeignKey(s => s.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}