using Jetwin.Database;
using Jetwin.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Jetwin.Models
{
    public class UserRepository : IUserRepository
    {
        private readonly DataAccessLayer _db;

        public UserRepository()
        {
            _db = new DataAccessLayer();
        }

        public DataTable GetUsers(string search = null)
        {
            string query = @"
                SELECT UserID, EmployeeName AS [Employee Name], 
                       Username, 
                       ContactNum AS [Contact Number], 
                       StatusName AS [Status]
                FROM Staff
                JOIN Status ON Staff.StatusID = Status.StatusID
                WHERE Username != 'admin'
                      AND (@Search IS NULL OR 
                           EmployeeName LIKE '%' + @Search + '%' OR 
                           Username LIKE '%' + @Search + '%' OR 
                           ContactNum LIKE '%' + @Search + '%')
                ORDER BY UserID DESC";

            return _db.ExecuteQuery(query, new Dictionary<string, object> { { "@Search", search ?? (object)DBNull.Value } });
        }

        public bool IsUsernameExists(string username)
        {
            string query = "SELECT COUNT(*) FROM Staff WHERE Username = @Username";
            return (int)_db.ExecuteScalar(query, new Dictionary<string, object> { { "@Username", username } }) > 0;
        }
        public bool IsDuplicateUser(int userId, string empName, string username, string contactNumber)
        {
            string query = @"
                SELECT COUNT(*)
                FROM Staff
                WHERE (EmployeeName = @EmpName OR Username = @Username OR ContactNum = @ContactNum)
                  AND UserID != @UserID";

            var parameters = new Dictionary<string, object>
            {
                { "@EmpName", empName },
                { "@Username", username },
                { "@ContactNum", contactNumber ?? (object)DBNull.Value },
                { "@UserID", userId }
            };

            return (int)_db.ExecuteScalar(query, parameters) > 0;
        }
        public bool IsDuplicateUser(string empName, string username, string contactNumber)
        {
            string query = @"
                SELECT COUNT(*)
                FROM Staff
                WHERE (EmployeeName = @EmpName OR Username = @Username OR ContactNum = @ContactNum)";

            var parameters = new Dictionary<string, object>
            {
                { "@EmpName", empName },
                { "@Username", username },
                { "@ContactNum", contactNumber ?? (object)DBNull.Value },
            };

            return (int)_db.ExecuteScalar(query, parameters) > 0;
        }
        public bool SaveUser(string empName, string username, string password, string contactNumber)
        {
            string query = @"
                INSERT INTO Staff (EmployeeName, Username, Password, ContactNum, RoleName, StatusID)
                VALUES (@EmpName, @Username, @Password, @ContactNum, 'User', 1)";
            return _db.ExecuteNonQuery(query, new Dictionary<string, object>
            {
                { "@EmpName", empName },
                { "@Username", username },
                { "@Password", password },
                { "@ContactNum", string.IsNullOrWhiteSpace(contactNumber) ? (object)DBNull.Value : (object)contactNumber }
            });
        }

        public bool ArchiveUser(int userId)
        {
            string query = "UPDATE Staff SET StatusID = 3 WHERE UserID = @UserID";
            return _db.ExecuteNonQuery(query, new Dictionary<string, object> { { "@UserID", userId } });
        }
        public bool UnarchiveUser(int userId)
        {
            string query = "UPDATE Staff SET StatusID = 1 WHERE UserID = @UserID";
            return _db.ExecuteNonQuery(query, new Dictionary<string, object> { { "@UserID", userId } });
        }
        public bool UpdateUser(int userId, string empName, string username, string contactNumber)
        {
            string query = @"
                UPDATE Staff
                SET EmployeeName = @EmpName,
                    Username = @Username,
                    ContactNum = @ContactNum
                WHERE UserID = @UserID";

            return _db.ExecuteNonQuery(query, new Dictionary<string, object>
            {
                { "@UserID", userId },
                { "@EmpName", empName },
                { "@Username", username },
                { "@ContactNum", string.IsNullOrWhiteSpace(contactNumber) ? (object)DBNull.Value : contactNumber }
            });
        }
    }
}
