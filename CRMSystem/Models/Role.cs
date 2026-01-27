using System.ComponentModel.DataAnnotations;

namespace CRMSystem.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Role")]
        public string RoleName { get; set; } = string.Empty;
    }
}
