using System.ComponentModel.DataAnnotations;

namespace CRMSystem.Models
{
    public class TodoDesc
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
