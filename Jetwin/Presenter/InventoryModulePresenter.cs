using Jetwin.Models;
using Jetwin.Modules;

namespace Jetwin.Presenter
{
    public class InventoryModulePresenter //INVENTORY MODULE
    {
        private readonly InventoryRepository _repository;
        private readonly InventoryView _inventoryView;
        public InventoryModulePresenter(InventoryView view)
        {
            _repository = new InventoryRepository();
            _inventoryView = view;
        }
        //#regionstart load data into data grid
        public void LoadPricelist()
        {
            var data = _repository.GetPricelist();
            _inventoryView.DisplayPricelist(data);
        }

        public void LoadStockLevel()
        {
            var data = _repository.GetStockLevel();
            _inventoryView.DisplayStockLevel(data);
        }

        public void LoadAdjustments()
        {
            var data = _repository.GetAdjustments();
            _inventoryView.DisplayAdjustments(data);
        }

        public void LoadRestocking()
        {
            var data = _repository.GetRestocking();
            _inventoryView.DisplayRestocking(data);
        }

        public void LoadSuppliers()
        {
            var data = _repository.GetSuppliers();
            _inventoryView.DisplaySuppliers(data);
        }
        //#regionend
    }
}
