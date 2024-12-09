using Jetwin.Models.Interfaces;
using Jetwin.Presenter;
using Jetwin.Utility;
using Jetwin.Views.Interfaces;
using System;
using System.Data;
using System.Windows.Forms;

namespace Jetwin.Modules
{
    public partial class SalesView : Form, ISalesView, INewTransactionView
    {
        private readonly SalesPresenter _salesPresenter;
        private readonly NewTransactionPresenter _transactPresenter;
        public SalesView(ISalesRepository repository, ITransactionRepository transactionRepository)
        {
            InitializeComponent();
            _salesPresenter = new SalesPresenter(this, repository);
            _transactPresenter = new NewTransactionPresenter(this, transactionRepository);

            //set textbox placeholders
            SetPlaceholders();

            // display default panel (viewing transaction, other panel is hidden which is new transaction panel)
            pnlViewSales.BringToFront();

            _transactPresenter.ConfigureLineItemsEvents(lineItemsDataGrid, btnRemove);

            Load += (s, e) => InitializeView();
        }
        
        private void InitializeView()
        {
            LoadData();
            SetEventHandlers();
        }
        private void LoadData()
        {
            _salesPresenter.LoadAllTransactions();
            _transactPresenter.LoadProducts();
            _transactPresenter.LoadCategories();
            _transactPresenter.LoadBrands();
        }
        private void SetEventHandlers()
        {
            //transaction complete subscription
            _transactPresenter.TransactionCompleted += _salesPresenter.LoadAllTransactions;

            //switch panel event handlers
            btnAddSales.Click += (s, e) => _salesPresenter.ShowAddTransactionPanel(btnAddSales, pnlViewSales, pnlNewTransact, btnBack);
            btnBack.Click += (s, e) => _salesPresenter.ShowViewTransactionPanel(btnAddSales, pnlViewSales, pnlNewTransact, btnBack);

            // event handlers for view sales panel
            btnAllTransact.Click += (s, e) => _salesPresenter.LoadAllTransactions(); //display all transactions regardless of status(completed, returned) (this is default panel)
            btnCompleted.Click += (s, e) => _salesPresenter.LoadFilteredTransactions("Completed");
            btnReturned.Click += (s, e) => _salesPresenter.LoadFilteredTransactions("Returned");
            transactionDataGrid.CellDoubleClick += (s, e) => _salesPresenter.ShowTransactionDetails(); //functionality of displaying detailed transaction on double click

            // event handlers for new transaction panel
            cbCategory.SelectedIndexChanged += (s, e) => _transactPresenter.FilterProducts();
            cbBrand.SelectedIndexChanged += (s, e) => _transactPresenter.FilterProducts();
            tbNTSearch.TextChanged += (s, e) => _transactPresenter.FilterProducts();
            tbQuantity.KeyPress += (s, e) =>
            {
                //allow numbers as input only
                e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
            };
            btnAddToCart.Click += (s, e) =>
            {
                _transactPresenter.AddToCart(); //put selected product in line items/cart
                tbQuantity.Clear(); //clear quantity textbox
            };
            btnCheckout.Click += (s, e) => _transactPresenter.Checkout(rbCash.Checked ? "Cash" : "Gcash");

            //this is add new transaction button (it load products into product selection data grid)
            btnAddSales.Click += (s, e) => _transactPresenter.LoadProducts();
        }
        private void SetPlaceholders()
        {
            TextBoxPlaceholder.SetPlaceholder(tbSearchSales, "Search sales...");
            TextBoxPlaceholder.SetPlaceholder(tbNTSearch, "Search for products");
            TextBoxPlaceholder.SetPlaceholder(tbQuantity, "Input quantity..");
        }
        public void HighlightActiveButton(string buttonName)
        {
            // reset all buttons and underline panels to inactive state
            btnAllTransact.ForeColor = System.Drawing.Color.Gray;
            btnCompleted.ForeColor = System.Drawing.Color.Gray;
            btnReturned.ForeColor = System.Drawing.Color.Gray;

            pnlUnderline1.Visible = false;
            pnlUnderline2.Visible = false;
            pnlUnderline3.Visible = false;

            // activate the selected button and underline panel
            if (buttonName == "btnAllTransact")
            {
                btnAllTransact.ForeColor = System.Drawing.Color.White;
                pnlUnderline1.Visible = true;
            }
            else if (buttonName == "Completed")
            {
                btnCompleted.ForeColor = System.Drawing.Color.White;
                pnlUnderline2.Visible = true;
            }
            else if (buttonName == "Returned")
            {
                btnReturned.ForeColor = System.Drawing.Color.White;
                pnlUnderline3.Visible = true;
            }
        }
        //#regionstart view sales panel methods
        public void PopulateTransactionGrid(DataTable data) => transactionDataGrid.DataSource = data;

        public void SetNumberOfSales(int totalSales) => lblNumberOfSales.Text = $"Total Sales: {totalSales}";

        public string SelectedStatusFilter => throw new NotImplementedException(); //not used yet, for future use

        public int SelectedTransactionID =>
            transactionDataGrid.SelectedRows.Count > 0 ?
            (int)transactionDataGrid.SelectedRows[0].Cells["Sales ID"].Value : 0;
        //#endregion

        //#regionstart new transaction panel methods
        public void SetQuantity(int quantity)
        {
            tbQuantity.Text = quantity.ToString();
        }
        public void PopulateProductGrid(DataTable data) => productsDataGrid.DataSource = data;

        public void PopulateLineItemsGrid(DataTable cartItems)
        {
            lineItemsDataGrid.DataSource = cartItems;

            //for fixing bug (display column header for attributes/variant)
            if (lineItemsDataGrid.Columns.Contains("Variant"))
            {
                lineItemsDataGrid.Columns["Variant"].HeaderText = "Variant";
            }

            //for fixing bug (hides the attribute combination id in column)
            if (lineItemsDataGrid.Columns.Contains("AttributeCombinationID"))
            {
                lineItemsDataGrid.Columns["AttributeCombinationID"].Visible = false;
            }
        }

        public void UpdateCartSummary(decimal subtotal, decimal discount, decimal total)
        {
            lblSubtotal.Text = subtotal.ToString("N2");
            lblDiscount.Text = discount.ToString("N2");
            lblTotal.Text = total.ToString("N2");
        }

        public void SetCartItemsCount(int count) => lblCartItems.Text = $"Cart Items: {count}";
        public void ClearInputs()
        {
            tbNTSearch.Clear();
            tbQuantity.Clear();
            cbCategory.SelectedIndex = 0;
            cbBrand.SelectedIndex = 0;
            lineItemsDataGrid.DataSource = null;
        }
        public void ResetToDefault()
        {
            pnlViewSales.SuspendLayout();
            pnlNewTransact.SuspendLayout();

            pnlNewTransact.Visible = false;
            pnlViewSales.Visible = false;
            btnAddSales.Visible = false;
            btnBack.Visible = false;

            // Show the default panel
            pnlViewSales.Visible = true;
            btnAddSales.Visible = true;
            _salesPresenter.LoadAllTransactions();

            pnlViewSales.ResumeLayout();
            pnlNewTransact.ResumeLayout();

            tbSearchSales.Clear();
            ClearInputs();
        }

        //#regionstart new transaction panel properties
        public string SelectedCategory //category combobox realtime searching
        {
            get
            {
                if (cbCategory.SelectedItem == null || cbCategory.SelectedIndex == 0)
                    return null; // return null for "All Categories" or no selection
                return cbCategory.SelectedItem.ToString();
            }
        }

        public string SelectedBrand //brand combobox realtime searching
        {
            get
            {
                if (cbBrand.SelectedItem == null || cbBrand.SelectedIndex == 0)
                    return null; // return null for "All Brands" or no selection
                return cbBrand.SelectedItem.ToString();
            }
        }
        public string ProductSearch //search field
        {
            get
            {
                var placeholder = "Search for products";
                return string.IsNullOrWhiteSpace(tbNTSearch.Text) || tbNTSearch.Text == placeholder
                    ? null
                    : tbNTSearch.Text.Trim();
            }
        }

        public int SelectedProductCode //realtime selection of product on row select
        {
            get
            {
                if (productsDataGrid.SelectedRows.Count > 0 &&
                    productsDataGrid.SelectedRows[0].Cells["Product Code"].Value != null)
                {
                    return (int)productsDataGrid.SelectedRows[0].Cells["Product Code"].Value;
                }
                return 0; // default to 0 if no row is selected or ProductID is null
            }
        }
        public int ProductQuantity //quantity inputted
        {
            get
            {
                if (int.TryParse(tbQuantity.Text, out int quantity) && quantity > 0) //input validation
                {
                    return quantity; // return the parsed quantity if valid (should be number and greater than 0)
                }
                return 0; // default to 0 if input is invalid
            }
        }
        public void PopulateCategories(DataTable categories)
        {
            cbCategory.Items.Clear();
            cbCategory.Items.Add("All Categories"); // optional default item
            foreach (DataRow row in categories.Rows)
            {
                cbCategory.Items.Add(row["CategoryName"].ToString());
            }
            cbCategory.SelectedIndex = 0; // set default selection
        }

        public void PopulateBrands(DataTable brands)
        {
            cbBrand.Items.Clear();
            cbBrand.Items.Add("All Brands"); // optional default item
            foreach (DataRow row in brands.Rows)
            {
                cbBrand.Items.Add(row["BrandName"].ToString());
            }
            cbBrand.SelectedIndex = 0; // set default selection
        }
        //#regionend
    }
}
