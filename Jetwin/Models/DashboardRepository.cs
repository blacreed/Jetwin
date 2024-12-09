using Jetwin.Database;
using Jetwin.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Jetwin.Models
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly DataAccessLayer _dataAccessLayer;
        public DashboardRepository()
        {
            _dataAccessLayer = new DataAccessLayer();
        }
        //get total sales amount
        public decimal GetTotalSales()
        {
            string query = "SELECT ISNULL(SUM(TotalAmount), 0) FROM SalesDetails";
            return Convert.ToDecimal(_dataAccessLayer.ExecuteScalar(query, null) ?? 0);
        }

        //get total number of orders
        public int GetTotalOrders()
        {
            string query = "SELECT ISNULL(COUNT(SalesID), 0) FROM Sales";
            return Convert.ToInt32(_dataAccessLayer.ExecuteScalar(query, null) ?? 0);
        }

        //get total number of products
        public int GetTotalProducts()
        {
            string query = "SELECT ISNULL(COUNT(ProductCode), 0) FROM Product WHERE StatusID = 1"; //only active products
            return Convert.ToInt32(_dataAccessLayer.ExecuteScalar(query, null) ?? 0);
        }

        //gets data for gross revenue chart
        public DataTable GetGrossRevenueData()
        {
            string query = @"
                SELECT CONVERT(DATE, SalesDate) AS SalesDate, SUM(sd.TotalAmount) AS Revenue
                FROM Sales s
                INNER JOIN SalesDetails sd ON s.SalesID = sd.SalesID
                GROUP BY CONVERT(DATE, SalesDate)";
            return _dataAccessLayer.ExecuteQuery(query, null);
        }

        //get data for top products chart
        public DataTable GetTopProductsData()
        {
            string query = @"
                SELECT TOP 5 Product.ProductName, SUM(sd.Quantity) AS Quantity
                FROM SalesDetails sd
                INNER JOIN Product ON sd.ProductCode = Product.ProductCode
                GROUP BY Product.ProductName
                ORDER BY Quantity DESC";
            return _dataAccessLayer.ExecuteQuery(query, null);
        }

        //get low stock products
        public DataTable GetLowStockProducts()
        {
            string query = @"
                SELECT DISTINCT Product.ProductName, Inventory.Quantity
                FROM Inventory
                INNER JOIN Product ON Inventory.ProductCode = Product.ProductCode
                WHERE Inventory.Quantity < Inventory.ReorderPoint
                OR Inventory.Quantity < Inventory.MinimumStockLevel
                OR Inventory.Quantity = 0
                ORDER BY Inventory.Quantity ASC";

            return _dataAccessLayer.ExecuteQuery(query, null);
        }

        //gets latest transactions
        public DataTable GetLatestTransactions(string dateFilter)
        {
            string query = @"
                SELECT SalesID, SalesDate, 
                       (SELECT SaleStatusName FROM SaleStatus WHERE SaleStatus.SaleStatusID = Sales.SaleStatusID) AS Status, 
                       (SELECT SUM(TotalAmount) FROM SalesDetails WHERE SalesDetails.SalesID = Sales.SalesID) AS Amount
                FROM Sales";

            var parameters = new Dictionary<string, object>();

            switch (dateFilter)
            {
                case "Today":
                    query += " WHERE CAST(SalesDate AS DATE) = CAST(GETDATE() AS DATE)";
                    break;
                case "Last 7 Days":
                    query += " WHERE SalesDate >= DATEADD(DAY, -7, GETDATE())";
                    break;
                case "Last 30 Days":
                    query += " WHERE SalesDate >= DATEADD(DAY, -30, GETDATE())";
                    break;
                case "This Month":
                    query += " WHERE MONTH(SalesDate) = MONTH(GETDATE()) AND YEAR(SalesDate) = YEAR(GETDATE())";
                    break;
                default:
                    query += " WHERE 1=1"; //no filtering if invalid dateFilter
                    break;
            }

            return _dataAccessLayer.ExecuteQuery(query, parameters);
        }

    }
}
