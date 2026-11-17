using ExamGuard.Models;

namespace ExamGuard.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalExams { get; set; }
        public int TotalDuties { get; set; }
        public int PendingSwaps { get; set; }
        public List<InvigilationDuty> Duties { get; set; } = new();
        public List<ApplicationUser> StaffMembers { get; set; } = new();
        public List<SwapRequest> AllSwapRequests { get; set; } = new();
    }
}