using System.ComponentModel.DataAnnotations;

namespace CRMSystem.Models
{
    public class ContactStatus
    {
        [Key]
        public int ContactStatusId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Status Name")]
        public string Name { get; set; } = string.Empty;

        // Navigation property
        public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    }
}
