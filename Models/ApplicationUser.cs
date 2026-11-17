using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ExamGuard.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string EmployeeOrRollNumber { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<InvigilationDuty> AssignedDuties { get; set; } = new List<InvigilationDuty>();
    }
}