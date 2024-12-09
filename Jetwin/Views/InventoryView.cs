using Jetwin.Presenter;
using Jetwin.Utility;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Jetwin.Modules
{
    public partial class InventoryView : Form
    {
        private readonly InventoryModulePresenter _inventoryPresenter;
        public InventoryView()
        {
            InitializeComponent();
            _inventoryPresenter = new InventoryModulePresenter(this);

            InitializeView();
        }
        private void InitializeView()
        {
            // Default submodule panel and active button
            ActivateSubmodule("Price List");
            InitializeEventHandlers();
            SetPlaceHolders();
        }
        private void InitializeEventHandlers()
        {
            btnPriceList.Click += (s, e) => ActivateSubmodule("Price List");
            btnStockLevel.Click += (s, e) => ActivateSubmodule("Stock Level");
            btnAdjustment.Click += (s, e) => ActivateSubmodule("Adjustment");
            btnRestocking.Click += (s, e) => ActivateSubmodule("Restocking");
            btnSupplier.Click += (s, e) => ActivateSubmodule("Supplier");
        }
        private void SetPlaceHolders()
        {
            TextBoxPlaceholder.SetPlaceholder(tbPriceListSearch, "Search products...");
            TextBoxPlaceholder.SetPlaceholder(tbStockLevelSearch, "Search products...");
            TextBoxPlaceholder.SetPlaceholder(tbAdjustmentSearch, "Search products...");
            TextBoxPlaceholder.SetPlaceholder(tbRestockingSearch, "Search products...");
            TextBoxPlaceholder.SetPlaceholder(tbSupplierSearch, "Search suppliers...");
        }
        private void ActivateSubmodule(string submodule)
        {
            HideAllPanels();
            HideUnderlines();

            switch(submodule)
            {
                case "Price List":
                    pnlPriceList.Visible = true;
                    pnlPriceListUnderline.Visible = true;
                    _inventoryPresenter.LoadPricelist();
                    break;
                case "Stock Level":
                    pnlStockLevel.Visible = true;
                    pnlStockLevelUnderline.Visible = true;
                    _inventoryPresenter.LoadStockLevel();
                    break;
                case "Adjustment":
                    pnlAdjustment.Visible = true;
                    pnlAdjustmentUnderline.Visible = true;
                    _inventoryPresenter.LoadAdjustments();
                    break;
                case "Restocking":
                    pnlRestocking.Visible = true;
                    pnlRestockingUnderline.Visible = true;
                    _inventoryPresenter.LoadRestocking();
                    break;
                case "Supplier":
                    pnlSupplier.Visible = true;
                    pnlSupplierUnderline.Visible = true;
                    _inventoryPresenter.LoadSuppliers();
                    break;
            }
            UpdateButtonStyles(submodule);
        }
        private void UpdateButtonStyles(string submodule)
        {
            btnPriceList.ForeColor = submodule == "Price List" ? Color.White : Color.Gray;
            btnStockLevel.ForeColor = submodule == "Stock Level" ? Color.White : Color.Gray;
            btnAdjustment.ForeColor = submodule == "Adjustment" ? Color.White : Color.Gray;
            btnRestocking.ForeColor = submodule == "Restocking" ? Color.White : Color.Gray;
            btnSupplier.ForeColor = submodule == "Supplier" ? Color.White : Color.Gray;
        }
        private void HideUnderlines()
        {
            pnlPriceListUnderline.Visible = false;
            pnlStockLevelUnderline.Visible = false;
            pnlAdjustmentUnderline.Visible = false;
            pnlRestockingUnderline.Visible = false;
            pnlSupplierUnderline.Visible = false;
        }

        private void HideAllPanels()
        {
            pnlPriceList.Visible = false;
            pnlStockLevel.Visible = false;
            pnlAdjustment.Visible = false;
            pnlRestocking.Visible = false;
            pnlSupplier.Visible = false;
        }

        //load data into data grids
        public void DisplayPricelist(DataTable data) => priceListDataGrid.DataSource = data;
        public void DisplayStockLevel(DataTable data) => stockLevelDataGrid.DataSource = data;
        public void DisplayAdjustments(DataTable data)
        {
            adjustmentDataGrid.DataSource = data;
            adjustmentDataGrid.Columns["AdjustmentID"].Visible = false;
        }
        public void DisplayRestocking(DataTable data)
        {
            restockingDataGrid.DataSource = data;
            restockingDataGrid.Columns["RestockingID"].Visible = false;
        }
        public void DisplaySuppliers(DataTable data) => supplierDataGrid.DataSource = data;
    }
}
