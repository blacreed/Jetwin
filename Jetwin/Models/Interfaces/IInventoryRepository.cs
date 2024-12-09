using System.Data;

namespace Jetwin.Models.Interfaces
{
    public interface IInventoryRepository
    {
        DataTable SearchInventory(string searchInput);
        bool SaveInventory(int productCode, int? attributeCombinationID, int quantity, int? minStock, int? maxStock, int? reorderPoint, string location);
        bool UpdateInventory(int inventoryId, int productCode, int? attributeCombinationID, int quantity, int? minStock, int? maxStock, int? reorderPoint, string location);
        bool ProductRequiresAttributes(int productCode);
        DataTable GetPricelist();
        DataTable GetStockLevel();
        DataTable GetAdjustments();
        DataTable GetRestocking();
        DataTable GetSuppliers();
    }
}
