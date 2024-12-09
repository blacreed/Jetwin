using Jetwin.Database;
using Jetwin.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace Jetwin.Models
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly DataAccessLayer _db;

        public InventoryRepository()
        {
            _db = new DataAccessLayer();
        }

        public DataTable SearchInventory(string searchInput)
        {
            string query = @"
                SELECT 
                    i.InventoryID AS [Inventory ID],
                    p.ProductCode AS [Product Code],
                    p.ProductName AS [Product Name],
                    c.CategoryName AS [Category],
                    b.BrandName AS [Brand],
                    ISNULL(
                        STRING_AGG(
                            CASE 
                                WHEN at.AttributeTypeName IS NOT NULL AND av.AttributeValueName IS NOT NULL THEN CONCAT(at.AttributeTypeName, ': ', av.AttributeValueName)
                                ELSE NULL
                            END, 
                            ', '
                        ), 'N/A'
                    ) AS [Variant],
                    u.UoMName AS [Unit of Measurement],
                    i.Quantity AS [Current Stock],
                    i.MinimumStockLevel AS [Minimum Stock],
                    i.MaximumStockLevel AS [Maximum Stock],
                    i.ReorderPoint AS [Reorder Point],
                    i.Location AS [Location]
                FROM Product p
                LEFT JOIN Inventory i ON p.ProductCode = i.ProductCode
                LEFT JOIN Category c ON p.CategoryID = c.CategoryID
                LEFT JOIN Brand b ON p.BrandID = b.BrandID
                LEFT JOIN UnitOfMeasurement u ON p.UoMID = u.UoMID
                LEFT JOIN ProductAttributes pa ON pa.ProductCode = p.ProductCode
                LEFT JOIN AttributeType at ON pa.AttributeTypeID = at.AttributeTypeID
                LEFT JOIN AttributeValue av ON pa.AttributeValueID = av.AttributeValueID
                WHERE (@SearchInput IS NULL OR 
                       p.ProductName LIKE '%' + @SearchInput + '%' OR 
                       c.CategoryName LIKE '%' + @SearchInput + '%' OR 
                       b.BrandName LIKE '%' + @SearchInput + '%')
                GROUP BY 
                    i.InventoryID, p.ProductCode, p.ProductName, c.CategoryName, b.BrandName, 
                    u.UoMName, i.Quantity, i.MinimumStockLevel, i.MaximumStockLevel, 
                    i.ReorderPoint, i.Location
                ORDER BY p.ProductCode DESC";
            
            return _db.ExecuteQuery(query, new Dictionary<string, object>
            {
                { "@SearchInput", searchInput ?? (object)DBNull.Value }
            });
        }

        public bool SaveInventory(int productCode, int? attributeCombinationID, int quantity, int? minStock, int? maxStock, int? reorderPoint, string location)
        {

            string query = @"
                INSERT INTO Inventory (ProductCode, AttributeCombinationID, Quantity, MinimumStockLevel, MaximumStockLevel, ReorderPoint, Location)
                VALUES (@ProductCode, @AttributeCombinationID, @Quantity, @MinimumStock, @MaximumStock, @ReorderPoint, @Location)";

            return _db.ExecuteNonQuery(query, new Dictionary<string, object>
            {
                {@"ProductCode", productCode },
                {@"AttributeCombinationID", attributeCombinationID ?? (object)DBNull.Value},
                { "@Quantity", quantity },
                { "@MinimumStock", minStock ?? (object)DBNull.Value },
                { "@MaximumStock", maxStock ?? (object)DBNull.Value },
                { "@ReorderPoint", reorderPoint ?? (object)DBNull.Value },
                { "@Location", string.IsNullOrWhiteSpace(location) ? (object)DBNull.Value : location }
            });
        }
        public int? GetAttributeCombinationID(int productCode)
        {
            //query for an existing attribute combination
            string query = @"
                SELECT TOP 1 ProductAttributeID
                FROM ProductAttributes
                WHERE ProductCode = @ProductCode";

            var result = _db.ExecuteScalar(query, new Dictionary<string, object> { { "@ProductCode", productCode } });

            return result != null ? (int?)result : null;
        }
        public bool ProductRequiresAttributes(int productCode)
        {
            string query = @"
                SELECT COUNT(*)
                FROM ProductAttributes
                WHERE ProductCode = @ProductCode";

            int count = (int)_db.ExecuteScalar(query, new Dictionary<string, object> { { "@ProductCode", productCode } });
            return count > 0;
        }
        public bool UpdateInventory(int inventoryId, int productCode, int? attributeCombinationID, int quantity, int? minStock, int? maxStock, int? reorderPoint, string location)
        {
            string query = @"
                UPDATE Inventory
                SET ProductCode = @ProductCode,
                    AttributeCombinationID = @AttributeCombinationID,
                    Quantity = @Quantity,
                    MinimumStockLevel = @MinimumStock,
                    MaximumStockLevel = @MaximumStock,
                    ReorderPoint = @ReorderPoint,
                    Location = @Location
                WHERE InventoryID = @InventoryID";

            return _db.ExecuteNonQuery(query, new Dictionary<string, object>
            {
                { "@InventoryID", inventoryId },
                {@"ProductCode", productCode},
                { "@AttributeCombinationID", attributeCombinationID ?? (object)DBNull.Value },
                { "@Quantity", quantity },
                { "@MinimumStock", minStock ?? (object)DBNull.Value },
                { "@MaximumStock", maxStock ?? (object)DBNull.Value },
                { "@ReorderPoint", reorderPoint ?? (object)DBNull.Value },
                { "@Location", string.IsNullOrWhiteSpace(location) ? (object)DBNull.Value : location }
            });
        }

        //
        public bool SaveInventory(int quantity, int? minStock, int? maxStock, int? reorderPoint, int? maxStock1, string location)
        {
            throw new NotImplementedException();
        }

        public bool UpdateInventory(int inventoryId, int quantity, int? minStock, int? maxStock, int? reorderPoint, int? maxStock1, string location)
        {
            throw new NotImplementedException();
        }
        //

        //---FOR INVENTORY MODULE---
        public DataTable GetPricelist()
        {
            string query = @"
                SELECT 
                    p.ProductCode AS [Product Code], 
                    p.ProductName AS [Product], 
                    c.CategoryName AS [Category], 
                    b.BrandName AS [Brand],
                    ISNULL(
                        STRING_AGG(
                            CASE 
                                WHEN at.AttributeTypeName IS NOT NULL AND av.AttributeValueName IS NOT NULL THEN CONCAT(at.AttributeTypeName, ': ', av.AttributeValueName)
                                ELSE NULL
                            END, 
                            ', '
                        ), 'N/A'
                    ) AS [Variant],
                    u.UoMName AS [Unit of Measurement],
                    p.UnitPrice AS [Unit Price],
                    i.Location AS [Location],
                    s.Supplier,
                    CASE 
                        WHEN i.Quantity = 0 THEN 'Out of Stock'
                        WHEN i.Quantity < i.MinimumStockLevel THEN 'Low'
                        WHEN i.Quantity >= i.MinimumStockLevel AND i.Quantity <= i.MaximumStockLevel * 0.8 THEN 'Medium'
                        WHEN i.Quantity > i.MaximumStockLevel * 0.8 AND i.Quantity <= i.MaximumStockLevel THEN 'High'
                        WHEN i.Quantity > i.MaximumStockLevel THEN 'Overstock'
                    END AS [Stock Status]
                FROM Product p
                JOIN Category c ON p.CategoryID = c.CategoryID
                JOIN Brand b ON p.BrandID = b.BrandID
                JOIN UnitOfMeasurement u ON p.UoMID = u.UoMID
                LEFT JOIN Inventory i ON i.ProductCode = p.ProductCode
                LEFT JOIN ProductAttributes pa ON pa.ProductCode = p.ProductCode
                LEFT JOIN AttributeType at ON pa.AttributeTypeID = at.AttributeTypeID
                LEFT JOIN AttributeValue av ON pa.AttributeValueID = av.AttributeValueID
                JOIN Supplier s ON p.SupplierID = s.SupplierID
                WHERE p.StatusID = 1 AND s.StatusID = 1
                GROUP BY 
                    p.ProductCode, p.ProductName, c.CategoryName, b.BrandName, u.UoMName, 
                    p.UnitPrice, i.Location, s.Supplier, i.Quantity, i.MinimumStockLevel, i.MaximumStockLevel;";
            return _db.ExecuteQuery(query, null);
        }

        public DataTable GetStockLevel()
        {
            string query = @"
                SELECT 
                    p.ProductCode AS [Product Code], 
                    p.ProductName AS [Product],
                    c.CategoryName AS [Category], 
                    b.BrandName AS [Brand],
                    ISNULL(
                        STRING_AGG(
                            CASE 
                                WHEN at.AttributeTypeName IS NOT NULL AND av.AttributeValueName IS NOT NULL THEN CONCAT(at.AttributeTypeName, ': ', av.AttributeValueName)
                                ELSE NULL
                            END, 
                            ', '
                        ), 'N/A'
                    ) AS [Variant],
                    u.UoMName AS [Unit of Measurement],
                    i.Quantity AS [Current Stock],
                    i.MinimumStockLevel AS [Minimum Stock],
                    i.MaximumStockLevel AS [Maximum Stock],
                    i.ReorderPoint AS [Reorder Point],
                    CASE 
                        WHEN i.Quantity = 0 THEN 'Out of Stock'
                        WHEN i.Quantity < i.MinimumStockLevel THEN 'Low'
                        WHEN i.Quantity >= i.MinimumStockLevel AND i.Quantity <= i.MaximumStockLevel * 0.8 THEN 'Medium'
                        WHEN i.Quantity > i.MaximumStockLevel * 0.8 AND i.Quantity <= i.MaximumStockLevel THEN 'High'
                        WHEN i.Quantity > i.MaximumStockLevel THEN 'Overstock'
                    END AS [Stock Status]
                FROM Inventory i
                JOIN Product p ON i.ProductCode = p.ProductCode
                JOIN Category c ON p.CategoryID = c.CategoryID
                JOIN Brand b ON p.BrandID = b.BrandID
                JOIN UnitOfMeasurement u ON p.UoMID = u.UoMID
                LEFT JOIN ProductAttributes pa ON pa.ProductCode = p.ProductCode
                LEFT JOIN AttributeType at ON pa.AttributeTypeID = at.AttributeTypeID
                LEFT JOIN AttributeValue av ON pa.AttributeValueID = av.AttributeValueID
                WHERE p.StatusID = 1
                GROUP BY 
                    p.ProductCode, p.ProductName, c.CategoryName, b.BrandName, u.UoMName, 
                    i.Quantity, i.MinimumStockLevel, i.MaximumStockLevel, i.ReorderPoint;";

            return _db.ExecuteQuery(query, null);
        }

        public DataTable GetAdjustments()
        {
            string query = @"
                SELECT 
                    a.AdjustmentID,
                    p.ProductCode AS [Product Code],
                    p.ProductName AS [Product],
                    c.CategoryName AS [Category], 
                    b.BrandName AS [Brand],
                    ISNULL(
                        STRING_AGG(
                            CASE 
                                WHEN at.AttributeTypeName IS NOT NULL AND av.AttributeValueName IS NOT NULL THEN CONCAT(at.AttributeTypeName, ': ', av.AttributeValueName)
                                ELSE NULL
                            END, 
                            ', '
                        ), 'N/A'
                    ) AS [Variant],
                    u.UoMName AS [Unit of Measurement],
                    a.AdjustmentReason AS [Reason],
                    a.QuantityAdjusted AS [Quantity Adjusted],
                    a.AdjustmentDate AS [Date Adjusted]
                FROM Adjustments a
                JOIN Product p ON a.ProductCode = p.ProductCode
                JOIN Category c ON p.CategoryID = c.CategoryID
                JOIN Brand b ON p.BrandID = b.BrandID
                JOIN UnitOfMeasurement u ON p.UoMID = u.UoMID
                LEFT JOIN ProductAttributes pa ON pa.ProductCode = p.ProductCode
                LEFT JOIN AttributeType at ON pa.AttributeTypeID = at.AttributeTypeID
                LEFT JOIN AttributeValue av ON pa.AttributeValueID = av.AttributeValueID
                WHERE p.StatusID = 1
                GROUP BY a.AdjustmentID, p.ProductCode, p.ProductName, c.CategoryName, b.BrandName, u.UoMName, a.AdjustmentReason, a.QuantityAdjusted, a.AdjustmentDate";
            return _db.ExecuteQuery(query, null);
        }

        public DataTable GetRestocking()
        {
            string query = @"
                SELECT 
                    r.RestockingID,
                    p.ProductCode AS [Product Code],
                    p.ProductName AS [Product],
                    c.CategoryName AS [Category], 
                    b.BrandName AS [Brand],
                    ISNULL(
                        STRING_AGG(
                            CASE 
                                WHEN at.AttributeTypeName IS NOT NULL AND av.AttributeValueName IS NOT NULL THEN CONCAT(at.AttributeTypeName, ': ', av.AttributeValueName)
                                ELSE NULL
                            END, 
                            ', '
                        ), 'N/A'
                    ) AS [Variant],
                    u.UoMName AS [Unit of Measurement],
                    r.QuantityRestocked AS [Quantity Restocked], 
                    r.RestockDate AS [Restock Date], 
                    s.Supplier
                FROM Restocking r
                JOIN Product p ON r.ProductCode = p.ProductCode
                JOIN Category c ON p.CategoryID = c.CategoryID
                JOIN Brand b ON p.BrandID = b.BrandID
                JOIN UnitOfMeasurement u ON p.UoMID = u.UoMID
                LEFT JOIN ProductAttributes pa ON pa.ProductCode = p.ProductCode
                LEFT JOIN AttributeType at ON pa.AttributeTypeID = at.AttributeTypeID
                LEFT JOIN AttributeValue av ON pa.AttributeValueID = av.AttributeValueID
                JOIN Supplier s ON r.SupplierID = s.SupplierID
                WHERE p.StatusID = 1 AND s.StatusID = 1
                GROUP BY r.RestockingID, p.ProductCode, p.ProductName, c.CategoryName, b.BrandName, u.UoMName, r.QuantityRestocked, r.RestockDate, s.Supplier";
            return _db.ExecuteQuery(query, null);
        }

        public DataTable GetSuppliers()
        {
            string query = @"
                SELECT 
                    s.Supplier AS [Supplier], 
                    ci.ContactPerson AS [Contact Person], 
                    ci.ContactNum AS [Contact Number], 
                    s.Remarks, 
                    s.LastOrderDate AS [Last Order Date]
                FROM Supplier s
                JOIN SupplierContactInfo ci ON s.ContactID = ci.ContactID
                WHERE s.StatusID = 1";
            return _db.ExecuteQuery(query, null);
        }
    }
}
