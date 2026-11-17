using System.ComponentModel.DataAnnotations;

namespace ExamGuard.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Employee or Roll Number is required")]
        [StringLength(30)]
        public string EmployeeOrRollNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a department")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Faculty"; // Default to Faculty/TA
    }
}