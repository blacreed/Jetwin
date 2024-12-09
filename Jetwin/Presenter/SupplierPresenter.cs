using Jetwin.Models;
using Jetwin.Modules;
using System.Data;
using System.Text.RegularExpressions;

namespace Jetwin.Presenter
{
    public class SupplierPresenter //MAINTENANCE MODULE -> SUPPLIER SUBMODULE
    {
        private readonly MaintenanceView _view;
        private readonly SupplierRepository _repository;

        public SupplierPresenter(MaintenanceView view)
        {
            _view = view;
            _repository = new SupplierRepository();
        }
        //get supplier data and display into data grid
        public void LoadSuppliers(string searchInput = null)
        {
            var suppliers = _repository.SearchSuppliers(searchInput);
            _view.DisplaySuppliers(suppliers);
        }
        //get supplier data but only with active status
        public DataTable LoadActiveSuppliers()
        {
            return _repository.GetActiveSuppliers();
        }

        public void SaveSupplier()
        {
            var supplierName = _view.SupplierName;
            var agentName = _view.AgentName;
            var contactNumber = _view.SupplierContact;
            var remarks = _view.Remarks;

            if (string.IsNullOrWhiteSpace(supplierName) ||
                string.IsNullOrWhiteSpace(agentName) ||
                string.IsNullOrWhiteSpace(contactNumber) ||
                !Regex.IsMatch(contactNumber, @"^\d{9,15}$"))
            {
                _view.ShowValidationMessage("Invalid input. Ensure all required fields are filled and valid.");
                return;
            }
            //if editing a supplier
            if (_view.editingId.HasValue)
            {
                //update supplier
                if (_repository.UpdateSupplier(_view.editingId.Value, supplierName, agentName, contactNumber, remarks))
                {
                    _view.ShowValidationMessage("Supplier updated successfully.");
                    LoadSuppliers();
                    _view.ClearSupplierInputs();
                    _view.ResetEditState();
                }
                else
                {
                    _view.ShowValidationMessage("Failed to update supplier.");
                }
            }
            //if not editing/just adding supplier
            else
            {
                // Add Supplier
                if (_repository.SaveSupplier(supplierName, agentName, contactNumber, remarks))
                {
                    _view.ShowValidationMessage("Supplier added successfully.");
                    LoadSuppliers();
                    _view.ClearSupplierInputs();
                }
                else
                {
                    _view.ShowValidationMessage("Failed to add supplier.");
                }
            }
        }

        public void ArchiveSupplier(int supplierId, string currentStatus)
        {
            bool success = currentStatus == "Archived"
                ? _repository.UnarchiveSupplier(supplierId)
                : _repository.ArchiveSupplier(supplierId);

            if (success)
            {
                _view.ShowValidationMessage($"Supplier {(currentStatus == "Archived" ? "unarchived" : "archived")} successfully.");
                LoadSuppliers();
            }
            else
            {
                _view.ShowValidationMessage("Failed to change supplier status.");
            }
        }
    }
}
