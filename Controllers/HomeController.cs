using ExamGuard.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ExamGuard.Controllers
{
    public class HomeController : Controller
    {
        private readonly IExamRepository _examRepo;

        public HomeController(IExamRepository examRepo)
        {
            _examRepo = examRepo;
        }

        public async Task<IActionResult> Index()
        {
            var publishedExams = await _examRepo.GetPublishedExamsAsync(5);
            return View(publishedExams);
        }
    }
}