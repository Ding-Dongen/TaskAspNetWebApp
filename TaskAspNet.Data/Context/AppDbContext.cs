using Microsoft.EntityFrameworkCore;
using TaskAspNet.Data.Entities;

namespace TaskAspNet.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<MemberEntity> Members { get; set; }
    public DbSet<ProjectEntity> Projects { get; set; }
    public DbSet<ClientEntity> Clients { get; set; }
    public DbSet<JobTitleEntity> JobTitles { get; set; }
    public DbSet<ProjectStatusEntity> ProjectStatuses { get; set; }
    public DbSet<ProjectMemberEntity> ProjectMembers { get; set; }
    public DbSet<MemberPhoneEntity> MemberPhones { get; set; }
    public DbSet<MemberAddressEntity> MemberAddresses { get; set; }
    public DbSet<NotificationEntity> Notifications { get; set; }
    public DbSet<NotificationTypeEntity> NotificationTypes { get; set; }
    public DbSet<NotificationTargetGroupEntity> NotificationTargetGroups { get; set; }
    public DbSet<NotificationDismissalEntity> NotificationDismissals { get; set; }
    public DbSet<Consent> Consents { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        
        modelBuilder.Entity<Consent>(entity =>
        {
            entity.ToTable("Consent", "dbo");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.IsConsentGiven).IsRequired();
            entity.Property(e => e.FunctionalCookies).IsRequired();
            entity.Property(e => e.AnalyticsCookies).IsRequired();
            entity.Property(e => e.MarketingCookies).IsRequired();
            entity.Property(e => e.AdvertisingCookies).IsRequired();
            entity.Property(e => e.DateGiven).IsRequired();
        });

        modelBuilder.Entity<NotificationDismissalEntity>()
        .HasKey(nd => new { nd.NotificationId, nd.UserId });

        modelBuilder.Entity<NotificationEntity>()
           .HasOne(n => n.NotificationType)
           .WithMany(nt => nt.Notifications)
           .HasForeignKey(n => n.NotificationTypeId)
           .OnDelete(DeleteBehavior.Restrict);

        // NotificationTypeEntity to NotificationTargetGroupEntity
        modelBuilder.Entity<NotificationTypeEntity>()
            .HasOne(nt => nt.TargetGroup)
            .WithMany(tg => tg.NotificationTypes)
            .HasForeignKey(nt => nt.TargetGroupId)
            .OnDelete(DeleteBehavior.Restrict);

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

            modelBuilder.Entity<MemberAddressEntity>()
            .HasOne(ma => ma.Member)
            .WithMany(m => m.Addresses)
            .HasForeignKey(ma => ma.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MemberAddressEntity>()
            .HasOne(ma => ma.Client)
            .WithMany(c => c.Addresses)
            .HasForeignKey(ma => ma.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MemberPhoneEntity>()
            .HasOne(mp => mp.Member)
            .WithMany(m => m.Phones)
            .HasForeignKey(mp => mp.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MemberPhoneEntity>()
            .HasOne(mp => mp.Client)
            .WithMany(c => c.Phones)
            .HasForeignKey(mp => mp.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProjectEntity>()
            .HasOne(p => p.Client)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Restrict);




        //  Seed Job Titles
        modelBuilder.Entity<JobTitleEntity>().HasData(
            new JobTitleEntity { Id = 1, Title = "Developer" },
            new JobTitleEntity { Id = 2, Title = "Designer" },
            new JobTitleEntity { Id = 3, Title = "Project Manager" }
        );

        //  Seed Project Statuses
        modelBuilder.Entity<ProjectStatusEntity>().HasData(
            new ProjectStatusEntity { Id = 1, StatusName = "Started" },
            new ProjectStatusEntity { Id = 2, StatusName = "Completed" }
        );

        // Seed Clients
        modelBuilder.Entity<ClientEntity>().HasData(
            new ClientEntity { Id = 1, ClientName = "Acme Corporation" },
            new ClientEntity { Id = 2, ClientName = "TechStart Inc." },
            new ClientEntity { Id = 3, ClientName = "Global Solutions Ltd." }
        );

        // Seed Members
        modelBuilder.Entity<MemberEntity>().HasData(
            new MemberEntity
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                JobTitleId = 1,
                DateOfBirth = new DateTime(1990, 1, 1),
                UserId = "user-1"
            },
            new MemberEntity
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                JobTitleId = 2,
                DateOfBirth = new DateTime(1992, 3, 15),
                UserId = "user-2"
            },
            new MemberEntity
            {
                Id = 3,
                FirstName = "Mike",
                LastName = "Johnson",
                Email = "mike.johnson@example.com",
                JobTitleId = 3,
                DateOfBirth = new DateTime(1988, 7, 22),
                UserId = "user-3"
            }
        );

        // Seed Member Phones
        modelBuilder.Entity<MemberPhoneEntity>().HasData(
            new MemberPhoneEntity { Id = 1, MemberId = 1, Phone = "555-0101", PhoneType = "Mobile" },
            new MemberPhoneEntity { Id = 2, MemberId = 1, Phone = "555-0102", PhoneType = "Work" },

            new MemberPhoneEntity { Id = 3, MemberId = 2, Phone = "555-0201", PhoneType = "Mobile" },

            new MemberPhoneEntity { Id = 4, MemberId = 3, Phone = "555-0301", PhoneType = "Mobile" },
            new MemberPhoneEntity { Id = 5, MemberId = 3, Phone = "555-0302", PhoneType = "Work" }
        );

        // Seed Member Addresses
        modelBuilder.Entity<MemberAddressEntity>().HasData(
            new MemberAddressEntity { Id = 1, MemberId = 1, Address = "123 Main St", ZipCode = "12345", City = "New York", AddressType = "Home" },
            new MemberAddressEntity { Id = 2, MemberId = 1, Address = "456 Elm St", ZipCode = "54321", City = "New York", AddressType = "Work" },

            new MemberAddressEntity { Id = 3, MemberId = 2, Address = "789 Oak Ave", ZipCode = "67890", City = "Los Angeles", AddressType = "Home" },

            new MemberAddressEntity { Id = 4, MemberId = 3, Address = "135 Pine Rd", ZipCode = "13579", City = "Chicago", AddressType = "Home" },
            new MemberAddressEntity { Id = 5, MemberId = 3, Address = "246 Maple St", ZipCode = "97531", City = "Chicago", AddressType = "Work" }
        );

        // Seed Projects
        modelBuilder.Entity<ProjectEntity>().HasData(
            new ProjectEntity
            {
                Id = 1,
                Name = "Website Redesign",
                Description = "Complete overhaul of company website",
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 6, 30),
                ClientId = 1,
                StatusId = 1,
                Budget = 50000.00m
            },
            new ProjectEntity
            {
                Id = 2,
                Name = "Mobile App Development",
                Description = "New iOS and Android app development",
                StartDate = new DateTime(2024, 2, 1),
                EndDate = new DateTime(2024, 8, 31),
                ClientId = 2,
                StatusId = 1,
                Budget = 75000.00m
            },
            new ProjectEntity
            {
                Id = 3,
                Name = "E-commerce Platform",
                Description = "Online store development",
                StartDate = new DateTime(2024, 3, 1),
                EndDate = new DateTime(2024, 9, 30),
                ClientId = 3,
                StatusId = 1,
                Budget = 100000.00m
            }
        );

        // Seed Project Members
        modelBuilder.Entity<ProjectMemberEntity>().HasData(
            new ProjectMemberEntity { ProjectId = 1, MemberId = 1 },
            new ProjectMemberEntity { ProjectId = 1, MemberId = 2 },
            new ProjectMemberEntity { ProjectId = 2, MemberId = 2 },
            new ProjectMemberEntity { ProjectId = 2, MemberId = 3 },
            new ProjectMemberEntity { ProjectId = 3, MemberId = 1 },
            new ProjectMemberEntity { ProjectId = 3, MemberId = 3 }
        );

        modelBuilder.Entity<NotificationTargetGroupEntity>().HasData(
            new NotificationTargetGroupEntity
            {
                Id = 1,
                Name = "Users",
                Description = "All user-level notifications",
                Roles = "User,Admin,SuperAdmin",
                IsActive = true
            },
            new NotificationTargetGroupEntity
            {
                Id = 2,
                Name = "Administrators",
                Description = "All admin-level notifications",
                Roles = "Admin,SuperAdmin",
                IsActive = true
            },
            new NotificationTargetGroupEntity
            {
                Id = 3,
                Name = "Clients",
                Description = "All client-level notifications",
                Roles = "User,Admin,SuperAdmin",
                IsActive = true
            }
        );

        modelBuilder.Entity<NotificationTypeEntity>().HasData(
        // Member events to Administrators
            new NotificationTypeEntity
            {
                Id = 1,
                Name = "MemberCreated",
                Description = "Fires when a new member is created",
                DefaultMessageTemplate = "New member '{0}' has been created",
                TargetGroupId = 2,
                IsActive = true
            },
            new NotificationTypeEntity
            {
                Id = 2,
                Name = "MemberUpdated",
                Description = "Fires when a member is updated",
                DefaultMessageTemplate = "Member '{0}' has been updated",
                TargetGroupId = 2,
                IsActive = true
            },

            // Project events to Users
            new NotificationTypeEntity
            {
                Id = 3,
                Name = "ProjectCreated",
                Description = "Fires when a new project is created",
                DefaultMessageTemplate = "New project '{0}' has been created",
                TargetGroupId = 1,
                IsActive = true
            },
            new NotificationTypeEntity
            {
                Id = 4,
                Name = "ProjectUpdated",
                Description = "Fires when a project is updated",
                DefaultMessageTemplate = "Project '{0}' has been updated",
                TargetGroupId = 1,
                IsActive = true
            },

            // Project membership events to Administrators
            new NotificationTypeEntity
            {
                Id = 5,
                Name = "MemberAddedToProject",
                Description = "Fires when a member is added to a project",
                DefaultMessageTemplate = "Member '{0}' added to project '{1}'",
                TargetGroupId = 2,
                IsActive = true
            },
            new NotificationTypeEntity
            {
                Id = 6,
                Name = "MemberRemovedFromProject",
                Description = "Fires when a member is removed from a project",
                DefaultMessageTemplate = "Member '{0}' removed from project '{1}'",
                TargetGroupId = 2,
                IsActive = true
            },

            // Client events to Clients
            new NotificationTypeEntity
            {
                Id = 7,
                Name = "ClientCreated",
                Description = "Fires when a new client is created",
                DefaultMessageTemplate = "New client '{0}' has been created",
                TargetGroupId = 3,
                IsActive = true
            },
            new NotificationTypeEntity
            {
                Id = 8,
                Name = "ClientUpdated",
                Description = "Fires when a client is updated",
                DefaultMessageTemplate = "Client '{0}' has been updated",
                TargetGroupId = 3,
                IsActive = true
            }
        );



    }
}