using Jetwin.Models;
using Jetwin.Models.Interfaces;
using Jetwin.Modules;
using Jetwin.Views;
using Jetwin.Views.Interfaces;
using Jetwin.Views.Others;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace Jetwin.Presenter
{
    public class NewTransactionPresenter //SALES MODULE -> NEW TRANSACTION
    {
        private readonly INewTransactionView _view;
        private readonly ITransactionRepository _repository;
        private DataTable _cartItems;
        public event Action TransactionCompleted;
        public static event Action TransactionCompletedStatic;

        private readonly DashboardRepository dashboardRepository;
        private readonly DashboardView dashboardView;
        private readonly DashboardPresenter dashboardPresenter;
        public NewTransactionPresenter(INewTransactionView view, ITransactionRepository repository)
        {
            _view = view;
            _repository = repository;
            _cartItems = new DataTable();
            InitializeCart();

            dashboardRepository = new DashboardRepository();
            dashboardView = new DashboardView(dashboardRepository);
            dashboardPresenter = new DashboardPresenter(dashboardView, dashboardRepository);
        }

        private void InitializeCart()
        {
            _cartItems.Columns.Add("ProductCode", typeof(int));
            _cartItems.Columns.Add("ProductName", typeof(string));
            _cartItems.Columns.Add("Brand", typeof(string));
            _cartItems.Columns.Add("Category", typeof(string));
            _cartItems.Columns.Add("Variant", typeof(string));
            _cartItems.Columns.Add("AttributeCombinationID", typeof(int));
            _cartItems.Columns.Add("Quantity", typeof(int));
            _cartItems.Columns.Add("UnitPrice", typeof(decimal));
            _cartItems.Columns.Add("Total", typeof(decimal));
        }

        public void LoadProducts()
        {
            var products = _repository.GetProducts(
                _view.SelectedCategory, _view.SelectedBrand, _view.ProductSearch);
            _view.PopulateProductGrid(products);
        }

        public void AddToCart()
        {
            if (_view.SelectedProductCode is 0 || _view.ProductQuantity <= 0)
            {
                MessageBox.Show("Please select a valid product and quantity.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var product = _repository.GetProductDetails(_view.SelectedProductCode);

            // Ensure stock availability
            int availableStock = GetStockForProduct(product.ProductCode);
            if (_view.ProductQuantity > availableStock)
            {
                _view.SetQuantity(availableStock);
            }

            // Check if product already exists in the cart
            var existingRow = _cartItems.AsEnumerable()
                .FirstOrDefault(row => row.Field<int>("ProductCode") == product.ProductCode);

            if (existingRow != null)
            {
                // Update quantity if it doesn't exceed the stock
                int currentQuantity = existingRow.Field<int>("Quantity");
                int newQuantity = currentQuantity + _view.ProductQuantity;

                if (newQuantity > availableStock)
                {
                    // Adjust to the maximum available stock
                    int maxAddableQuantity = availableStock - currentQuantity;
                    _view.SetQuantity(maxAddableQuantity > 0 ? maxAddableQuantity : 0);

                    MessageBox.Show($"Insufficient stock for '{product.ProductName}'.", "Stock Limit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    if (_view.ProductQuantity <= 0)
                        return; // Exit if no quantity can be added
                }

                // Update existing row
                existingRow["Quantity"] = newQuantity;
                existingRow["Total"] = newQuantity * product.UnitPrice;
            }
            else
            {
                if (_view.ProductQuantity > availableStock)
                {
                    MessageBox.Show($"Insufficient stock for '{product.ProductName}'.", "Stock Limit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Add new row
                _cartItems.Rows.Add(
                    product.ProductCode,
                    product.ProductName,
                    product.Brand,
                    product.Category,
                    product.Variant,
                    product.AttributeCombinationID,
                    _view.ProductQuantity,
                    product.UnitPrice,
                    product.UnitPrice * _view.ProductQuantity
                );
            }

            _view.PopulateLineItemsGrid(_cartItems);
            UpdateCartSummary();
        }
        private int GetStockForProduct(int productCode)
        {
            var stockData = _repository.GetStock(productCode);
            if (stockData == null || ((DataTable)stockData).Rows.Count == 0)
                return 0; // Default to 0 if no stock found

            return (int)((DataTable)stockData).Rows[0]["Quantity"];
        }
        private void UpdateCartSummary()
        {
            var subtotal = 0M;
            foreach (DataRow row in _cartItems.Rows)
                subtotal += Convert.ToDecimal(row["Total"]);

            var discount = 0M; // Placeholder for future discounts
            var total = subtotal - discount;

            _view.UpdateCartSummary(subtotal, discount, total);
            _view.SetCartItemsCount(_cartItems.Rows.Count);
        }
        public void FilterProducts()
        {

            // Convert "All Brand" and "All Category" to null for filtering
            var selectedCategory = _view.SelectedCategory == "All Categories" ? null : _view.SelectedCategory;
            var selectedBrand = _view.SelectedBrand == "All Brands" ? null : _view.SelectedBrand;
            var searchKeyword = _view.ProductSearch;

            // Retrieve filtered products
            var products = _repository.GetProducts(selectedCategory, selectedBrand, searchKeyword);

            // Update the product grid
            _view.PopulateProductGrid(products);
        }
        public void ConfigureLineItemsEvents(DataGridView lineItemsDataGrid, Button btnRemove)
        {
            // Hook selection event for enabling/disabling btnRemove
            lineItemsDataGrid.SelectionChanged += (s, e) =>
            {
                btnRemove.Enabled = lineItemsDataGrid.SelectedRows.Count > 0;
            };

            // Hook click event for removing items
            btnRemove.Click += (s, e) =>
            {
                if (lineItemsDataGrid.SelectedRows.Count > 0)
                {
                    var rowIndex = lineItemsDataGrid.SelectedRows[0].Index;
                    _cartItems.Rows.RemoveAt(rowIndex);
                    _view.PopulateLineItemsGrid(_cartItems);
                    UpdateCartSummary();
                }
            };
        }

        public void ConfirmTransaction(DataTable cartItems, string paymentType, decimal amountPaid, string referenceNumber = null)
        {
            if (cartItems == null || cartItems.Rows.Count == 0)
            {
                MessageBox.Show("Cart is empty. Cannot proceed with the transaction.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                //GET THE STAFF ID
                var staffID = UserSession.StaffID;
                if (staffID == 0)
                {
                    MessageBox.Show("Invalid session. Please log in again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // Execute the transaction
                _repository.ExecuteTransac(cartItems, paymentType, amountPaid, staffID, referenceNumber);

                TransactionCompletedStatic?.Invoke();
                // Clear cart and update UI
                _view.ClearInputs();
                _view.PopulateLineItemsGrid(null);
                _view.UpdateCartSummary(0, 0, 0);
                MessageBox.Show("Transaction completed successfully!", "Success");

                // Raise the event to notify listeners
                LoadProducts();
                TransactionCompleted?.Invoke();
                
                dashboardPresenter.LoadDashboardData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Transaction failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public bool IsReferenceNumberUnique(string referenceNumber)
        {
            if(_repository.IsReferenceNumberUnique(referenceNumber))
            {
                MessageBox.Show("Reference number already exists. Please enter a unique reference number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        public void Checkout(string paymentType)
        {
            if (_cartItems == null || _cartItems.Rows.Count == 0)
            {
                MessageBox.Show("Cart is empty. Please add items before checkout.", "Error");
                return;
            }
            var balance = 0M;
            foreach (DataRow row in _cartItems.Rows)
                balance += Convert.ToDecimal(row["Total"]);

            if (paymentType == "Cash")
            {
                var form = new CashPayment(balance, this, _cartItems);
                form.ShowDialog();
            }
            else if (paymentType == "Gcash")
            {
                var form = new GCashPayment(balance, this, _cartItems);
                form.ShowDialog();
            }

            ClearTransaction();
        }
        public void LoadCategories()
        {
            var categories = _repository.GetDistinctCategories();
            _view.PopulateCategories(categories);
        }

        public void LoadBrands()
        {
            var brands = _repository.GetDistinctBrands();
            _view.PopulateBrands(brands);
        }
        public void ClearTransaction()
        {
            _cartItems.Clear();
            _view.PopulateLineItemsGrid(_cartItems);
            UpdateCartSummary();
            _view.ClearInputs();
        }
    }
}

