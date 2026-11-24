using ExamGuard.Models;
using ExamGuard.Repositories.Interfaces;
using ExamGuard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ExamGuard.Controllers
{
    [Authorize(Roles = "Faculty")]
    public class FacultyController : Controller
    {
        private readonly IDutyRepository _dutyRepo;
        private readonly ISwapRepository _swapRepo;
        private readonly IUserRepository _userRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public FacultyController(
            IDutyRepository dutyRepo,
            ISwapRepository swapRepo,
            IUserRepository userRepo,
            UserManager<ApplicationUser> userManager)
        {
            _dutyRepo = dutyRepo;
            _swapRepo = swapRepo;
            _userRepo = userRepo;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return RedirectToAction("Login", "Account");

            var currentUser = await _userRepo.GetUserByIdAsync(userId);
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var viewModel = new FacultyDashboardViewModel
            {
                CurrentUser = currentUser,
                MyDuties = (await _dutyRepo.GetDutiesByUserIdAsync(userId)).ToList(),
                IncomingSwaps = (await _swapRepo.GetIncomingSwapsForUserAsync(userId)).ToList(),
                OpenSwapBoard = (await _swapRepo.GetOpenSwapBoardAsync(userId)).ToList(),
                SwapHistory = (await _swapRepo.GetSwapHistoryForUserAsync(userId)).ToList(),
                Colleagues = (await _userRepo.GetColleaguesAsync(userId)).ToList()
            };

            return View(viewModel);
        }

        // $.ajax Endpoint: Submit Swap Request
        [HttpPost]
        public async Task<IActionResult> RequestSwapAjax([FromForm] int OriginalDutyId, [FromForm] string SwapType, [FromForm] string? TargetUserId, [FromForm] string? Reason)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { success = false, message = "Unauthorized" });

            var duty = await _dutyRepo.GetDutyByIdAsync(OriginalDutyId);
            if (duty == null || duty.AssignedUserId != userId)
                return Json(new { success = false, message = "Duty not found." });

            var swapRequest = new SwapRequest
            {
                OriginalDutyId = OriginalDutyId,
                RequesterUserId = userId,
                TargetUserId = SwapType == "open" ? null : TargetUserId,
                Reason = Reason,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            await _swapRepo.CreateSwapRequestAsync(swapRequest);
            await _dutyRepo.UpdateDutyStatusAsync(OriginalDutyId, "SwapRequested");

            return Json(new { success = true, message = "Swap request sent successfully!" });
        }

        // $.post Endpoint: Accept or Decline Swap
        [HttpPost]
        public async Task<IActionResult> RespondSwapAjax(int swapRequestId, string decision)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { success = false });

            var swap = await _swapRepo.GetSwapByIdAsync(swapRequestId);
            if (swap == null || swap.Status != "Pending")
                return Json(new { success = false, message = "Swap request already processed." });

            if (decision == "Accept")
            {
                await _swapRepo.UpdateSwapStatusAsync(swapRequestId, "Accepted", userId);
                await _dutyRepo.ReassignDutyAsync(swap.OriginalDutyId, userId, "Assigned");
                return Json(new { success = true, message = "Duty swap accepted! Your timetable has been updated." });
            }
            else if (decision == "Decline")
            {
                await _swapRepo.UpdateSwapStatusAsync(swapRequestId, "Rejected");
                await _dutyRepo.UpdateDutyStatusAsync(swap.OriginalDutyId, "Assigned");
                return Json(new { success = true, message = "Swap request declined." });
            }

            return Json(new { success = false });
        }
    }
}