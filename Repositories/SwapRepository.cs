using Dapper;
using ExamGuard.Models;
using ExamGuard.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace ExamGuard.Repositories
{
    public class SwapRepository : ISwapRepository
    {
        private readonly string _connectionString;

        public SwapRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private SqlConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<int> GetPendingSwapsCountAsync()
        {
            const string sql = "SELECT COUNT(*) FROM SwapRequests WHERE Status = 'Pending'";
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql);
        }

        public async Task<IEnumerable<SwapRequest>> GetAllPendingSwapsAsync()
        {
            const string sql = @"
                SELECT s.*, d.*, e.*, req.*
                FROM SwapRequests s
                INNER JOIN InvigilationDuties d ON s.OriginalDutyId = d.DutyId
                INNER JOIN Exams e ON d.ExamId = e.ExamId
                INNER JOIN AspNetUsers req ON s.RequesterUserId = req.Id
                WHERE s.Status = 'Pending'
                ORDER BY s.CreatedAt DESC";

            using var conn = CreateConnection();
            return await conn.QueryAsync<SwapRequest, InvigilationDuty, Exam, ApplicationUser, SwapRequest>(
                sql,
                (swap, duty, exam, requester) =>
                {
                    duty.Exam = exam;
                    swap.OriginalDuty = duty;
                    swap.RequesterUser = requester;
                    return swap;
                },
                splitOn: "DutyId,ExamId,Id"
            );
        }

        public async Task<IEnumerable<SwapRequest>> GetIncomingSwapsForUserAsync(string userId)
        {
            const string sql = @"
                SELECT s.*, d.*, e.*, req.*
                FROM SwapRequests s
                INNER JOIN InvigilationDuties d ON s.OriginalDutyId = d.DutyId
                INNER JOIN Exams e ON d.ExamId = e.ExamId
                INNER JOIN AspNetUsers req ON s.RequesterUserId = req.Id
                WHERE s.TargetUserId = @UserId AND s.Status = 'Pending'
                ORDER BY s.CreatedAt DESC";

            using var conn = CreateConnection();
            return await conn.QueryAsync<SwapRequest, InvigilationDuty, Exam, ApplicationUser, SwapRequest>(
                sql,
                (swap, duty, exam, requester) =>
                {
                    duty.Exam = exam;
                    swap.OriginalDuty = duty;
                    swap.RequesterUser = requester;
                    return swap;
                },
                new { UserId = userId },
                splitOn: "DutyId,ExamId,Id"
            );
        }

        public async Task<IEnumerable<SwapRequest>> GetOpenSwapBoardAsync(string excludeUserId)
        {
            const string sql = @"
                SELECT s.*, d.*, e.*, req.*
                FROM SwapRequests s
                INNER JOIN InvigilationDuties d ON s.OriginalDutyId = d.DutyId
                INNER JOIN Exams e ON d.ExamId = e.ExamId
                INNER JOIN AspNetUsers req ON s.RequesterUserId = req.Id
                WHERE s.TargetUserId IS NULL AND s.RequesterUserId != @ExcludeUserId AND s.Status = 'Pending'
                ORDER BY s.CreatedAt DESC";

            using var conn = CreateConnection();
            return await conn.QueryAsync<SwapRequest, InvigilationDuty, Exam, ApplicationUser, SwapRequest>(
                sql,
                (swap, duty, exam, requester) =>
                {
                    duty.Exam = exam;
                    swap.OriginalDuty = duty;
                    swap.RequesterUser = requester;
                    return swap;
                },
                new { ExcludeUserId = excludeUserId },
                splitOn: "DutyId,ExamId,Id"
            );
        }

        public async Task<IEnumerable<SwapRequest>> GetSwapHistoryForUserAsync(string userId)
        {
            const string sql = @"
                SELECT s.*, d.*, e.*, req.*
                FROM SwapRequests s
                INNER JOIN InvigilationDuties d ON s.OriginalDutyId = d.DutyId
                INNER JOIN Exams e ON d.ExamId = e.ExamId
                INNER JOIN AspNetUsers req ON s.RequesterUserId = req.Id
                WHERE (s.RequesterUserId = @UserId OR s.TargetUserId = @UserId) AND s.Status != 'Pending'
                ORDER BY s.CreatedAt DESC";

            using var conn = CreateConnection();
            return await conn.QueryAsync<SwapRequest, InvigilationDuty, Exam, ApplicationUser, SwapRequest>(
                sql,
                (swap, duty, exam, requester) =>
                {
                    duty.Exam = exam;
                    swap.OriginalDuty = duty;
                    swap.RequesterUser = requester;
                    return swap;
                },
                new { UserId = userId },
                splitOn: "DutyId,ExamId,Id"
            );
        }

        public async Task<SwapRequest?> GetSwapByIdAsync(int swapRequestId)
        {
            const string sql = "SELECT * FROM SwapRequests WHERE SwapRequestId = @Id";
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<SwapRequest>(sql, new { Id = swapRequestId });
        }

        public async Task CreateSwapRequestAsync(SwapRequest swap)
        {
            const string sql = @"
                INSERT INTO SwapRequests (OriginalDutyId, RequesterUserId, TargetUserId, TargetDutyId, Reason, Status, CreatedAt)
                VALUES (@OriginalDutyId, @RequesterUserId, @TargetUserId, @TargetDutyId, @Reason, @Status, @CreatedAt)";

            using var conn = CreateConnection();
            await conn.ExecuteAsync(sql, swap);
        }

        public async Task UpdateSwapStatusAsync(int swapRequestId, string status, string? targetUserId = null)
        {
            const string sql = @"
                UPDATE SwapRequests 
                SET Status = @Status, 
                    TargetUserId = COALESCE(@TargetUserId, TargetUserId) 
                WHERE SwapRequestId = @Id";

            using var conn = CreateConnection();
            await conn.ExecuteAsync(sql, new { Id = swapRequestId, Status = status, TargetUserId = targetUserId });
        }

        public async Task DeleteSwapsByDutyIdAsync(int dutyId)
        {
            const string sql = "DELETE FROM SwapRequests WHERE OriginalDutyId = @DutyId OR TargetDutyId = @DutyId";
            using var conn = CreateConnection();
            await conn.ExecuteAsync(sql, new { DutyId = dutyId });
        }
    }
}