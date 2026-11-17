using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExamGuard.Models
{
    public class InvigilationDuty
    {
        [Key]
        public int DutyId { get; set; }

        [Required]
        public int ExamId { get; set; }

        [ForeignKey("ExamId")]
        public Exam? Exam { get; set; }

        [Required]
        public string AssignedUserId { get; set; } = string.Empty;

        [ForeignKey("AssignedUserId")]
        public ApplicationUser? AssignedUser { get; set; }

        // Status values: "Assigned", "SwapRequested", "Completed"
        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Assigned";
    }
}