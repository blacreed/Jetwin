using Jetwin.Database;
using Jetwin.Models.Interfaces;
using System.Collections.Generic;
using System.Data;

namespace Jetwin.Models
{
    public class SalesRepository : ISalesRepository
    {
        private readonly DataAccessLayer _db;

        public SalesRepository()
        {
            _db = new DataAccessLayer();
        }

        public DataTable GetAllTransactions()
        {
            string query = @"
                SELECT 
                    s.SalesID AS [Sales ID],
                    s.SalesDate AS [Sales Date],
                    st.EmployeeName AS [Staff Name],
                    ss.SaleStatusName AS [Status],
                    SUM(sd.TotalAmount) AS [Total],
                    p.PaymentType AS [Payment Type],
                    MAX(p.AmountPaid) AS [Amount Paid],
                    p.PaymentReference AS [Reference Number]
                FROM Sales s
                JOIN Staff st ON s.StaffID = st.UserID
                JOIN SaleStatus ss ON s.SaleStatusID = ss.SaleStatusID
                JOIN SalesDetails sd ON s.SalesID = sd.SalesID
                LEFT JOIN Payment p ON s.SalesID = p.SalesID
                GROUP BY s.SalesID, s.SalesDate, st.EmployeeName, ss.SaleStatusName, p.PaymentType, p.PaymentReference";
            return _db.ExecuteQuery(query, null);
        }

        public DataTable GetTransactionsByStatus(string status)
        {
            string query = @"
                SELECT 
                    s.SalesID AS [Sales ID],
                    s.SalesDate AS [Sales Date],
                    st.EmployeeName AS [Staff Name],
                    ss.SaleStatusName AS [Status],
                    SUM(sd.TotalAmount) AS [Total],
                    p.PaymentType AS [Payment Type],
                    MAX(p.AmountPaid) AS [Amount Paid],
                    p.PaymentReference AS [Reference Number]
                FROM Sales s
                JOIN Staff st ON s.StaffID = st.UserID
                JOIN SaleStatus ss ON s.SaleStatusID = ss.SaleStatusID
                JOIN SalesDetails sd ON s.SalesID = sd.SalesID
                LEFT JOIN Payment p ON s.SalesID = p.SalesID
                WHERE ss.SaleStatusName = @Status
                GROUP BY s.SalesID, s.SalesDate, st.EmployeeName, ss.SaleStatusName, p.PaymentType, p.PaymentReference";

            return _db.ExecuteQuery(query, new Dictionary<string, object> { { "@Status", status } });
        }

        public DataTable GetTransactionDetails(int transactionID)
        {
            string query = @"
                SELECT 
                    p.ProductName AS [Product Name],
                    b.BrandName AS [Brand],
                    c.CategoryName AS [Category],
                    u.UoMName AS [Unit of Measurement],
                    ISNULL(
                        STRING_AGG(
                            CASE 
                                WHEN at.AttributeTypeName IS NOT NULL AND av.AttributeValueName IS NOT NULL THEN CONCAT(at.AttributeTypeName, ': ', av.AttributeValueName)
                                ELSE NULL
                            END, 
                            ', '
                        ), 'N/A'
                    ) AS [Variant],
                    sd.Quantity,
                    sd.UnitPrice AS [Unit Price],
                    sd.Discount,
                    sd.TotalAmount AS [Total Amount]
                FROM SalesDetails sd
                JOIN Product p ON sd.ProductCode = p.ProductCode
                JOIN Brand b ON p.BrandID = b.BrandID
                JOIN Category c ON p.CategoryID = c.CategoryID
                JOIN UnitOfMeasurement u ON p.UoMID = u.UoMID
                LEFT JOIN ProductAttributes pa ON pa.ProductCode = p.ProductCode
                LEFT JOIN AttributeType at ON pa.AttributeTypeID = at.AttributeTypeID
                LEFT JOIN AttributeValue av ON pa.AttributeValueID = av.AttributeValueID
                WHERE sd.SalesID = @TransactionID
                GROUP BY 
                    p.ProductName, b.BrandName, c.CategoryName, 
                    u.UoMName, sd.Quantity, sd.UnitPrice, sd.Discount, sd.TotalAmount;";
            return _db.ExecuteQuery(query, new Dictionary<string, object> { { "@TransactionID", transactionID } });
        }
    }
}

