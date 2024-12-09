using Jetwin.Models.Interfaces;
using Jetwin.Models;
using Jetwin.Presenter.Entities;
using System.Collections.Generic;
using System.Data;
using Jetwin.Modules;
using System;

namespace Jetwin.Presenter
{
    public class ProductPresenter //MAINTENANCE MODULE -> PRODUCT SUBMODULE
    {
        private readonly MaintenanceView _view;
        private readonly IProductRepository _repository;
        private DataTable _cachedProducts;
        public static event Action ProductUpdated;
        public ProductPresenter(MaintenanceView view)
        {
            _view = view;
            _repository = new ProductRepository();
        }
        //#regionstart search functionality
        public void SearchProducts(string searchInput)
        {
            var products = _repository.SearchProducts(searchInput);
            _view.DisplayProducts(products);
        }
        public void SearchCategories(string searchInput)
        {
            var categories = _repository.SearchCategories(searchInput);
            _view.DisplayCategories(categories);
        }
        public void SearchBrands(string searchInput)
        {
            var brands = _repository.SearchBrands(searchInput);
            _view.DisplayBrands(brands);
        }
        //#regionend
        //PRODUCT SUBMODULE
        //#regionstart loading of data
        public DataTable LoadProducts()
        {
            var products = _repository.GetProducts();
            _view.DisplayProducts(products);

            return products;
        }

        public DataTable LoadCategories()
        {
            var categories = _repository.GetCategories();
            _view.DisplayCategories(categories);
            return categories;
        }
        public DataTable LoadActiveCategories()
        {
            return _repository.GetActiveCategories();
        }
        public DataTable LoadActiveBrands()
        {
            return _repository.GetActiveBrands();
        }

        public DataTable LoadBrands()
        {
            var brands = _repository.GetBrands();
            _view.DisplayBrands(brands);
            return brands;
        }

        public DataTable LoadSuppliers()
        {
            return _repository.GetSuppliers();
        }

        public void LoadUnitsOfMeasurement()
        {
            var units = _repository.GetUnitsOfMeasurement();
            _view.PopulateUnitsOfMeasurement(units);
        }

        public DataTable LoadAttributeTypes()
        {
            var attributeTypes = _repository.GetAttributeTypes();
            //_view.DisplayAttributes(attributeTypes);
            return attributeTypes;
        }

        public DataTable LoadAttributeValues(int attributeTypeID)
        {
            return _repository.GetAttributeValues(attributeTypeID);
        }
        //regionend
        public void SaveProduct(Product product, List<ProductAttribute> attributes)
        {
            //if editing a product
            if(_view.editingId.HasValue)
            {
                product.ProductCode = _view.editingId.Value;
                if (_repository.UpdateProduct(product, attributes))
                {
                    _view.ShowValidationMessage("Product updated successfully.");
                    LoadProducts();
                    ProductUpdated?.Invoke();
                    _view.ClearProductInputs();
                    _view.ResetEditState();
                }
                else
                {
                    _view.ShowValidationMessage("Failed to update product.");
                }
            }
            //if just adding a product, not editing
            else
            {
                //validation check
                if (_repository.IsProductExists(product.ProductName, attributes))
                {
                    _view.ShowValidationMessage("Product with the same details and attributes already exists.");
                    return;
                }

                if (_repository.SaveProduct(product, attributes))
                {
                    _view.ShowValidationMessage("Product saved successfully.");
                    LoadProducts();
                    ProductUpdated?.Invoke();
                    _view.ClearProductInputs();
                }
                else
                {
                    _view.ShowValidationMessage("Failed to save product.");
                }
            }
            
        }

        //CATEGORY, BRAND, ATTRIBUTE SUBMODULE
        //regionstart save methods
        public void SaveCategory(string categoryName)
        {
            
            if (_view.editingId.HasValue) // Edit operation
            {

                if (_repository.UpdateCategory(_view.editingId.Value, categoryName))
                {
                    _view.ShowValidationMessage("Category updated successfully.");
                    _view.PopulateCategories(_repository.GetCategories());
                    _view.DisplayCategories(_repository.GetCategories());
                    _view.ClearCategoryBrandInput();
                    _view.ResetEditState();
                }
                else
                {
                    _view.ShowValidationMessage("Failed to update category.");
                }
            }
            else
            {
                if (_repository.IsCategoryExists(categoryName))
                {
                    _view.ShowValidationMessage("Category name already exists. Please use a unique name.");
                    return;
                }

                if (_repository.SaveCategory(categoryName))
                {
                    _view.ShowValidationMessage("Category saved successfully.");
                    _view.PopulateCategories(_repository.GetCategories());
                    _view.DisplayCategories(_repository.GetCategories());
                }
                else
                {
                    _view.ShowValidationMessage("Failed to save category.");
                }
            }
        }

        public void SaveBrand(string brandName)
        {
            if (_view.editingId.HasValue) // Edit operation
            {

                if (_repository.UpdateBrand(_view.editingId.Value, brandName))
                {
                    _view.ShowValidationMessage("Brand updated successfully.");
                    _view.PopulateBrands(_repository.GetBrands());
                    _view.DisplayBrands(_repository.GetBrands());
                    _view.ClearCategoryBrandInput();
                    _view.ResetEditState();
                }
                else
                {
                    _view.ShowValidationMessage("Failed to update brand.");
                }
            }
            else
            {
                if (_repository.IsBrandExists(brandName))
                {
                    _view.ShowValidationMessage("Brand name already exists. Please use a unique name.");
                    return;
                }

                if (_repository.SaveBrand(brandName))
                {
                    _view.ShowValidationMessage("Brand saved successfully.");
                    _view.PopulateBrands(_repository.GetBrands());
                    _view.DisplayBrands(_repository.GetBrands());
                }
                else
                {
                    _view.ShowValidationMessage("Failed to save brand.");
                }
            }
        }
        //endregion
        //#startregion archive and unarchive (lacks functionality for updating button text of archive button for unarchiving)
        public void ArchiveProduct(int productId)
        {
            if (!_repository.ArchiveProduct(productId))
            {
                _view.ShowValidationMessage("Failed to archive product.");
                return;
            }

            _view.ShowValidationMessage("Product archived successfully.");
            ProductUpdated?.Invoke();
            LoadProducts();
        }
        public void UnarchiveProduct(int productId)
        {
            if (_repository.UnarchiveProduct(productId))
            {
                _view.ShowValidationMessage("Product unarchived successfully.");
                ProductUpdated?.Invoke();
                LoadProducts();
            }
            else
            {
                _view.ShowValidationMessage("Failed to unarchive product.");
            }
        }
        public void ArchiveCategory(int categoryId)
        {
            if (!_repository.ArchiveCategory(categoryId))
            {
                _view.ShowValidationMessage("Failed to archive category.");
                return;
            }

            _view.ShowValidationMessage("Category archived successfully.");
            LoadCategories();
        }
        public void UnarchiveCategory(int categoryId)
        {
            if (_repository.UnarchiveCategory(categoryId))
            {
                _view.ShowValidationMessage("Category unarchived successfully.");
                LoadCategories();
            }
            else
            {
                _view.ShowValidationMessage("Failed to unarchive category.");
            }
        }
        public void ArchiveBrand(int brandId)
        {
            if (!_repository.ArchiveBrand(brandId))
            {
                _view.ShowValidationMessage("Failed to archive brand.");
                return;
            }

            _view.ShowValidationMessage("Brand archived successfully.");
            LoadBrands();
        }
        public void UnarchiveBrand(int brandId)
        {
            if (_repository.UnarchiveBrand(brandId))
            {
                _view.ShowValidationMessage("Brand unarchived successfully.");
                LoadBrands();
            }
            else
            {
                _view.ShowValidationMessage("Failed to unarchive brand.");

            }
        }
    }
}