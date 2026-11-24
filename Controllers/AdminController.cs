using ExamGuard.Models;
using ExamGuard.Repositories.Interfaces;
using ExamGuard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamGuard.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IExamRepository _examRepo;
        private readonly IDutyRepository _dutyRepo;
        private readonly ISwapRepository _swapRepo;
        private readonly IUserRepository _userRepo;

        public AdminController(
            IExamRepository examRepo,
            IDutyRepository dutyRepo,
            ISwapRepository swapRepo,
            IUserRepository userRepo)
        {
            _examRepo = examRepo;
            _dutyRepo = dutyRepo;
            _swapRepo = swapRepo;
            _userRepo = userRepo;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalExams = await _examRepo.GetTotalExamsCountAsync(),
                TotalDuties = await _dutyRepo.GetTotalDutiesCountAsync(),
                PendingSwaps = await _swapRepo.GetPendingSwapsCountAsync(),
                Duties = (await _dutyRepo.GetAllDutiesWithDetailsAsync()).ToList(),
                StaffMembers = (await _userRepo.GetAllFacultyAndTAsAsync()).ToList(),
                AllSwapRequests = (await _swapRepo.GetAllPendingSwapsAsync()).ToList()
            };

            return View(viewModel);
        }

        // 1. $.get Endpoint: Live Search Filter
        [HttpGet]
        public async Task<IActionResult> SearchDuties(string? query)
        {
            var duties = await _dutyRepo.GetAllDutiesWithDetailsAsync(query);
            var result = duties.Select(d => new
            {
                dutyId = d.DutyId,
                examId = d.ExamId,
                courseCode = d.Exam?.CourseCode,
                courseName = d.Exam?.CourseName,
                examDate = d.Exam?.ExamDate.ToString("MMM dd, yyyy"),
                timeSlot = $"{d.Exam?.StartTime} - {d.Exam?.EndTime}",
                venue = d.Exam?.Venue,
                isPublished = d.Exam?.IsPublished ?? false,
                assignedUserName = d.AssignedUser?.FullName,
                rollNo = d.AssignedUser?.EmployeeOrRollNumber,
                status = d.Status
            });

            return Json(result);
        }

        // 2. $.ajax Endpoint: Create Exam & Assign Duty
        [HttpPost]
        public async Task<IActionResult> CreateExamAjax([FromForm] string CourseName, [FromForm] string CourseCode, [FromForm] DateTime ExamDate, [FromForm] string StartTime, [FromForm] string EndTime, [FromForm] string Venue, [FromForm] string AssignedUserId)
        {
            if (string.IsNullOrWhiteSpace(CourseName) || string.IsNullOrWhiteSpace(AssignedUserId))
                return Json(new { success = false, message = "Please fill in all required fields." });

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

            int newExamId = await _examRepo.CreateExamAsync(exam);

            var duty = new InvigilationDuty
            {
                ExamId = newExamId,
                AssignedUserId = AssignedUserId,
                Status = "Assigned"
            };

            await _dutyRepo.CreateDutyAsync(duty);
            return Json(new { success = true, message = "Exam slot created and assigned!" });
        }

        // 3. $.post Endpoint: Toggle Publish Status
        [HttpPost]
        public async Task<IActionResult> TogglePublishAjax(int examId)
        {
            await _examRepo.TogglePublishStatusAsync(examId);
            return Json(new { success = true });
        }

        // 4. $.post Endpoint: Delete Duty
        [HttpPost]
        public async Task<IActionResult> DeleteDutyAjax(int dutyId)
        {
            var duty = await _dutyRepo.GetDutyByIdAsync(dutyId);
            if (duty != null)
            {
                await _swapRepo.DeleteSwapsByDutyIdAsync(dutyId);
                await _dutyRepo.DeleteDutyAsync(dutyId);
                await _examRepo.DeleteExamAsync(duty.ExamId);
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PublishAll()
        {
            await _examRepo.PublishAllAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}