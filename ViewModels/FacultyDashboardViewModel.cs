using ExamGuard.Models;

namespace ExamGuard.ViewModels
{
    public class FacultyDashboardViewModel
    {
        public ApplicationUser CurrentUser { get; set; } = new();
        public List<InvigilationDuty> MyDuties { get; set; } = new();
        public List<SwapRequest> IncomingSwaps { get; set; } = new();
        public List<SwapRequest> OpenSwapBoard { get; set; } = new();
        public List<SwapRequest> SwapHistory { get; set; } = new();
        public List<ApplicationUser> Colleagues { get; set; } = new();
    }
}