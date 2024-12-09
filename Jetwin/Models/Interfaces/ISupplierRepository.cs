using System.Data;

namespace Jetwin.Models.Interfaces
{
    public interface ISupplierRepository
    {
        DataTable SearchSuppliers(string searchInput);
        bool SaveSupplier(string supplierName, string agentName, string contactNumber, string remarks);
        bool UpdateSupplier(int supplierId, string supplierName, string agentName, string contactNumber, string remarks);
        bool ArchiveSupplier(int supplierId);
        bool UnarchiveSupplier(int supplierId);
        DataTable GetActiveSuppliers();
    }
}
