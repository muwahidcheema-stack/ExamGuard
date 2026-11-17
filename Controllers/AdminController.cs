using ExamGuard.Data;
using ExamGuard.Models;
using ExamGuard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExamGuard.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var staffUsers = await _userManager.GetUsersInRoleAsync("Faculty");

            var viewModel = new AdminDashboardViewModel
            {
                TotalExams = await _context.Exams.CountAsync(),
                TotalDuties = await _context.InvigilationDuties.CountAsync(),
                PendingSwaps = await _context.SwapRequests.CountAsync(s => s.Status == "Pending"),
                Duties = await _context.InvigilationDuties
                    .Include(d => d.Exam)
                    .Include(d => d.AssignedUser)
                    .OrderBy(d => d.Exam!.ExamDate)
                    .ToListAsync(),
                StaffMembers = staffUsers.ToList(),
                AllSwapRequests = await _context.SwapRequests
                    .Include(s => s.OriginalDuty)!.ThenInclude(d => d!.Exam)
                    .Include(s => s.RequesterUser)
                    .Include(s => s.TargetUser)
                    .Where(s => s.Status == "Pending")
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExamAndDuty(string CourseName, string CourseCode, DateTime ExamDate, string StartTime, string EndTime, string Venue, string AssignedUserId)
        {
            if (string.IsNullOrWhiteSpace(CourseName) || string.IsNullOrWhiteSpace(AssignedUserId))
                return RedirectToAction(nameof(Index));

            var exam = new Exam
            {
                CourseName = CourseName,
                CourseCode = CourseCode,
                ExamDate = ExamDate,
                StartTime = StartTime,
                EndTime = EndTime,
                Venue = Venue,
                IsPublished = true
            };

            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            var duty = new InvigilationDuty
            {
                ExamId = exam.ExamId,
                AssignedUserId = AssignedUserId,
                Status = "Assigned"
            };

            _context.InvigilationDuties.Add(duty);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PublishAll()
        {
            var unpublished = await _context.Exams.Where(e => !e.IsPublished).ToListAsync();
            foreach (var exam in unpublished)
            {
                exam.IsPublished = true;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDuty(int dutyId)
        {
            var duty = await _context.InvigilationDuties.Include(d => d.Exam).FirstOrDefaultAsync(d => d.DutyId == dutyId);
            if (duty != null)
            {
                var relatedSwaps = _context.SwapRequests.Where(s => s.OriginalDutyId == dutyId || s.TargetDutyId == dutyId);
                _context.SwapRequests.RemoveRange(relatedSwaps);
                _context.InvigilationDuties.Remove(duty);
                if (duty.Exam != null)
                {
                    _context.Exams.Remove(duty.Exam);
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}