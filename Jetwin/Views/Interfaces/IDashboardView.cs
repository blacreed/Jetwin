using System.Data;

namespace Jetwin.Views
{
    public interface IDashboardView
    {
        string DateFilter { get; }
        void SetTotalSales(decimal totalSales);
        void SetTotalOrders(int totalOrders);
        void SetTotalProducts(int totalProducts);
        void SetGrossRevenueChart(DataTable data);
        void SetTopProductsChart(DataTable data);
        void PopulateLowStockDataGrid(DataTable data);
        void PopulateLatestTransactionDataGrid(DataTable data);
    }
}
