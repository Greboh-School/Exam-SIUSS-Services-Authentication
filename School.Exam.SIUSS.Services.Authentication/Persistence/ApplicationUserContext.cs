using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using School.Exam.SIUSS.Services.Authentication.Models.Entities;

namespace School.Exam.SIUSS.Services.Authentication.Persistence;

public class ApplicationUserContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }

    public ApplicationUserContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .HasIndex(x => x.UserName)
            .IsUnique();
    }
}