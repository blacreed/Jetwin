using System.Data;

namespace Jetwin.Models.Interfaces
{
    public interface ITransactionRepository
    {
        DataTable GetDistinctCategories();
        DataTable GetDistinctBrands();
        int GetLoggedInStaffID(string username);
        DataTable GetProducts(string category, string brand, string search);
        void ExecuteTransac(DataTable cartItems, string paymentType, decimal amountPaid, int staffID, string referenceNumber = null);
        (int ProductCode, string ProductName, string Brand, string Category, string Variant, int AttributeCombinationID, decimal UnitPrice) GetProductDetails(int productCode);
        DataTable GetStock(int productCode);
        bool IsReferenceNumberUnique(string referenceNumber);
    }
}
