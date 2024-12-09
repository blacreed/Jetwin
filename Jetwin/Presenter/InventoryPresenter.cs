using Jetwin.Models;
using Jetwin.Modules;
using System;
using System.Windows.Forms;

namespace Jetwin.Presenter
{
    public class InventoryPresenter //MAINTENANCE MODULE -> INVENTORY SUBMODULE
    {
        private readonly MaintenanceView _maintenanceView;
        private readonly InventoryRepository _repository;
        public static event Action InventoryUpdated;
        public InventoryPresenter(MaintenanceView view)
        {
            _maintenanceView = view;
            _repository = new InventoryRepository();
        }

        //load data into data grid
        public void LoadInventory(string searchInput = null)
        {
            var inventory = _repository.SearchInventory(searchInput);
            _maintenanceView.DisplayInventory(inventory);
        }

        public void SaveInventory()
        {
            //validation check
            if (string.IsNullOrWhiteSpace(_maintenanceView.SelectedProduct))
            {
                _maintenanceView.ShowValidationMessage("Please select a product to configure its inventory.");
                return;
            }
            
            if (!int.TryParse(_maintenanceView.SelectedProduct, out int productCode))
            {
                _maintenanceView.ShowValidationMessage("Invalid product selection.");
                return;
            }

            //retrieve or validate AttributeCombinationID
            int? attributeCombinationID = _repository.GetAttributeCombinationID(productCode);
            if (attributeCombinationID == null && _repository.ProductRequiresAttributes(productCode))
            {
                _maintenanceView.ShowValidationMessage("The selected product requires attribute combinations. Please configure attributes first.");
                return;
            }

            int? minStock = int.TryParse(_maintenanceView.MinimumStock, out int min) ? (int?)min : null;
            int? maxStock = int.TryParse(_maintenanceView.MaximumStock, out int max) ? (int?)max : null;
            int? reorderPoint = int.TryParse(_maintenanceView.ReorderPoint, out int reorder) ? (int?)reorder : null;

            string location = string.IsNullOrWhiteSpace(_maintenanceView.Location) ? null : _maintenanceView.Location;

            if (!int.TryParse(_maintenanceView.Quantity, out int quantity) || quantity < 0)
            {
                _maintenanceView.ShowValidationMessage("Quantity must be a valid non-negative number.");
                return;
            }
            //if editing (discarded function, no edit functionality for inventory)
            if (_maintenanceView.editingId.HasValue)
            {
                if (_repository.UpdateInventory(_maintenanceView.editingId.Value, productCode, attributeCombinationID, quantity, minStock, maxStock, reorderPoint, location))
                {
                    _maintenanceView.ShowValidationMessage("Inventory updated successfully.");

                    LoadInventory();
                    InventoryUpdated?.Invoke();
                    _maintenanceView.ClearInventoryInputs();
                    _maintenanceView.ResetEditState();
                }
                else
                {
                    _maintenanceView.ShowValidationMessage("Failed to update inventory.");
                }
            }
            //if configuring inventory, not editing
            else
            {
                if (_repository.SaveInventory(productCode, attributeCombinationID, quantity, minStock, maxStock, reorderPoint, location))
                {
                    _maintenanceView.ShowValidationMessage("Inventory added successfully.");

                    LoadInventory();
                    InventoryUpdated?.Invoke();
                    _maintenanceView.ClearInventoryInputs();
                }
                else
                {
                    _maintenanceView.ShowValidationMessage("Failed to add inventory.");
                }
            }
        }

        
    }
}
