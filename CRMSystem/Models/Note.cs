// File: Models/Note.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMSystem.Models
{
    public enum NoteType
    {
        [Display(Name = "Note")]
        Note,
        [Display(Name = "Task")]
        Task,
        [Display(Name = "Meeting")]
        Meeting
    }

    public class Note
    {
        [Key]
        public int NoteId { get; set; }

        [Required]
        [StringLength(4000)]
        [Display(Name = "Content")]
        public string Content { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Type")]
        public NoteType Type { get; set; } = NoteType.Note;

        [Display(Name = "Due Date")]
        [DataType(DataType.DateTime)]
        public DateTime? DueDate { get; set; }

        [Display(Name = "Completed")]
        public bool IsCompleted { get; set; } = false;

        [Display(Name = "Completed At")]
        public DateTime? CompletedAt { get; set; }

        [Required]
        [Display(Name = "Contact")]
        public int ContactId { get; set; }

        [Required]
        [Display(Name = "Author")]
        public string AuthorId { get; set; } = string.Empty;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ContactId")]
        public Contact? Contact { get; set; }

        [ForeignKey("AuthorId")]
        public ApplicationUser? Author { get; set; }
    }
}
