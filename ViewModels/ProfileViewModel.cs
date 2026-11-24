using System.ComponentModel.DataAnnotations;

namespace ExamGuard.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Employee or Roll Number is required")]
        [StringLength(30)]
        public string EmployeeOrRollNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a department")]
        public string Department { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}