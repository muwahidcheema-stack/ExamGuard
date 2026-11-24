using ExamGuard.Models;

namespace ExamGuard.Repositories.Interfaces
{
    public interface IExamRepository
    {
        Task<IEnumerable<Exam>> GetPublishedExamsAsync(int limit = 5);
        Task<int> GetTotalExamsCountAsync();
        Task<int> CreateExamAsync(Exam exam);
        Task TogglePublishStatusAsync(int examId);
        Task PublishAllAsync();
        Task DeleteExamAsync(int examId);
    }
}