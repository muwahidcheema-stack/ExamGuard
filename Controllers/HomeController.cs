using ExamGuard.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExamGuard.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var publishedExams = await _context.Exams
                .Where(e => e.IsPublished)
                .OrderBy(e => e.ExamDate)
                .Take(5)
                .ToListAsync();

            return View(publishedExams);
        }
    }
}