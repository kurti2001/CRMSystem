using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(20)]
        [Display(Name = "Title")]
        [Column("Name_Title")]
        public string? NameTitle { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        [Column("Name_First")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Middle Name")]
        [Column("Name_Middle")]
        public string? MiddleName { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        [Column("Name_Last")]
        public string LastName { get; set; } = string.Empty;

        public string FullName => string.IsNullOrEmpty(MiddleName)
            ? $"{FirstName} {LastName}"
            : $"{FirstName} {MiddleName} {LastName}";

        [Display(Name = "User Roles")]
        [Column("User_Roles")]
        public int? UserRolesId { get; set; }

        [Display(Name = "User Status")]
        [Column("User_Status")]
        public int? UserStatusId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("UserStatusId")]
        public UserStatus? UserStatus { get; set; }

        public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
