using Dapper;
using ExamGuard.Models;
using ExamGuard.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace ExamGuard.Repositories
{
    public class ExamRepository : IExamRepository
    {
        private readonly string _connectionString;

        public ExamRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private SqlConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<Exam>> GetPublishedExamsAsync(int limit = 5)
        {
            const string sql = @"
                SELECT TOP (@Limit) * 
                FROM Exams 
                WHERE IsPublished = 1 
                ORDER BY ExamDate ASC";

            using var conn = CreateConnection();
            return await conn.QueryAsync<Exam>(sql, new { Limit = limit });
        }

        public async Task<int> GetTotalExamsCountAsync()
        {
            const string sql = "SELECT COUNT(*) FROM Exams";
            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql);
        }

        public async Task<int> CreateExamAsync(Exam exam)
        {
            const string sql = @"
                INSERT INTO Exams (CourseName, CourseCode, ExamDate, StartTime, EndTime, Venue, IsPublished)
                VALUES (@CourseName, @CourseCode, @ExamDate, @StartTime, @EndTime, @Venue, @IsPublished);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            using var conn = CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql, exam);
        }

        public async Task TogglePublishStatusAsync(int examId)
        {
            const string sql = @"
                UPDATE Exams 
                SET IsPublished = CASE WHEN IsPublished = 1 THEN 0 ELSE 1 END 
                WHERE ExamId = @ExamId";

            using var conn = CreateConnection();
            await conn.ExecuteAsync(sql, new { ExamId = examId });
        }

        public async Task PublishAllAsync()
        {
            const string sql = "UPDATE Exams SET IsPublished = 1 WHERE IsPublished = 0";
            using var conn = CreateConnection();
            await conn.ExecuteAsync(sql);
        }

        public async Task DeleteExamAsync(int examId)
        {
            const string sql = "DELETE FROM Exams WHERE ExamId = @ExamId";
            using var conn = CreateConnection();
            await conn.ExecuteAsync(sql, new { ExamId = examId });
        }
    }
}