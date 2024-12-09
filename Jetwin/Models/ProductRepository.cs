using Jetwin.Database;
using Jetwin.Models.Interfaces;
using Jetwin.Presenter.Entities;
using System;
using System.Collections.Generic;
using System.Data;

namespace Jetwin.Models
{
    public class ProductRepository : IProductRepository
    {
        private readonly DataAccessLayer _db;

        public ProductRepository()
        {
            _db = new DataAccessLayer();
        }
        //#regionstart Products
        public DataTable SearchProducts(string searchInput)
        {
            string query = @"
                SELECT 
                    p.ProductCode AS [Product Code],
                    p.CategoryID,
                    p.BrandID,
                    p.SupplierID,
                    p.UoMID,
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
                    p.UnitPrice AS [Unit Price],
                    u.UoMName AS [Unit of Measurement],
                    s.Supplier AS [Supplier],
                    CASE 
                        WHEN p.StatusID = 1 THEN 'Active'
                        WHEN p.StatusID = 2 THEN 'Inactive'
                        WHEN p.StatusID = 3 THEN 'Archived'
                        ELSE 'Unknown'
                    END AS [Status]
                FROM Product p
                JOIN Category c ON p.CategoryID = c.CategoryID
                JOIN Brand b ON p.BrandID = b.BrandID
                JOIN UnitOfMeasurement u ON p.UoMID = u.UoMID
                JOIN Supplier s ON p.SupplierID = s.SupplierID
                LEFT JOIN ProductAttributes pa ON pa.ProductCode = p.ProductCode
                LEFT JOIN AttributeType at ON pa.AttributeTypeID = at.AttributeTypeID
                LEFT JOIN AttributeValue av ON pa.AttributeValueID = av.AttributeValueID
                WHERE (ISNULL(@SearchInput, '') = '' OR p.ProductName LIKE '%' + @SearchInput + '%')
                GROUP BY 
                    p.ProductCode, p.CategoryID, p.BrandID, p.SupplierID, p.UoMID, p.ProductName, c.CategoryName, b.BrandName,
                    p.UnitPrice, u.UoMName, s.Supplier, p.StatusID
                ORDER BY p.ProductCode DESC;";

            return _db.ExecuteQuery(query, new Dictionary<string, object>
            {
                { "@SearchInput", searchInput ?? (object)DBNull.Value }
            });
        }
        //PRODUCT SUBMODULE
        public DataTable GetProducts()
        {
            string query = @"
                SELECT 
                    p.ProductCode AS [Product Code],
                    p.CategoryID,
                    p.BrandID,
                    p.SupplierID,
                    p.UoMID,
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
                    p.UnitPrice AS [Unit Price],
                    u.UoMName AS [Unit of Measurement],
                    s.Supplier AS [Supplier],
                    CASE 
                        WHEN p.StatusID = 1 THEN 'Active'
                        WHEN p.StatusID = 2 THEN 'Inactive'
                        WHEN p.StatusID = 3 THEN 'Archived'
                        ELSE 'Unknown'
                    END AS [Status]
                FROM Product p
                JOIN Category c ON p.CategoryID = c.CategoryID
                JOIN Brand b ON p.BrandID = b.BrandID
                JOIN UnitOfMeasurement u ON p.UoMID = u.UoMID
                JOIN Supplier s ON p.SupplierID = s.SupplierID
                LEFT JOIN ProductAttributes pa ON pa.ProductCode = p.ProductCode
                LEFT JOIN AttributeType at ON pa.AttributeTypeID = at.AttributeTypeID
                LEFT JOIN AttributeValue av ON pa.AttributeValueID = av.AttributeValueID
                GROUP BY 
                    p.ProductCode, p.CategoryID, p.BrandID, p.SupplierID, p.UoMID, p.ProductName, c.CategoryName, b.BrandName, 
                    p.UnitPrice, u.UoMName, s.Supplier, p.StatusID
                ORDER BY p.ProductCode DESC;";

            return _db.ExecuteQuery(query, null);
        }

        public bool SaveProduct(Product product, List<ProductAttribute> attributes)
        {
            bool success = false;

            _db.ExecuteTransaction(transaction =>
            {
                string query = @"
                    INSERT INTO Product (ProductName, CategoryID, BrandID, SupplierID, UnitPrice, UoMID, StatusID)
                    VALUES (@ProductName, @CategoryID, @BrandID, @SupplierID, @UnitPrice, @UoMID, 1);
                    SELECT SCOPE_IDENTITY();";

                var result = _db.ExecuteScalar(query, new Dictionary<string, object>
                {
                    { "@ProductName", product.ProductName },
                    { "@CategoryID", product.CategoryID },
                    { "@BrandID", product.BrandID },
                    { "@SupplierID", product.SupplierID },
                    { "@UnitPrice", product.UnitPrice },
                    { "@UoMID", product.UoMID }
                }, transaction);

                int productCode = Convert.ToInt32(result);

                foreach (var attr in attributes)
                {
                    string attrQuery = @"
                        INSERT INTO ProductAttributes (ProductCode, AttributeTypeID, AttributeValueID)
                        VALUES (@ProductCode, @AttributeTypeID, @AttributeValueID)";

                    _db.ExecuteNonQuery(attrQuery, new Dictionary<string, object>
                    {
                        { "@ProductCode", productCode },
                        { "@AttributeTypeID", attr.AttributeTypeID },
                        { "@AttributeValueID", attr.AttributeValueID }
                    }, transaction);
                    }

                success = true;
            });

            return success;
        }

        public bool IsProductExists(string productName, List<ProductAttribute> attributes)
        {
            var products = _db.ExecuteQuery("SELECT ProductCode FROM Product WHERE ProductName = @ProductName", new Dictionary<string, object> { { "@ProductName", productName } });
            if (products.Rows.Count == 0) return false;

            var productCode = (int)products.Rows[0]["ProductCode"];
            var productAttributes = _db.ExecuteQuery("SELECT AttributeTypeID, AttributeValueID FROM ProductAttributes WHERE ProductCode = @ProductCode", new Dictionary<string, object> { { "@ProductCode", productCode } });

            foreach (var attribute in attributes)
            {
                bool found = false;
                foreach (DataRow productAttribute in productAttributes.Rows)
                {
                    if ((int)productAttribute["AttributeTypeID"] == attribute.AttributeTypeID &&
                        (int)productAttribute["AttributeValueID"] == attribute.AttributeValueID)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found) return false;
            }
            return true;
        }
        //#regionend
        //#regionstart Category
        public DataTable SearchCategories(string searchInput)
        {
            string query = @"
                SELECT 
                    c.CategoryID AS [Category ID],
                    c.CategoryName AS [Category Name],
                    CASE 
                        WHEN c.StatusID = 1 THEN 'Active'
                        WHEN c.StatusID = 2 THEN 'Inactive'
                        WHEN c.StatusID = 3 THEN 'Archived'
                        ELSE 'Unknown'
                    END AS [Status]
                FROM Category c
                WHERE (ISNULL(@SearchInput, '') = '' OR c.CategoryName LIKE '%' + @SearchInput + '%')
                ORDER BY c.CategoryID DESC;";

            return _db.ExecuteQuery(query, new Dictionary<string, object>
            {
                { "@SearchInput", searchInput ?? (object)DBNull.Value }
            });
        }
        public DataTable GetCategories()
        {
            string query = @"
                SELECT 
                    c.CategoryID AS [Category ID],
                    c.CategoryName AS [Category Name],
                    CASE 
                        WHEN c.StatusID = 1 THEN 'Active'
                        WHEN c.StatusID = 2 THEN 'Inactive'
                        WHEN c.StatusID = 3 THEN 'Archived'
                        ELSE 'Unknown'
                    END AS [Status]
                FROM Category c
                ORDER BY c.CategoryID DESC;";

            return _db.ExecuteQuery(query, null);
        }
        public DataTable GetActiveCategories()
        {
            string query = @"
                SELECT 
                    c.CategoryID AS [Category ID],
                    c.CategoryName AS [Category Name]
                FROM Category c
                WHERE c.StatusID = 1
                ORDER BY c.CategoryID DESC;";

            return _db.ExecuteQuery(query, null);
        }
        //#regionend Category
        //#regionstart Brand
        public DataTable SearchBrands(string searchInput)
        {
            string query = @"
                SELECT 
                    b.BrandID AS [Brand ID],
                    b.BrandName AS [Brand Name],
                    CASE 
                        WHEN b.StatusID = 1 THEN 'Active'
                        WHEN b.StatusID = 2 THEN 'Inactive'
                        WHEN b.StatusID = 3 THEN 'Archived'
                        ELSE 'Unknown'
                    END AS [Status]
                FROM Brand b
                WHERE (ISNULL(@SearchInput, '') = '' OR b.BrandName LIKE '%' + @SearchInput + '%')
                ORDER BY b.BrandID DESC;";

            return _db.ExecuteQuery(query, new Dictionary<string, object>
            {
                { "@SearchInput", searchInput ?? (object)DBNull.Value }
            });
        }
        public DataTable GetBrands()
        {
            string query = @"
                SELECT 
                    b.BrandID AS [Brand ID],
                    b.BrandName AS [Brand Name],
                    CASE 
                        WHEN b.StatusID = 1 THEN 'Active'
                        WHEN b.StatusID = 2 THEN 'Inactive'
                        WHEN b.StatusID = 3 THEN 'Archived'
                        ELSE 'Unknown'
                    END AS [Status]
                FROM Brand b
                ORDER BY b.BrandID DESC;";

            return _db.ExecuteQuery(query, null);
        }
        public DataTable GetActiveBrands()
        {
            string query = @"
                SELECT 
                    b.BrandID AS [Brand ID],
                    b.BrandName AS [Brand Name]
                FROM Brand b
                WHERE b.StatusID = 1
                ORDER BY b.BrandID DESC;";

            return _db.ExecuteQuery(query, null);
        }

        //#regionend brand
        //#regionstart supplier

        public DataTable GetSuppliers() => _db.ExecuteQuery("SELECT * FROM Supplier", null);

        //#regionend supplier

        public DataTable GetUnitsOfMeasurement() => _db.ExecuteQuery("SELECT * FROM UnitOfMeasurement", null);

        //#regionstart attribute
        public DataTable SearchAttributes(string searchInput) //discarded for now
        {
            string query = @"
                SELECT 
                    at.AttributeTypeName AS [Attribute Type],
                    av.AttributeValueName AS [Attribute Value],
                    CASE 
                        WHEN at.StatusID = 1 THEN 'Active'
                        WHEN at.StatusID = 2 THEN 'Inactive'
                        WHEN at.StatusID = 3 THEN 'Archived'
                        ELSE 'Unknown'
                    END AS [Type Status],
                    CASE 
                        WHEN av.StatusID = 1 THEN 'Active'
                        WHEN av.StatusID = 2 THEN 'Inactive'
                        WHEN av.StatusID = 3 THEN 'Archived'
                        ELSE 'Unknown'
                    END AS [Value Status]
                FROM AttributeType at
                LEFT JOIN AttributeValue av ON at.AttributeTypeID = av.AttributeTypeID
                WHERE (ISNULL(@SearchInput, '') = '' 
                       OR at.AttributeTypeName LIKE '%' + @SearchInput + '%' 
                       OR av.AttributeValueName LIKE '%' + @SearchInput + '%')
                ORDER BY at.AttributeTypeName, av.AttributeValueName;";

            return _db.ExecuteQuery(query, new Dictionary<string, object>
            {
                { "@SearchInput", searchInput ?? (object)DBNull.Value }
            });
        }

        public DataTable GetAttributeTypes() => _db.ExecuteQuery("SELECT * FROM AttributeType", null);

        public DataTable GetAttributeValues(int attributeTypeID) =>
            _db.ExecuteQuery("SELECT * FROM AttributeValue WHERE AttributeTypeID = @TypeID", new Dictionary<string, object>
            {
                { "@TypeID", attributeTypeID }
            });
        //#regionend Attribute

        //CATEGORY BRAND SUBMODULE
        public bool SaveCategory(string categoryName)
        {
            string query = "INSERT INTO Category (CategoryName, StatusID) VALUES (@Name, 1)";
            return _db.ExecuteNonQuery(query, new Dictionary<string, object> { { "@Name", categoryName } });
        }
        // Check if a category exists
        public bool IsCategoryExists(string categoryName)
        {
            string query = "SELECT COUNT(*) FROM Category WHERE CategoryName = @Name";
            return (int)_db.ExecuteScalar(query, new Dictionary<string, object> { { "@Name", categoryName } }) > 0;
        }
        public bool IsDuplicateCategory(int categoryId, string categoryName)
        {
            string query = @"
                SELECT COUNT(*)
                FROM Category
                WHERE CategoryName = @CategoryName
                  AND CategoryID != @CategoryID";

            return (int)_db.ExecuteScalar(query, new Dictionary<string, object>
            {
                { "@CategoryName", categoryName },
                { "@CategoryID", categoryId }
            }) > 0;
        }
        public bool SaveBrand(string brandName)
        {
            string query = "INSERT INTO Brand (BrandName, StatusID) VALUES (@Name, 1)";
            return _db.ExecuteNonQuery(query, new Dictionary<string, object> { { "@Name", brandName } });
        }
        //check if a brand exists
        public bool IsBrandExists(string brandName)
        {
            string query = "SELECT COUNT(*) FROM Brand WHERE BrandName = @Name";
            return (int)_db.ExecuteScalar(query, new Dictionary<string, object> { { "@Name", brandName } }) > 0;
        }
        public bool IsDuplicateBrand(int brandId, string brandName)
        {
            string query = @"
                SELECT COUNT(*)
                FROM Brand
                WHERE BrandName = @BrandName
                  AND BrandID != @BrandID";

            return (int)_db.ExecuteScalar(query, new Dictionary<string, object>
            {
                { "@BrandName", brandName },
                { "@BrandID", brandId }
            }) > 0;
        }

        //#startregion Archive
        public bool ArchiveProduct(int productId)
        {
            string query = "UPDATE Product SET StatusID = 3 WHERE ProductCode = @ProductID";
            return _db.ExecuteNonQuery(query, new Dictionary<string, object> { { "@ProductID", productId } });
        }
        public bool UnarchiveProduct(int productId)
        {
            string query = "UPDATE Product SET StatusID = 1 WHERE ProductCode = @ProductID";
            return _db.ExecuteNonQuery(query, new Dictionary<string, object> { { "@ProductID", productId } });
        }
        public bool ArchiveCategory(int categoryId)
        {
            string query = "UPDATE Category SET StatusID = 3 WHERE CategoryID = @CategoryID";
            return _db.ExecuteNonQuery(query, new Dictionary<string, object> { { "@CategoryID", categoryId } });
        }
        public bool UnarchiveCategory(int categoryId)
        {
            string query = "UPDATE Category SET StatusID = 1 WHERE CategoryID = @CategoryID";
            return _db.ExecuteNonQuery(query, new Dictionary<string, object> { { "@CategoryID", categoryId } });
        }
        public bool ArchiveBrand(int brandId)
        {
            string query = "UPDATE Brand SET StatusID = 3 WHERE BrandID = @BrandID";
            return _db.ExecuteNonQuery(query, new Dictionary<string, object> { { "@BrandID", brandId } });
        }
        public bool UnarchiveBrand(int brandId)
        {
            string query = "UPDATE Brand SET StatusID = 1 WHERE BrandID = @BrandID";
            return _db.ExecuteNonQuery(query, new Dictionary<string, object> { { "@BrandID", brandId } });
        }

        public bool UpdateProduct(Product product, List<ProductAttribute> attributes)
        {
            string query = @"
                UPDATE Product
                SET ProductName = @ProductName,
                    CategoryID = @CategoryID,
                    BrandID = @BrandID,
                    SupplierID = @SupplierID,
                    UnitPrice = @UnitPrice,
                    UoMID = @UoMID
                WHERE ProductCode = @ProductCode";

            var parameters = new Dictionary<string, object>
            {
                { "@ProductCode", product.ProductCode },
                { "@ProductName", product.ProductName },
                { "@CategoryID", product.CategoryID },
                { "@BrandID", product.BrandID },
                { "@SupplierID", product.SupplierID },
                { "@UnitPrice", product.UnitPrice },
                { "@UoMID", product.UoMID }
            };

            return _db.ExecuteNonQuery(query, parameters);
        }
        public bool UpdateCategory(int categoryId, string categoryName)
        {
            string query = @"
                UPDATE Category
                SET CategoryName = @CategoryName
                WHERE CategoryID = @CategoryID";

            var parameters =  new Dictionary<string, object>
            {
                { "@CategoryName", categoryName },
                { "@CategoryID", categoryId }
            };
            return _db.ExecuteNonQuery(query, parameters);
        }
        public bool UpdateBrand(int brandId, string brandName)
        {
            string query = @"
                UPDATE Brand
                SET BrandName = @BrandName
                WHERE BrandID = @BrandID";

            var parameters = new Dictionary<string, object>
            {
                { "@BrandName", brandName },
                { "@BrandID", brandId }
            };
            return _db.ExecuteNonQuery(query, parameters);
        }
    }
}
