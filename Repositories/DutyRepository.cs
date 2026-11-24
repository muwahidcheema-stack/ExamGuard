using Dapper;
using ExamGuard.Models;
using ExamGuard.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace ExamGuard.Repositories
{
    public class DutyRepository : IDutyRepository
    {
        private readonly string _connectionString;

        public DutyRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private SqlConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<int> GetTotalDutiesCountAsync()
        {
            const string sql = "SELECT COUNT(*) FROM InvigilationDuties";
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql);
        }

        public async Task<IEnumerable<InvigilationDuty>> GetAllDutiesWithDetailsAsync(string? searchTerm = null)
        {
            var sql = @"
                SELECT d.*, e.*, u.*
                FROM InvigilationDuties d
                INNER JOIN Exams e ON d.ExamId = e.ExamId
                INNER JOIN AspNetUsers u ON d.AssignedUserId = u.Id
                WHERE (@Search IS NULL 
                       OR e.CourseCode LIKE '%' + @Search + '%' 
                       OR e.CourseName LIKE '%' + @Search + '%'
                       OR e.Venue LIKE '%' + @Search + '%'
                       OR u.FullName LIKE '%' + @Search + '%')
                ORDER BY e.ExamDate ASC";

            using var conn = CreateConnection();
            return await conn.QueryAsync<InvigilationDuty, Exam, ApplicationUser, InvigilationDuty>(
                sql,
                (duty, exam, user) =>
                {
                    duty.Exam = exam;
                    duty.AssignedUser = user;
                    return duty;
                },
                new { Search = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm.Trim() },
                splitOn: "ExamId,Id"
            );
        }

        public async Task<IEnumerable<InvigilationDuty>> GetDutiesByUserIdAsync(string userId)
        {
            const string sql = @"
                SELECT d.*, e.*
                FROM InvigilationDuties d
                INNER JOIN Exams e ON d.ExamId = e.ExamId
                WHERE d.AssignedUserId = @UserId
                ORDER BY e.ExamDate ASC";

            using var conn = CreateConnection();
            return await conn.QueryAsync<InvigilationDuty, Exam, InvigilationDuty>(
                sql,
                (duty, exam) =>
                {
                    duty.Exam = exam;
                    return duty;
                },
                new { UserId = userId },
                splitOn: "ExamId"
            );
        }

        public async Task<InvigilationDuty?> GetDutyByIdAsync(int dutyId)
        {
            const string sql = "SELECT * FROM InvigilationDuties WHERE DutyId = @DutyId";
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<InvigilationDuty>(sql, new { DutyId = dutyId });
        }

        public async Task CreateDutyAsync(InvigilationDuty duty)
        {
            const string sql = @"
                INSERT INTO InvigilationDuties (ExamId, AssignedUserId, Status)
                VALUES (@ExamId, @AssignedUserId, @Status)";

            using var conn = CreateConnection();
            await conn.ExecuteAsync(sql, duty);
        }

        public async Task UpdateDutyStatusAsync(int dutyId, string status)
        {
            const string sql = "UPDATE InvigilationDuties SET Status = @Status WHERE DutyId = @DutyId";
            using var conn = CreateConnection();
            await conn.ExecuteAsync(sql, new { DutyId = dutyId, Status = status });
        }

        public async Task ReassignDutyAsync(int dutyId, string newUserId, string newStatus)
        {
            const string sql = @"
                UPDATE InvigilationDuties 
                SET AssignedUserId = @NewUserId, Status = @NewStatus 
                WHERE DutyId = @DutyId";

            using var conn = CreateConnection();
            await conn.ExecuteAsync(sql, new { DutyId = dutyId, NewUserId = newUserId, NewStatus = newStatus });
        }

        public async Task DeleteDutyAsync(int dutyId)
        {
            const string sql = "DELETE FROM InvigilationDuties WHERE DutyId = @DutyId";
            using var conn = CreateConnection();
            await conn.ExecuteAsync(sql, new { DutyId = dutyId });
        }
    }
}