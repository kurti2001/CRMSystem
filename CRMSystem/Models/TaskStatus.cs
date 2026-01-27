using System.ComponentModel.DataAnnotations;

namespace CRMSystem.Models
{
    public class TaskStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
