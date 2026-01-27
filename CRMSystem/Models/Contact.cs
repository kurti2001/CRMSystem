using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMSystem.Models
{
    public class Contact
    {
        [Key]
        [Column("id")]
        public int ContactId { get; set; }

        [StringLength(20)]
        [Display(Name = "Title")]
        [Column("Contact_Title")]
        public string? ContactTitle { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        [Column("Contact_First")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Middle Name")]
        [Column("Contact_Middle")]
        public string? MiddleName { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        [Column("Contact_Last")]
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        public string FullName => string.IsNullOrEmpty(MiddleName)
            ? $"{FirstName} {LastName}"
            : $"{FirstName} {MiddleName} {LastName}";

        [StringLength(200)]
        [Display(Name = "Lead Referral Source")]
        [Column("Lead_Referral_Source")]
        public string? LeadReferralSource { get; set; }

        [Display(Name = "Date of Initial Contact")]
        [DataType(DataType.Date)]
        [Column("Date_of_Initial_Contact")]
        public DateTime? DateOfInitialContact { get; set; }

        [StringLength(100)]
        [Display(Name = "Job Title")]
        [Column("Title")]
        public string? Title { get; set; }

        [StringLength(200)]
        [Display(Name = "Company")]
        [Column("Company")]
        public string? Company { get; set; }

        [StringLength(100)]
        [Display(Name = "Industry")]
        [Column("Industry")]
        public string? Industry { get; set; }

        [StringLength(500)]
        [Display(Name = "Address")]
        [Column("Address")]
        public string? Address { get; set; }

        [StringLength(200)]
        [Display(Name = "Street Address 1")]
        [Column("Address_Street_1")]
        public string? AddressStreet1 { get; set; }

        [StringLength(200)]
        [Display(Name = "Street Address 2")]
        [Column("Address_Street_2")]
        public string? AddressStreet2 { get; set; }

        [StringLength(100)]
        [Display(Name = "City")]
        [Column("Address_City")]
        public string? AddressCity { get; set; }

        [StringLength(100)]
        [Display(Name = "State")]
        [Column("Address_State")]
        public string? AddressState { get; set; }

        [StringLength(20)]
        [Display(Name = "Zip Code")]
        [Column("Address_Zip")]
        public string? AddressZip { get; set; }

        [StringLength(100)]
        [Display(Name = "Country")]
        [Column("Address_Country")]
        public string? AddressCountry { get; set; }

        [StringLength(20)]
        [Phone]
        [Display(Name = "Phone Number")]
        [Column("Phone")]
        public string? Phone { get; set; }

        [Required]
        [StringLength(256)]
        [EmailAddress]
        [Display(Name = "Email Address")]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Status")]
        [Column("Status")]
        public int ContactStatusId { get; set; }

        [StringLength(500)]
        [Display(Name = "Website")]
        [Url]
        [Column("Website")]
        public string? Website { get; set; }

        [StringLength(500)]
        [Display(Name = "LinkedIn Profile")]
        [Url]
        [Column("LinkedIn_Profile")]
        public string? LinkedInProfile { get; set; }

        [StringLength(4000)]
        [Display(Name = "Background Info")]
        [Column("Background_Info")]
        public string? BackgroundInfo { get; set; }

        [Required]
        [Display(Name = "Sales Rep")]
        [Column("Sales_Rep")]
        public string SalesRepId { get; set; } = string.Empty;

        [Display(Name = "Rating")]
        [Range(1, 5)]
        [Column("Rating")]
        public int? Rating { get; set; }

        [StringLength(100)]
        [Display(Name = "Project Type")]
        [Column("Project_Type")]
        public string? ProjectType { get; set; }

        [StringLength(2000)]
        [Display(Name = "Project Description")]
        [Column("Project_Description")]
        public string? ProjectDescription { get; set; }

        [Display(Name = "Proposal Due Date")]
        [DataType(DataType.Date)]
        [Column("Proposal_Due_Date")]
        public DateTime? ProposalDueDate { get; set; }

        [Display(Name = "Budget")]
        [Column("Budget")]
        [DataType(DataType.Currency)]
        public decimal? Budget { get; set; }

        [StringLength(2000)]
        [Display(Name = "Deliverables")]
        [Column("Deliverables")]
        public string? Deliverables { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ContactStatusId")]
        public ContactStatus? ContactStatus { get; set; }

        [ForeignKey("SalesRepId")]
        public ApplicationUser? SalesRep { get; set; }

        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
