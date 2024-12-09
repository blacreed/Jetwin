using Jetwin.Presenter.Entities;
using System.Collections.Generic;
using System.Data;

namespace Jetwin.Models.Interfaces
{
    public interface IProductRepository
    {
        DataTable GetProducts();
        bool SaveProduct(Product product, List<ProductAttribute> attributes);
        bool IsProductExists(string productName, List<ProductAttribute> attributes);
        DataTable GetCategories();
        DataTable GetBrands();
        DataTable GetSuppliers();
        DataTable GetUnitsOfMeasurement();
        DataTable GetAttributeTypes();
        DataTable GetAttributeValues(int attributeTypeID);
        bool SaveBrand(string brandName);
        bool IsBrandExists(string brandName);
        bool IsDuplicateBrand(int brandId, string brandName);
        bool IsCategoryExists(string categoryName);
        bool IsDuplicateCategory(int categoryId, string categoryName);
        bool SaveCategory(string categoryName);
        DataTable SearchProducts(string searchInput);
        DataTable SearchCategories(string searchInput);
        DataTable SearchBrands(string searchInput);

        bool ArchiveProduct(int productId);
        bool ArchiveCategory(int categoryId);
        bool ArchiveBrand(int brandId);
        bool UnarchiveBrand(int brandId);
        bool UnarchiveCategory(int categoryId);
        bool UnarchiveProduct(int productId);
        bool UpdateProduct(Product product, List<ProductAttribute> attributes);
        bool UpdateCategory(int categoryId, string categoryName);
        bool UpdateBrand(int brandId, string brandName);

        DataTable GetActiveCategories();
        DataTable GetActiveBrands();
    }
}
