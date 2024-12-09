using Jetwin.Models.Interfaces;
using Jetwin.Views;
using System.Data;

namespace Jetwin.Presenter
{
    public class DashboardPresenter //DASHBOARD MODULE
    {
        private readonly IDashboardView _view;
        private readonly IDashboardRepository _repository;
        public DashboardPresenter(IDashboardView view, IDashboardRepository repository)
        {
            _view = view;
            _repository = repository;

            //event subscription, if a new product, inventory or transaction is added, then update dashboard
            NewTransactionPresenter.TransactionCompletedStatic += LoadDashboardData;
            ProductPresenter.ProductUpdated += LoadDashboardData;
            InventoryPresenter.InventoryUpdated += LoadDashboardData;
        }

        //load all data
        public void LoadDashboardData()
        {
            _view.SetTotalSales(_repository.GetTotalSales());
            _view.SetTotalOrders(_repository.GetTotalOrders());
            _view.SetTotalProducts(_repository.GetTotalProducts());
            _view.SetGrossRevenueChart(_repository.GetGrossRevenueData());
            _view.SetTopProductsChart(_repository.GetTopProductsData());
            _view.PopulateLowStockDataGrid(_repository.GetLowStockProducts());
            UpdateLatestTransactionGrid();
        }

        public void UpdateLatestTransactionGrid()
        {
            var data = _repository.GetLatestTransactions(_view.DateFilter);
            if (data == null || data.Rows.Count == 0)
            {
                _view.PopulateLatestTransactionDataGrid(new DataTable()); //clear grid if no data
                return;
            }
            _view.PopulateLatestTransactionDataGrid(data);
        }
    }
}
