using CRMSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CRMSystem.Data
{
    public class CrmDbContext : IdentityDbContext<ApplicationUser>
    {
        public CrmDbContext(DbContextOptions<CrmDbContext> options)
            : base(options)
        {
        }

        public DbSet<Contact> Contacts { get; set; }
        public DbSet<ContactStatus> ContactStatuses { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<TodoType> TodoTypes { get; set; }
        public DbSet<TodoDesc> TodoDescs { get; set; }
        public DbSet<Models.TaskStatus> TaskStatuses { get; set; }
        public DbSet<UserStatus> UserStatuses { get; set; }
        public DbSet<Role> CrmRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ApplicationUser configuration
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(u => u.UserStatus)
                    .WithMany(us => us.Users)
                    .HasForeignKey(u => u.UserStatusId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ContactStatus configuration
            builder.Entity<ContactStatus>(entity =>
            {
                entity.ToTable("contact_status");
                entity.HasKey(cs => cs.ContactStatusId);

                entity.Property(cs => cs.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(cs => cs.Name)
                    .IsUnique();

                entity.HasMany(cs => cs.Contacts)
                    .WithOne(c => c.ContactStatus)
                    .HasForeignKey(c => c.ContactStatusId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Contact configuration
            builder.Entity<Contact>(entity =>
            {
                entity.ToTable("contact");
                entity.HasKey(c => c.ContactId);

                entity.Property(c => c.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(c => c.Phone)
                    .HasMaxLength(20);

                entity.Property(c => c.Company)
                    .HasMaxLength(200);

                entity.Property(c => c.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(c => c.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(c => c.Email);

                entity.HasIndex(c => c.SalesRepId);

                entity.HasOne(c => c.SalesRep)
                    .WithMany(u => u.Contacts)
                    .HasForeignKey(c => c.SalesRepId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(c => c.Notes)
                    .WithOne(n => n.Contact)
                    .HasForeignKey(n => n.ContactId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(c => c.Budget)
                    .HasPrecision(18, 2);
            });

            // Note configuration
            builder.Entity<Note>(entity =>
            {
                entity.ToTable("notes");
                entity.HasKey(n => n.NoteId);

                entity.Property(n => n.Content)
                    .IsRequired()
                    .HasMaxLength(4000);

                entity.Property(n => n.Date)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(n => n.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(n => n.ContactId);

                entity.HasIndex(n => n.SalesRepId);

                entity.HasOne(n => n.SalesRep)
                    .WithMany(u => u.Notes)
                    .HasForeignKey(n => n.SalesRepId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(n => n.TodoType)
                    .WithMany(tt => tt.Notes)
                    .HasForeignKey(n => n.TodoTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(n => n.TodoDesc)
                    .WithMany(td => td.Notes)
                    .HasForeignKey(n => n.TodoDescId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(n => n.TaskStatus)
                    .WithMany(ts => ts.Notes)
                    .HasForeignKey(n => n.TaskStatusId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // TodoType configuration
            builder.Entity<TodoType>(entity =>
            {
                entity.ToTable("todo_type");
                entity.HasKey(tt => tt.Id);

                entity.Property(tt => tt.Type)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            // TodoDesc configuration
            builder.Entity<TodoDesc>(entity =>
            {
                entity.ToTable("todo_desc");
                entity.HasKey(td => td.Id);

                entity.Property(td => td.Description)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            // TaskStatus configuration
            builder.Entity<Models.TaskStatus>(entity =>
            {
                entity.ToTable("task_status");
                entity.HasKey(ts => ts.Id);

                entity.Property(ts => ts.Status)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            // UserStatus configuration
            builder.Entity<UserStatus>(entity =>
            {
                entity.ToTable("user_status");
                entity.HasKey(us => us.Id);

                entity.Property(us => us.Status)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            // Role configuration
            builder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");
                entity.HasKey(r => r.Id);

                entity.Property(r => r.RoleName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            // Seed data for ContactStatus (matching the schema: lead, proposal, customer/won, archive)
            builder.Entity<ContactStatus>().HasData(
                new ContactStatus { ContactStatusId = 1, Name = "lead" },
                new ContactStatus { ContactStatusId = 2, Name = "proposal" },
                new ContactStatus { ContactStatusId = 3, Name = "customer/won" },
                new ContactStatus { ContactStatusId = 4, Name = "archive" }
            );

            // Seed data for TodoType
            builder.Entity<TodoType>().HasData(
                new TodoType { Id = 1, Type = "Task" },
                new TodoType { Id = 2, Type = "Meeting" }
            );

            // Seed data for TodoDesc
            builder.Entity<TodoDesc>().HasData(
                new TodoDesc { Id = 1, Description = "Follow Up Email" },
                new TodoDesc { Id = 2, Description = "Phone Call" },
                new TodoDesc { Id = 3, Description = "Conference" },
                new TodoDesc { Id = 4, Description = "Meetup" },
                new TodoDesc { Id = 5, Description = "Tech Demo" }
            );

            // Seed data for TaskStatus
            builder.Entity<Models.TaskStatus>().HasData(
                new Models.TaskStatus { Id = 1, Status = "Pending" },
                new Models.TaskStatus { Id = 2, Status = "Completed" }
            );

            // Seed data for UserStatus
            builder.Entity<UserStatus>().HasData(
                new UserStatus { Id = 1, Status = "Active" },
                new UserStatus { Id = 2, Status = "Inactive" }
            );

            // Seed data for Role
            builder.Entity<Role>().HasData(
                new Role { Id = 1, RoleName = "Sales Rep" },
                new Role { Id = 2, RoleName = "Manager" }
            );
        }
    }
}
