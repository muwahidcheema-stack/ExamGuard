using System.ComponentModel.DataAnnotations;

namespace ExamGuard.Models
{
    public class Exam
    {
        [Key]
        public int ExamId { get; set; }

        [Required]
        [StringLength(100)]
        public string CourseName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime ExamDate { get; set; }

        [Required]
        [StringLength(20)]
        public string StartTime { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string EndTime { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Venue { get; set; } = string.Empty;

        public bool IsPublished { get; set; } = false;

        // Navigation property
        public ICollection<InvigilationDuty> InvigilationDuties { get; set; } = new List<InvigilationDuty>();
    }
}