using ExamGuard.Data;
using ExamGuard.Models;
using ExamGuard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExamGuard.Controllers
{
    [Authorize(Roles = "Faculty")]
    public class FacultyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FacultyController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var allFaculty = await _userManager.GetUsersInRoleAsync("Faculty");

            var viewModel = new FacultyDashboardViewModel
            {
                CurrentUser = currentUser,
                MyDuties = await _context.InvigilationDuties
                    .Include(d => d.Exam)
                    .Where(d => d.AssignedUserId == currentUser.Id)
                    .OrderBy(d => d.Exam!.ExamDate)
                    .ToListAsync(),

                IncomingSwaps = await _context.SwapRequests
                    .Include(s => s.OriginalDuty)!.ThenInclude(d => d!.Exam)
                    .Include(s => s.RequesterUser)
                    .Where(s => s.TargetUserId == currentUser.Id && s.Status == "Pending")
                    .ToListAsync(),

                OpenSwapBoard = await _context.SwapRequests
                    .Include(s => s.OriginalDuty)!.ThenInclude(d => d!.Exam)
                    .Include(s => s.RequesterUser)
                    .Where(s => s.TargetUserId == null && s.RequesterUserId != currentUser.Id && s.Status == "Pending")
                    .ToListAsync(),

                SwapHistory = await _context.SwapRequests
                    .Include(s => s.OriginalDuty)!.ThenInclude(d => d!.Exam)
                    .Include(s => s.RequesterUser)
                    .Include(s => s.TargetUser)
                    .Where(s => (s.RequesterUserId == currentUser.Id || s.TargetUserId == currentUser.Id) && s.Status != "Pending")
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync(),

                Colleagues = allFaculty.Where(u => u.Id != currentUser.Id).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestSwap(int OriginalDutyId, string SwapType, string? TargetUserId, string? Reason)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var duty = await _context.InvigilationDuties.FindAsync(OriginalDutyId);
            if (duty == null || duty.AssignedUserId != currentUser.Id)
                return RedirectToAction(nameof(Index));

            var swapRequest = new SwapRequest
            {
                OriginalDutyId = OriginalDutyId,
                RequesterUserId = currentUser.Id,
                TargetUserId = SwapType == "open" ? null : TargetUserId,
                Reason = Reason,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            duty.Status = "SwapRequested";
            _context.SwapRequests.Add(swapRequest);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RespondSwap(int swapRequestId, string decision)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var swap = await _context.SwapRequests
                .Include(s => s.OriginalDuty)
                .FirstOrDefaultAsync(s => s.SwapRequestId == swapRequestId);

            if (swap == null || swap.Status != "Pending")
                return RedirectToAction(nameof(Index));

            if (decision == "Accept")
            {
                swap.Status = "Accepted";
                swap.TargetUserId = currentUser.Id;

                if (swap.OriginalDuty != null)
                {
                    swap.OriginalDuty.AssignedUserId = currentUser.Id;
                    swap.OriginalDuty.Status = "Assigned";
                }
            }
            else if (decision == "Decline")
            {
                swap.Status = "Rejected";
                if (swap.OriginalDuty != null)
                {
                    swap.OriginalDuty.Status = "Assigned";
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}