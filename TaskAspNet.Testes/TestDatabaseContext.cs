using Microsoft.EntityFrameworkCore;
using TaskAspNet.Data.Entities;

namespace TaskAspNet.Tests;

public class TestDatabaseContext : DbContext
{
    public TestDatabaseContext(DbContextOptions<TestDatabaseContext> options)
        : base(options)
    {
    }

    public DbSet<ProjectEntity> Projects { get; set; }
    public DbSet<ClientEntity> Clients { get; set; }
    public DbSet<ProjectStatusEntity> ProjectStatuses { get; set; }
    public DbSet<MemberEntity> Members { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProjectEntity>()
            .HasOne(p => p.Client)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => p.ClientId);

        modelBuilder.Entity<ProjectEntity>()
            .HasOne(p => p.Status)
            .WithMany()
            .HasForeignKey(p => p.StatusId);

        modelBuilder.Entity<ProjectMemberEntity>()
            .HasKey(pm => new { pm.ProjectId, pm.MemberId });

        modelBuilder.Entity<ProjectMemberEntity>()
            .HasOne(pm => pm.Project)
            .WithMany(p => p.ProjectMembers)
            .HasForeignKey(pm => pm.ProjectId);

        modelBuilder.Entity<ProjectMemberEntity>()
            .HasOne(pm => pm.Member)
            .WithMany(m => m.ProjectMembers)
            .HasForeignKey(pm => pm.MemberId);

        modelBuilder.Entity<ProjectStatusEntity>().HasData(
            new ProjectStatusEntity { Id = 1, StatusName = "Active" },
            new ProjectStatusEntity { Id = 2, StatusName = "Completed" },
            new ProjectStatusEntity { Id = 3, StatusName = "On Hold" }
        );

        modelBuilder.Entity<ClientEntity>().HasData(
            new ClientEntity { Id = 1, ClientName = "Test Client 1" },
            new ClientEntity { Id = 2, ClientName = "Test Client 2" }
        );

        modelBuilder.Entity<ProjectEntity>().HasData(
            new ProjectEntity
            {
                Id = 1,
                Name = "Test Project 1",
                Description = "Test Description 1",
                ClientId = 1,
                StatusId = 1,
                StartDate = DateTime.Today,    
                Budget = 1000m
            }
        );
    }

}