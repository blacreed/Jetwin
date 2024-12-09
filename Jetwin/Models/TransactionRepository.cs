using Jetwin.Database;
using Jetwin.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Jetwin.Models
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly DataAccessLayer _db;

        public TransactionRepository()
        {
            _db = new DataAccessLayer();
        }

        public DataTable GetProducts(string category, string brand, string search)
        {
            var query = @"
                SELECT 
                    p.ProductCode AS [Product Code],
                    p.ProductName AS [Product Name],
                    b.BrandName AS [Brand],
                    c.CategoryName AS [Category],
                    ISNULL(
                        STRING_AGG(
                            CASE 
                                WHEN at.AttributeTypeName IS NOT NULL AND av.AttributeValueName IS NOT NULL THEN CONCAT(at.AttributeTypeName, ': ', av.AttributeValueName)
                                ELSE NULL
                            END, 
                            ', '
                        ), 'N/A'
                    ) AS [Variant],
                    p.UnitPrice AS [Unit Price],
                    u.UoMName AS [Unit of Measurement],
                    i.Quantity AS [Stock]
                FROM Product p
                JOIN Brand b ON p.BrandID = b.BrandID
                JOIN Category c ON p.CategoryID = c.CategoryID
                JOIN Inventory i ON p.ProductCode = i.ProductCode
                JOIN UnitOfMeasurement u ON p.UoMID = u.UoMID
                LEFT JOIN ProductAttributes pa ON pa.ProductCode = p.ProductCode
                LEFT JOIN AttributeType at ON pa.AttributeTypeID = at.AttributeTypeID
                LEFT JOIN AttributeValue av ON pa.AttributeValueID = av.AttributeValueID
                WHERE i.Quantity > 0
                  AND p.StatusID = 1
                  AND (ISNULL(@Category, '') = '' OR c.CategoryName = @Category)
                  AND (ISNULL(@Brand, '') = '' OR b.BrandName = @Brand)
                  AND (ISNULL(@Search, '') = '' OR p.ProductName LIKE '%' + @Search + '%')
                GROUP BY 
                    p.ProductCode, p.ProductName, b.BrandName, c.CategoryName, 
                    p.UnitPrice, u.UoMName, i.Quantity;";

            return _db.ExecuteQuery(query, new Dictionary<string, object>
            {
                { "@Category", category ?? (object)DBNull.Value },
                { "@Brand", brand ?? (object)DBNull.Value },
                { "@Search", search ?? (object)DBNull.Value }
            });
        }
        public DataTable GetStock(int productCode)
        {
            string query = @"
                SELECT Quantity 
                FROM Inventory 
                WHERE ProductCode = @ProductCode";
            return _db.ExecuteQuery(query, new Dictionary<string, object>
            {
                { "@ProductCode", productCode }
            });
        }
        public (int ProductCode, string ProductName, string Brand, string Category, string Variant, int AttributeCombinationID, decimal UnitPrice) GetProductDetails(int productCode)
        {
            var query = @"
                SELECT 
                    p.ProductCode AS [Product Code], 
                    p.ProductName AS [Product Name], 
                    b.BrandName AS [Brand Name], 
                    c.CategoryName AS [Category Name], 
                    ISNULL(
                        STRING_AGG(
                            CASE 
                                WHEN at.AttributeTypeName IS NOT NULL AND av.AttributeValueName IS NOT NULL THEN CONCAT(at.AttributeTypeName, ': ', av.AttributeValueName)
                                ELSE NULL
                            END, 
                            ', '
                        ), 'N/A'
                    ) AS [Variant],
                    ISNULL(pa.ProductAttributeID, 0) AS AttributeCombinationID, 
                    p.UnitPrice AS [Unit Price]
                FROM Product p
                JOIN Brand b ON p.BrandID = b.BrandID
                JOIN Category c ON p.CategoryID = c.CategoryID
                LEFT JOIN ProductAttributes pa ON pa.ProductCode = p.ProductCode
                LEFT JOIN AttributeType at ON pa.AttributeTypeID = at.AttributeTypeID
                LEFT JOIN AttributeValue av ON pa.AttributeValueID = av.AttributeValueID
                WHERE p.ProductCode = @ProductCode
                GROUP BY 
                    p.ProductCode, 
                    p.ProductName, 
                    b.BrandName, 
                    c.CategoryName, 
                    pa.ProductAttributeID, 
                    p.UnitPrice";
            var data = _db.ExecuteQuery(query, new Dictionary<string, object> { { "@ProductCode", productCode } });

            //error handling
            if (data.Rows.Count == 0)
            {
                throw new KeyNotFoundException($"Product with ID {productCode} not found.");
            }

            var row = data.Rows[0];
            return (
                Convert.ToInt32(row["Product Code"]),
                row["Product Name"].ToString(),
                row["Brand Name"].ToString(),
                row["Category Name"].ToString(),
                row["Variant"].ToString(),
                Convert.ToInt32(row["AttributeCombinationID"]),
                Convert.ToDecimal(row["Unit Price"])
            );
        }
        public bool IsReferenceNumberUnique(string referenceNumber)
        {
            string query = "SELECT COUNT(*) FROM Payment WHERE PaymentReference = @PaymentReference";
            return (int)_db.ExecuteScalar(query, new Dictionary<string, object> { { "@PaymentReference", referenceNumber } }) > 0;
        }
        public DataTable GetDistinctCategories()
        {
            var query = "SELECT DISTINCT CategoryName FROM Category WHERE StatusID = 1";
            return _db.ExecuteQuery(query, null);
        }

        public DataTable GetDistinctBrands()
        {
            var query = "SELECT DISTINCT BrandName FROM Brand WHERE StatusID = 1";
            return _db.ExecuteQuery(query, null);
        }
        public int GetLoggedInStaffID(string username)
        {
            var query = "SELECT UserID FROM Staff WHERE Username = @Username AND StatusID = 1";
            var result = _db.ExecuteScalar(query, new Dictionary<string, object>
            {
                { "@Username", username }
            });
            return result != null ? Convert.ToInt32(result) : throw new KeyNotFoundException("User not found.");
        }
        public void ExecuteTransac(DataTable cartItems, string paymentType, decimal amountPaid, int staffID, string referenceNumber = null)
        {
            try
            {
                var salesId = 0;

                _db.ExecuteTransaction(transaction =>
                {
                    //step 1: insert into sales table(database) and get the sales ID
                    salesId = InsertSales(transaction, paymentType, amountPaid, staffID);

                    //step 2: insert each cart item into SalesDetails table(database) and update inventory
                    foreach (DataRow row in cartItems.Rows)
                    {
                        Console.WriteLine($"ProductCode: {row.Field<int>("ProductCode")}, " +
                            $"AttributeCombinationID: {row["AttributeCombinationID"]}, " +
                            $"Quantity: {row.Field<int>("Quantity")}");
                        InsertSalesDetail(transaction, salesId, row);
                        UpdateInventory(transaction, row);
                    }

                    //step 3: insert payment in payment table(database)
                    InsertPayment(transaction, salesId, paymentType, amountPaid, referenceNumber);
                });
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private int InsertSales(SqlTransaction transaction, string paymentType, decimal totalAmount, int staffID)
        {
            var query = @"
              INSERT INTO Sales (SaleStatusID, SalesDate, StaffID)
              OUTPUT INSERTED.SalesID
              VALUES ((SELECT SaleStatusID FROM SaleStatus WHERE SaleStatusName = 'Completed'), GETDATE(), @StaffID)";

            return (int)_db.ExecuteScalar(query, new Dictionary<string, object>
            {
                { "@StaffID", staffID }
            }, transaction);
        }

        private void InsertSalesDetail(SqlTransaction transaction, int salesId, DataRow row)
        {
            var query = @"
                INSERT INTO SalesDetails (SalesID, ProductCode, Quantity, UnitPrice)
                VALUES (@SalesID, @ProductCode, @Quantity, @UnitPrice)";

            _db.ExecuteNonQuery(query, new Dictionary<string, object>
            {
                { "@SalesID", salesId },
                { "@ProductCode", row.Field<int>("ProductCode") },
                { "@Quantity", row.Field<int>("Quantity") },
                { "@UnitPrice", row.Field<decimal>("UnitPrice") }
            }, transaction);
        }

        private void UpdateInventory(SqlTransaction transaction, DataRow row)
        {
            var query = @"
                UPDATE Inventory
                SET Quantity = Quantity - @Quantity
                WHERE ProductCode = @ProductCode
                  AND Quantity >= @Quantity";

            var parameters = new Dictionary<string, object>
            {
                { "@Quantity", row.Field<int>("Quantity") },
                { "@ProductCode", row.Field<int>("ProductCode") }
            };

            var affectedRows = _db.ExecuteNonQuery(query, parameters, transaction);

            if (!affectedRows)
            {
                throw new InvalidOperationException($"Insufficient stock for Product ID: {row.Field<int>("ProductCode")}");
            }
        }
        private void InsertPayment(SqlTransaction transaction, int salesId, string paymentType, decimal amountPaid, string referenceNumber)
        {
            var query = @"
                INSERT INTO Payment (SalesID, PaymentType, AmountPaid, PaymentReference)
                VALUES (@SalesID, @PaymentType, @AmountPaid, @PaymentReference)";

            _db.ExecuteNonQuery(query, new Dictionary<string, object>
            {
                { "@SalesID", salesId },
                { "@PaymentType", paymentType },
                { "@AmountPaid", amountPaid },
                { "@PaymentReference", referenceNumber ?? (object)DBNull.Value }
            }, transaction);
        }
    }
}
