using Jetwin.Database;
using System;
using System.Collections.Generic;
using System.Security;

namespace Jetwin.Models
{
    internal class Login : DataAccessLayer
    {
        //Fields & Properties
        public SecureString Password { get; set; } //for future password hashing
        private readonly DataAccessLayer _dataAccessLayer = new DataAccessLayer();

        //METHODS
        public bool ValidateCredentials(string username, string password)
        {
            string credentialsQuery = "SELECT COUNT(1) FROM Staff WHERE Username = @Username AND Password = @Password";

            var parameters = new Dictionary<string, object>
            {
                { "@Username", username },
                { "@Password", password }
            };

            return Convert.ToInt32(ExecuteScalar(credentialsQuery, parameters)) > 0;
        }
        public (int StaffID, string Role) GetUserDetails(string username)
        {
            var query = "SELECT UserID, RoleName FROM Staff WHERE Username = @Username AND StatusID = 1";
            var data = _dataAccessLayer.ExecuteQuery(query, new Dictionary<string, object>
             {
                 { "@Username", username }
             });

            if (data.Rows.Count == 0) return (0, null);

            var row = data.Rows[0];
            return (Convert.ToInt32(row["UserID"]), row["RoleName"].ToString());
        }
    }
}
