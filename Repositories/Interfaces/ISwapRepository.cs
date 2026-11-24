using ExamGuard.Models;

namespace ExamGuard.Repositories.Interfaces
{
    public interface ISwapRepository
    {
        Task<int> GetPendingSwapsCountAsync();
        Task<IEnumerable<SwapRequest>> GetAllPendingSwapsAsync();
        Task<IEnumerable<SwapRequest>> GetIncomingSwapsForUserAsync(string userId);
        Task<IEnumerable<SwapRequest>> GetOpenSwapBoardAsync(string excludeUserId);
        Task<IEnumerable<SwapRequest>> GetSwapHistoryForUserAsync(string userId);
        Task<SwapRequest?> GetSwapByIdAsync(int swapRequestId);
        Task CreateSwapRequestAsync(SwapRequest swap);
        Task UpdateSwapStatusAsync(int swapRequestId, string status, string? targetUserId = null);
        Task DeleteSwapsByDutyIdAsync(int dutyId);
    }
}
