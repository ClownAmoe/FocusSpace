using FocusSpace.Domain.Entities;
using FocusSpace.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using DomainTask = FocusSpace.Domain.Entities.Task;

namespace FocusSpace.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<DomainTask> Tasks => Set<DomainTask>();
        public DbSet<Session> Sessions => Set<Session>();
        public DbSet<Planet> Planets => Set<Planet>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── User ──────────────────────────────────────────────────
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Role)
                      .HasConversion<string>()
                      .HasDefaultValue(UserRole.User);

                entity.HasOne(u => u.CurrentPlanet)
                      .WithMany(p => p.Users)
                      .HasForeignKey(u => u.CurrentPlanetId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Planet ────────────────────────────────────────────────
            modelBuilder.Entity<Planet>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            });

            // ── Task ──────────────────────────────────────────────────
            modelBuilder.Entity<DomainTask>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Title).IsRequired().HasMaxLength(300);

                entity.HasOne(t => t.User)
                      .WithMany(u => u.Tasks)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Session ───────────────────────────────────────────────
            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Status)
                      .HasConversion<string>()
                      .HasDefaultValue(SessionStatus.Ongoing);

                entity.HasOne(s => s.User)
                      .WithMany(u => u.Sessions)
                      .HasForeignKey(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Task)
                      .WithMany(t => t.Sessions)
                      .HasForeignKey(s => s.TaskId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
