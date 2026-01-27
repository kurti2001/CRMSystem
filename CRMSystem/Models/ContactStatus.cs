using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMSystem.Models
{
    public class ContactStatus
    {
        [Key]
        [Column("id")]
        public int ContactStatusId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Status Name")]
        [Column("status")]
        public string Name { get; set; } = string.Empty;

        // Navigation property
        public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    }
}
