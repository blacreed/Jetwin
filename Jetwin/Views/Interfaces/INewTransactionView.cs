using System.Data;

namespace Jetwin.Views.Interfaces
{
    public interface INewTransactionView
    {
        string SelectedCategory { get; }
        string SelectedBrand { get; }
        string ProductSearch { get; }
        int SelectedProductCode { get; }
        int ProductQuantity { get; }

        void PopulateProductGrid(DataTable products);
        void PopulateLineItemsGrid(DataTable cartItems);
        void UpdateCartSummary(decimal subtotal, decimal discount, decimal total);
        void SetCartItemsCount(int count);
        void ClearInputs();

        // New Methods for Comboboxes
        void PopulateCategories(DataTable categories);
        void PopulateBrands(DataTable brands);
        void SetQuantity(int availableStock);
    }
}
