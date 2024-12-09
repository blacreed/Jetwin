using System.Data;

namespace Jetwin.Models.Interfaces
{
    public interface ISalesRepository
    {
        DataTable GetAllTransactions();
        DataTable GetTransactionsByStatus(string status);
        DataTable GetTransactionDetails(int transactionID);
    }
}
