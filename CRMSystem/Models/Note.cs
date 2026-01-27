using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMSystem.Models
{
    public class Note
    {
        [Key]
        [Column("id")]
        public int NoteId { get; set; }

        [Display(Name = "Date")]
        [Column("Date")]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(4000)]
        [Display(Name = "Notes")]
        [Column("Notes")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Is Todo")]
        [Column("Is_New_Todo")]
        public bool IsNewTodo { get; set; } = false;

        [Display(Name = "Todo Type")]
        [Column("Todo_Type_ID")]
        public int? TodoTypeId { get; set; }

        [Display(Name = "Todo Description")]
        [Column("Todo_Desc_ID")]
        public int? TodoDescId { get; set; }

        [Display(Name = "Due Date")]
        [DataType(DataType.DateTime)]
        [Column("Todo_Due_Date")]
        public DateTime? TodoDueDate { get; set; }

        [Required]
        [Display(Name = "Contact")]
        [Column("Contact")]
        public int ContactId { get; set; }

        [Display(Name = "Task Status")]
        [Column("Task_Status")]
        public int? TaskStatusId { get; set; }

        [StringLength(4000)]
        [Display(Name = "Task Update")]
        [Column("Task_Update")]
        public string? TaskUpdate { get; set; }

        [Required]
        [Display(Name = "Sales Rep")]
        [Column("Sales_Rep")]
        public string SalesRepId { get; set; } = string.Empty;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("ContactId")]
        public Contact? Contact { get; set; }

        [ForeignKey("SalesRepId")]
        public ApplicationUser? SalesRep { get; set; }

        [ForeignKey("TodoTypeId")]
        public TodoType? TodoType { get; set; }

        [ForeignKey("TodoDescId")]
        public TodoDesc? TodoDesc { get; set; }

        [ForeignKey("TaskStatusId")]
        public TaskStatus? TaskStatus { get; set; }
    }
}
