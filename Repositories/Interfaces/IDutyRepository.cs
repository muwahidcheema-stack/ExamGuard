using ExamGuard.Models;

namespace ExamGuard.Repositories.Interfaces
{
    public interface IDutyRepository
    {
        Task<int> GetTotalDutiesCountAsync();
        Task<IEnumerable<InvigilationDuty>> GetAllDutiesWithDetailsAsync(string? searchTerm = null);
        Task<IEnumerable<InvigilationDuty>> GetDutiesByUserIdAsync(string userId);
        Task<InvigilationDuty?> GetDutyByIdAsync(int dutyId);
        Task CreateDutyAsync(InvigilationDuty duty);
        Task UpdateDutyStatusAsync(int dutyId, string status);
        Task ReassignDutyAsync(int dutyId, string newUserId, string newStatus);
        Task DeleteDutyAsync(int dutyId);
    }
}