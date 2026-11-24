using ExamGuard.Models;

namespace ExamGuard.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAllFacultyAndTAsAsync();
        Task<IEnumerable<ApplicationUser>> GetColleaguesAsync(string currentUserId);
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
    }
}