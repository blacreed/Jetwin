using System.Data;

namespace Jetwin.Models.Interfaces
{
    public interface IUserRepository
    {
        DataTable GetUsers(string search = null);
        bool IsUsernameExists(string username);
        bool IsDuplicateUser(int userId, string empName, string username, string contactNumber);
        bool IsDuplicateUser(string empName, string username, string contactNumber);
        bool SaveUser(string empName, string username, string password, string contactNumber);
        bool ArchiveUser(int userId);
    }
}
