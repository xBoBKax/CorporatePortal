using CorporatePortal.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CorporatePortal.Api.Data
{
    public class PortalDbContext : DbContext
    {
        public PortalDbContext(DbContextOptions<PortalDbContext> options)
            : base(options)
        {
        }

        public DbSet<PortalPage> PortalPages => Set<PortalPage>();

        public DbSet<LinkGroup> LinkGroups => Set<LinkGroup>();

        public DbSet<PortalLink> PortalLinks => Set<PortalLink>();

        public DbSet<PortalModule> PortalModules => Set<PortalModule>();

        public DbSet<EmployeeBirthday> EmployeeBirthdays => Set<EmployeeBirthday>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PortalPage>(entity =>
            {
                entity.Property(page => page.Title).HasMaxLength(200).IsRequired();
                entity.Property(page => page.WelcomeText).HasMaxLength(500);
                entity.Property(page => page.LogoUrl).HasMaxLength(1000);
                entity.Property(page => page.BackgroundType).HasMaxLength(50).IsRequired();
                entity.Property(page => page.BackgroundValue).HasMaxLength(1000);
            });

            modelBuilder.Entity<LinkGroup>(entity =>
            {
                entity.Property(group => group.Title).HasMaxLength(200).IsRequired();
                entity.Property(group => group.Description).HasMaxLength(500);
                entity.Property(group => group.Icon).HasMaxLength(1000);
                entity.HasOne(group => group.PortalPage)
                    .WithMany(page => page.LinkGroups)
                    .HasForeignKey(group => group.PortalPageId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PortalLink>(entity =>
            {
                entity.Property(link => link.Title).HasMaxLength(200).IsRequired();
                entity.Property(link => link.Url).HasMaxLength(2000).IsRequired();
                entity.Property(link => link.Description).HasMaxLength(500);
                entity.Property(link => link.Icon).HasMaxLength(1000);
                entity.HasOne(link => link.LinkGroup)
                    .WithMany(group => group.Links)
                    .HasForeignKey(link => link.LinkGroupId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PortalModule>(entity =>
            {
                entity.Property(module => module.Type).HasMaxLength(80).IsRequired();
                entity.Property(module => module.Title).HasMaxLength(200).IsRequired();
                entity.HasOne(module => module.PortalPage)
                    .WithMany(page => page.Modules)
                    .HasForeignKey(module => module.PortalPageId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<EmployeeBirthday>(entity =>
            {
                entity.Property(employee => employee.FullName).HasMaxLength(250).IsRequired();
                entity.Property(employee => employee.Department).HasMaxLength(250);
            });
        }
    }
}
