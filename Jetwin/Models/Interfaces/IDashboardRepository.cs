using System;
using System.Data;

namespace Jetwin.Models.Interfaces
{
    public interface IDashboardRepository
    {
        decimal GetTotalSales();
        int GetTotalOrders();
        int GetTotalProducts();
        DataTable GetGrossRevenueData();
        DataTable GetTopProductsData();
        DataTable GetLowStockProducts();
        DataTable GetLatestTransactions(string dateFilter);
    }
}
