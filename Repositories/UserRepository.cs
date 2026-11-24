using Dapper;
using ExamGuard.Models;
using ExamGuard.Repositories.Interfaces;
using Microsoft.Data.SqlClient;

namespace ExamGuard.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private SqlConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IEnumerable<ApplicationUser>> GetAllFacultyAndTAsAsync()
        {
            const string sql = @"
                SELECT u.* 
                FROM AspNetUsers u
                INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
                INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
                WHERE r.Name = 'Faculty'
                ORDER BY u.FullName ASC";

            using var conn = CreateConnection();
            return await conn.QueryAsync<ApplicationUser>(sql);
        }

        public async Task<IEnumerable<ApplicationUser>> GetColleaguesAsync(string currentUserId)
        {
            const string sql = @"
                SELECT u.* 
                FROM AspNetUsers u
                INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
                INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
                WHERE r.Name = 'Faculty' AND u.Id != @CurrentUserId
                ORDER BY u.FullName ASC";

            using var conn = CreateConnection();
            return await conn.QueryAsync<ApplicationUser>(sql, new { CurrentUserId = currentUserId });
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            const string sql = "SELECT * FROM AspNetUsers WHERE Id = @UserId";
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<ApplicationUser>(sql, new { UserId = userId });
        }
    }
}