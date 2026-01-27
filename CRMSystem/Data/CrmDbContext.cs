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
            });

            // ContactStatus configuration
            builder.Entity<ContactStatus>(entity =>
            {
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

                entity.Property(c => c.Address)
                    .HasMaxLength(500);

                entity.Property(c => c.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(c => c.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(c => c.Email);

                entity.HasIndex(c => c.AssignedToId);

                entity.HasOne(c => c.AssignedTo)
                    .WithMany(u => u.Contacts)
                    .HasForeignKey(c => c.AssignedToId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(c => c.Notes)
                    .WithOne(n => n.Contact)
                    .HasForeignKey(n => n.ContactId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Note configuration
            builder.Entity<Note>(entity =>
            {
                entity.HasKey(n => n.NoteId);

                entity.Property(n => n.Content)
                    .IsRequired()
                    .HasMaxLength(4000);

                entity.Property(n => n.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(n => n.ContactId);

                entity.HasIndex(n => n.AuthorId);

                entity.HasOne(n => n.Author)
                    .WithMany(u => u.Notes)
                    .HasForeignKey(n => n.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed data for ContactStatus
            builder.Entity<ContactStatus>().HasData(
                new ContactStatus
                {
                    ContactStatusId = 1,
                    Name = "Lead"
                },
                new ContactStatus
                {
                    ContactStatusId = 2,
                    Name = "Opportunity"
                },
                new ContactStatus
                {
                    ContactStatusId = 3,
                    Name = "Customer"
                }
            );
        }
    }
}
