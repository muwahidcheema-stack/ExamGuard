using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExamGuard.Models
{
    public class SwapRequest
    {
        [Key]
        public int SwapRequestId { get; set; }

        [Required]
        public int OriginalDutyId { get; set; }

        [ForeignKey("OriginalDutyId")]
        public InvigilationDuty? OriginalDuty { get; set; }

        [Required]
        public string RequesterUserId { get; set; } = string.Empty;

        [ForeignKey("RequesterUserId")]
        public ApplicationUser? RequesterUser { get; set; }

        // Null if posted to the Open Department Swap Board
        public string? TargetUserId { get; set; }

        [ForeignKey("TargetUserId")]
        public ApplicationUser? TargetUser { get; set; }

        public int? TargetDutyId { get; set; }

        [ForeignKey("TargetDutyId")]
        public InvigilationDuty? TargetDuty { get; set; }

        [StringLength(250)]
        public string? Reason { get; set; }

        // Status values: "Pending", "Accepted", "Rejected", "Cancelled"
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}