using Jetwin.Models.Interfaces;
using Jetwin.Presenter;
using Jetwin.Views;
using System.Data;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Jetwin.Modules
{
    public partial class DashboardView : Form, IDashboardView
    {
        private readonly DashboardPresenter presenter;
        public DashboardView(IDashboardRepository repository)
        {
            InitializeComponent();
            presenter = new DashboardPresenter(this, repository);

            //SET DEFAULT VALUE FOR CB DATE FILTER
            cbDate.Items.AddRange(new[] { "Today", "Last 7 Days", "Last 30 Days", "This Month" });
            cbDate.SelectedItem = "Today"; //set default combobox value

            // handle date filter change
            cbDate.SelectedIndexChanged += (s, e) => presenter.UpdateLatestTransactionGrid();

            Load += (s, e) => presenter.LoadDashboardData();
        }

        public string DateFilter => cbDate.SelectedItem?.ToString();
        public void SetTotalSales(decimal totalSales) => lblTotalSales.Text = totalSales.ToString("N2");
        public void SetTotalOrders(int totalOrders) => lblTotalOrders.Text = totalOrders.ToString();
        public void SetTotalProducts(int totalProducts) => lblTotalProducts.Text = totalProducts.ToString();
        public void PopulateLowStockDataGrid(DataTable data) => lowStockDataGrid.DataSource = data;
        public void PopulateLatestTransactionDataGrid(DataTable data) => latestTransactionDataGrid.DataSource = data;
        public void SetGrossRevenueChart(DataTable data) => PopulateChart(chartGrossRevenue, data, "SalesDate", "Revenue");
        public void SetTopProductsChart(DataTable data) => PopulateChart(chartTopProducts, data, "ProductName", "Quantity");

        private static void PopulateChart(Chart chart, DataTable data, string xColumn, string yColumn)
        {
            chart.Series[0].Points.Clear();
            foreach (DataRow row in data.Rows)
                chart.Series[0].Points.AddXY(row[xColumn], row[yColumn]);
        }
        
    }
}
