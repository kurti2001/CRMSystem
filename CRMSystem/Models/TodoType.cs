using System.ComponentModel.DataAnnotations;

namespace CRMSystem.Models
{
    public class TodoType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Type")]
        public string Type { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
