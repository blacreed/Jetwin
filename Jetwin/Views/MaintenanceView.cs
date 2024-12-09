using Jetwin.Presenter;
using Jetwin.Presenter.Entities;
using Jetwin.Utility;
using Jetwin.Views.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Jetwin.Modules
{

    public partial class MaintenanceView : Form, IMaintenanceView
    {
        private readonly UserPresenter _userPresenter;
        private readonly ProductPresenter _productPresenter;
        private readonly AttributePresenter _attributePresenter;
        private readonly SupplierPresenter _supplierPresenter;
        private readonly InventoryPresenter _inventoryPresenter;

        bool _isProductLoaded = false;
        private DataTable _cachedProducts;
        private const int CB_SETCUEBANNER = 0x1703; //FOR COMBOBOX PLACEHOLDER

        public string SearchInput => tbSearch.Text.Trim();
        public int? editingId = null;

        //#startregion constructor
        public MaintenanceView()
        {
            InitializeComponent();

            _userPresenter = new UserPresenter(this);
            _productPresenter = new ProductPresenter(this);
            _attributePresenter = new AttributePresenter(this);
            _supplierPresenter = new SupplierPresenter(this);
            _inventoryPresenter = new InventoryPresenter(this);

            InitializeView();
        }
        //#endregion constructor
        //initialize all event handlers
        private void InitializeView()
        {
            TextBoxPlaceholder.SetPlaceholder(tbSearch, "Search keyword...");

            // Search functionality for all submodules
            tbSearch.TextChanged += (s, e) => PerformSearch();

            // User submodule
            btnSaveUser.Click += (s, e) => _userPresenter.SaveUser();

            CheckCancelVisibilityEvent();

            ConfigureSubmoduleButtons();
            ConfigureProductSubmodule();
            ConfigureOtherSubmodules();
            ConfigureEditAndArchiveButtons();
            ConfigureSupplierSubmodule();
            ConfigureInventorySubmodule();

            // Default submodule panel and active button
            ActivateSubmodule("User");
        }
        //#startregion Configuration
        private void ConfigureSubmoduleButtons() // Submodules Button click event handlers
        {
            btnUsers.Click += (s, e) => ActivateSubmodule("User");
            btnInventory.Click += (s, e) => ActivateSubmodule("Inventory");
            btnSupplier.Click += (s, e) => ActivateSubmodule("Supplier");
            btnAuditTrail.Click += (s, e) => ActivateSubmodule("Audit");
            btnBackup.Click += (s, e) => ActivateSubmodule("Backup");
        }
        private void ConfigureProductSubmodule()
        {
            //inside bntproduct.click _productPresenter.LoadProducts();
            btnProducts.Click += (s, e) => ActivateSubmodule("Product");
            rbProduct.CheckedChanged += (s, e) =>
            {
                if (rbProduct.Checked)
                {
                    ShowProductPanel();
                    _productPresenter.LoadProducts();
                    LoadComboboxesForProductPanel();
                }
            };

            rbCategory.CheckedChanged += (s, e) =>
            {
                if (rbCategory.Checked)
                {
                    ShowCategoryPanel();
                }
            };

            rbBrand.CheckedChanged += (s, e) =>
            {
                if (rbBrand.Checked)
                {
                    ShowBrandPanel();
                }
            };

            rbAttribute.CheckedChanged += (s, e) =>
            {
                if (rbAttribute.Checked)
                {
                    ShowAttributePanel();
                }
            };

            tbPrice.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                {
                    e.Handled = true;
                }
                else if (e.KeyChar == '.' && (s as TextBox).Text.IndexOf('.') > -1)
                {
                    e.Handled = true;
                }
            };

            btnCancelProduct.Click += (s, e) =>
            {
                ClearProductInputs();
                ShowProductPanel();
                btnCancelProduct.Visible = false;
            };

            // Product continue panel
            btnBackProduct.Click += (s, e) => ShowProductPanel();

            btnCancelProduct2.Click += (s, e) =>
            {
                ClearProductInputs();
                ShowProductPanel();
                btnCancelProduct.Visible = false;
                btnCancelProduct2.Visible = false;
            };

            cbAttributeType.SelectedIndexChanged += (s, e) =>
            {
                if (cbAttributeType.SelectedItem != null)
                { //POPULATE COMBOBOX
                    var attributeTypeID = (int)((DataRowView)cbAttributeType.SelectedItem)["AttributeTypeID"];
                    PopulateAttributeValues(_productPresenter.LoadAttributeValues(attributeTypeID));
                }
            };

            attributesDataGrid.Columns.Add("AttributeTypeID", "Attribute Type ID");
            attributesDataGrid.Columns.Add("AttributeTypeName", "Attribute Type");
            attributesDataGrid.Columns.Add("AttributeValueID", "Attribute Value ID");
            attributesDataGrid.Columns.Add("AttributeValueName", "Attribute Value");
            attributesDataGrid.Columns["AttributeTypeID"].Visible = false;
            attributesDataGrid.Columns["AttributeValueID"].Visible = false;
            btnAddAttribute.Click += (s, e) => AddAttributeToGrid();
            btnSaveProduct.Click += (s, e) => SaveProduct();
        }
        private void ConfigureOtherSubmodules()
        {
            tbUserContact.KeyPress += (s, e) =>
            {
                e.Handled = !char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar);
            };
            btnCancelUser.Click += (s, e) => ClearUserInputs();
            CheckCancelUserVisibility();

            btnCancelCtgyBd.Click += (s, e) => ClearCategoryBrandInput();
            CheckCancelCtgyBdVisibility();

            btnSaveCtgyBd.Click += (s, e) => SaveCategoryOrBrand();
            btnAddAttrType.Click += (s, e) => SaveAttributeType();
            btnAddAttrValue.Click += (s, e) => SaveAttributeValue();

            //if placeholder is active or if no attribute type is selected then disable the textbox attribute value
            cbSelectAttrType.SelectedIndexChanged += (s, e) => tbAddAttrValue.Enabled = cbSelectAttrType.SelectedItem != null;
        }
        //#endregion configuration

        //FOR COMBOBOX PLACEHOLDER, FROM STACKOVERFLOW
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string lParam);
        private void PerformSearch()
        {
            string input = SearchInput;
            if (UserPanel.Visible)
            {
                 _userPresenter.LoadUsers(input);
            }
            else if (ProductPanel.Visible)
            {
                _productPresenter.SearchProducts(input);
            }
            else if (BrandCategoryPanel.Visible)
            {
                if(rbCategory.Checked)
                    _productPresenter.SearchCategories(input);
                else if(rbBrand.Checked)
                     _productPresenter.SearchBrands(input);
            }
            else if (AttributePanel.Visible)
            {
                _attributePresenter.SearchAttributes(input);
            }
        }

        public void ShowValidationMessage(string message)
        {
            MessageBox.Show(message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        //#startregion Maintenance module
        private void HideAllPanels()
        {
            ProductPanel.Visible = false;
            ProductContinuePanel.Visible = false;
            BrandCategoryPanel.Visible = false;
            AttributePanel.Visible = false;
            UserPanel.Visible = false;
            InventoryPanel.Visible = false;
            SupplierPanel.Visible = false;
            AuditPanel.Visible = false;
            BackupPanel.Visible = false;
        }
        private void HideUnderlines() //reset all underlines
        {
            userUnderline.Visible = false;
            productUnderline.Visible = false;
            inventoryUnderline.Visible = false;
            supplierUnderline.Visible = false;
            auditUnderline.Visible = false;
            backupUnderline.Visible = false;
        }
        private void UpdateButtonStyles(string submodule)
        {
            //make button appear as active or inactive depending on panel opened
            btnUsers.ForeColor = submodule == "User" ? Color.White : Color.Gray;
            btnProducts.ForeColor = submodule == "Product" ? Color.White : Color.Gray;
            btnInventory.ForeColor = submodule == "Inventory" ? Color.White : Color.Gray;
            btnSupplier.ForeColor = submodule == "Supplier" ? Color.White : Color.Gray;
            btnAuditTrail.ForeColor = submodule == "Audit" ? Color.White : Color.Gray;
            btnBackup.ForeColor = submodule == "Backup" ? Color.White : Color.Gray;
        }
        private void ActivateSubmodule(string submodule)
        {
            HideAllPanels(); //reset panels
            HideUnderlines(); //reset underline of buttons

            pnlSelection.Visible = false; //hide radio button panel
            if(!pnlSubmoduleAdd.Visible) pnlSubmoduleAdd.Visible = true;

            //display panel, button underline, and load data on selected submodule
            switch (submodule)
            {
                case "User":
                    UserPanel.Visible = true;
                    userUnderline.Visible = true;
                    _userPresenter.LoadUsers();
                    break;
                case "Product":
                    ShowProductSubmodule();
                    break;
                case "Inventory":
                    InventoryPanel.Visible = true;
                    inventoryUnderline.Visible = true;
                    _inventoryPresenter.LoadInventory();
                    break;
                case "Supplier":
                    SupplierPanel.Visible = true;
                    supplierUnderline.Visible = true;
                    _supplierPresenter.LoadSuppliers();
                    break;
                case "Audit":
                    AuditPanel.Visible = false;
                    pnlSubmoduleAdd.Visible = false;
                    auditUnderline.Visible = true;
                    break;
                case "Backup":
                    BackupPanel.Visible = false;
                    pnlSubmoduleAdd.Visible = false;
                    backupUnderline.Visible = true;
                    break;
            }

            //reset radio buttons unless in the product submodule
            if (submodule != "Product")
            {
                rbProduct.Checked = false;
                rbCategory.Checked = false;
                rbBrand.Checked = false;
                rbAttribute.Checked = false;
            }

            UpdateButtonStyles(submodule);
        }
        //#endregion Maintenance module

        //#regionstart ProductPanel-specific methods
        private void LoadComboboxesForProductPanel() //comboboxes in add product panel
        {
            PopulateCategories(_productPresenter.LoadActiveCategories());
            PopulateBrands(_productPresenter.LoadActiveBrands());

            PopulateSuppliers(_supplierPresenter.LoadActiveSuppliers());
            PopulateAttributeTypes(_productPresenter.LoadAttributeTypes());
            _productPresenter.LoadUnitsOfMeasurement();
        }

        private void CheckCancelVisibilityEvent() //logic is if there is any input in product panel then show cancel button(cancel button will clear all input fields)
        {
            tbProductName.TextChanged += CheckCancelVisibility;
            cbCategory.SelectedIndexChanged += CheckCancelVisibility;
            cbBrand.SelectedIndexChanged += CheckCancelVisibility;
            cbSupplier.SelectedIndexChanged += CheckCancelVisibility;
            cbUoM.SelectedIndexChanged += CheckCancelVisibility;
            tbPrice.TextChanged += CheckCancelVisibility;

            btnCancelProduct.Click += (s, e) => ClearProductInputs();
            btnCancelProduct2.Click += (s, e) => ClearProductInputs();

            btnContinueProduct.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tbProductName.Text) || cbCategory.SelectedItem is null ||
                    cbBrand.SelectedItem is null || cbSupplier.SelectedItem is null || cbUoM.SelectedItem is null ||
                    string.IsNullOrWhiteSpace(tbPrice.Text))
                {
                    MessageBox.Show("All fields are mandatory.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else ShowProductContinuePanel();
            };
        }
        private void CheckCancelVisibility(object sender, EventArgs e)
        {
            bool anyFieldFilled = !string.IsNullOrWhiteSpace(tbProductName.Text) ||
                                  cbCategory.SelectedItem != null ||
                                  cbBrand.SelectedItem != null ||
                                  cbSupplier.SelectedItem != null ||
                                  cbUoM.SelectedItem != null ||
                                  !string.IsNullOrWhiteSpace(tbPrice.Text);

            btnCancelProduct.Visible = anyFieldFilled;
            btnCancelProduct2.Visible = anyFieldFilled;
        }
        //#regionend

        //#regionstart UserPanel-specific methods
        //properties
        public string EmpName => tbEmpName.Text.Trim();
        public string Username => tbUsername.Text.Trim();
        public string Password => tbPassword.Text;
        public string ConfirmPassword => tbConfirmPassword.Text;
        public string ContactNumber => tbUserContact.Text.Trim();

        public void DisplayUsers(DataTable users) //load user data into data grid
        {
            submoduleDataGrid.DataSource = users;
            submoduleDataGrid.Columns["UserID"].Visible = false;
            lblTotal.Text = $"Total Users: {users.Rows.Count}";
        }

        public void ClearUserInputs()
        {
            tbEmpName.Clear();
            tbUsername.Clear();
            tbPassword.Clear();
            tbConfirmPassword.Clear();
            tbUserContact.Clear();
            btnCancelUser.Visible = false;
            lblAddUser.Text = "Add a new user";
        }
        //#regionend
        //#regionstart Product-continue specific methods
        private void SaveProduct()
        {
            //validation checks
            if (string.IsNullOrWhiteSpace(tbProductName.Text) || cbCategory.SelectedItem is null ||
                cbBrand.SelectedItem is null || cbSupplier.SelectedItem is null ||
                cbUoM.SelectedItem is null || string.IsNullOrWhiteSpace(tbPrice.Text))
            {
                ShowValidationMessage("All fields must be filled out.");
                return;
            }

            if (!decimal.TryParse(tbPrice.Text, out decimal price) || price <= 0)
            {
                ShowValidationMessage("Price must be a valid positive number.");
                return;
            }
            //take all values and set it as a single property of product (this particular product has these values)
            var product = new Product
            {
                ProductName = tbProductName.Text.Trim(),
                CategoryID = (int)cbCategory.SelectedValue,
                BrandID = (int)cbBrand.SelectedValue,
                SupplierID = (int)cbSupplier.SelectedValue,
                UnitPrice = price,
                UoMID = (int)cbUoM.SelectedValue
            };
            //take the attributes from the attribute data grid
            var attributes = new List<ProductAttribute>();
            foreach (DataGridViewRow row in attributesDataGrid.Rows)
            {
                attributes.Add(new ProductAttribute
                {
                    AttributeTypeID = (int)row.Cells["AttributeTypeID"].Value,
                    AttributeValueID = (int)row.Cells["AttributeValueID"].Value
                });
            }

            _productPresenter.SaveProduct(product, attributes); //insert product and attribute of product to database
            //reset panel and combobox
            ShowProductPanel();
            cbAttributeType.SelectedItem = null;
            cbAttributeValue.SelectedItem = null;
        }
        private void AddAttributeToGrid()
        {
            if (cbAttributeType.SelectedItem is null || cbAttributeValue.SelectedItem is null)
            {
                ShowValidationMessage("Please select both Attribute Type and Attribute Value.");
                return;
            }

            int attributeTypeID = (int)((DataRowView)cbAttributeType.SelectedItem)["AttributeTypeID"];
            int attributeValueID = (int)((DataRowView)cbAttributeValue.SelectedItem)["AttributeValueID"];

            foreach (DataGridViewRow row in attributesDataGrid.Rows)
            {
                if ((int)row.Cells["AttributeTypeID"].Value == attributeTypeID ||
                    (int)row.Cells["AttributeValueID"].Value == attributeValueID)
                {
                    ShowValidationMessage("This attribute has already been added.");
                    return;
                }
            }

            attributesDataGrid.Rows.Add(
                attributeTypeID,
                ((DataRowView)cbAttributeType.SelectedItem)["AttributeTypeName"].ToString(),
                attributeValueID,
                ((DataRowView)cbAttributeValue.SelectedItem)["AttributeValueName"].ToString());
        }
        //#regionend
        //#regionstart Product submodule> load data into data grid
        public void DisplayProducts(DataTable products)
        {
            submoduleDataGrid.DataSource = null;
            submoduleDataGrid.DataSource = products;
            submoduleDataGrid.Columns["CategoryID"].Visible = false;
            submoduleDataGrid.Columns["BrandID"].Visible = false;
            submoduleDataGrid.Columns["SupplierID"].Visible = false;
            submoduleDataGrid.Columns["UoMID"].Visible = false;
            lblTotal.Text = $"Total Products: {products.Rows.Count}";
        }

        public void DisplayCategories(DataTable categories) //FILL SUBMODULE DATA GRID
        {
            submoduleDataGrid.DataSource = categories;
            submoduleDataGrid.Columns["Category ID"].Visible = false;
            lblTotal.Text = $"Total Categories: {categories.Rows.Count}";
        }
        public void DisplayBrands(DataTable brands)
        {
            submoduleDataGrid.DataSource = brands;
            submoduleDataGrid.Columns["Brand ID"].Visible = false;
            lblTotal.Text = $"Total Brands: {brands.Rows.Count}";
        }
        public void DisplayAttributes(DataTable attributeType)
        {
            submoduleDataGrid.DataSource = attributeType;
            lblTotal.Text = $"Total Attributes: {attributeType.Rows.Count}";
        }

        //PRODUCT PANEL load data into comboboxes
        private void PopulateComboBox(ComboBox comboBox, DataTable data, string displayMember, string valueMember, string placeholder)
        {
            SendMessage(comboBox.Handle, CB_SETCUEBANNER, 0, placeholder);
            comboBox.DataSource = data;
            comboBox.DisplayMember = displayMember;
            comboBox.ValueMember = valueMember;
            comboBox.SelectedIndex = -1;
        }
        public void PopulateCategories(DataTable categories) => PopulateComboBox(cbCategory, categories, "Category Name", "Category ID", "--Select a category--");

        public void PopulateBrands(DataTable brands) => PopulateComboBox(cbBrand, brands, "Brand Name", "Brand ID", "--Select a brand--");

        public void PopulateSuppliers(DataTable suppliers) => PopulateComboBox(cbSupplier, suppliers, "Supplier", "SupplierID", "--Select a supplier--");

        public void PopulateUnitsOfMeasurement(DataTable units) => PopulateComboBox(cbUoM, units, "UoMName", "UoMID", "--Select unit--");

        public void PopulateAttributeTypes(DataTable attributeTypes)
        {
            PopulateComboBox(cbAttributeType, attributeTypes, "AttributeTypeName", "AttributeTypeID", "--Select an attribute type--");
            PopulateComboBox(cbSelectAttrType, attributeTypes, "AttributeTypeName", "AttributeTypeID", "--Select an attribute type--");
        }

        public void PopulateAttributeValues(DataTable attributeValues)
        {
            PopulateComboBox(cbAttributeValue, attributeValues, "AttributeValueName", "AttributeValueID", "--Select an attribute value--");
            cbAttributeValue.Enabled = true;
        }
        
        public void ClearProductInputs()
        {
            //set to default state
            tbProductName.Clear();
            cbCategory.SelectedItem = null;
            cbBrand.SelectedItem = null;
            cbSupplier.SelectedItem = null;
            cbUoM.SelectedItem = null;
            tbPrice.Clear();
            attributesDataGrid.Rows.Clear();
            btnCancelProduct.Visible = false;
            btnCancelProduct2.Visible = false;
            lblAddProduct.Text = "Add a new product";
            lblAddProduct2.Text = "Add a new product";
        }
        //#regionend
        // brand, category, attribute submodule
        public void ClearCategoryBrandInput()
        {
            tbCtgyBd.Clear(); // Clear the textbox for category/brand input
            lblAddCtgyBd.Text = rbCategory.Checked ? "Add a new category" : "Add a new brand";
        }
        private void ShowProductContinuePanel()
        {
            HideAllPanels();
            ProductContinuePanel.Visible = true;
            rbProduct.Checked = false; //prevent accidental radio trigger
        }

        private void ShowProductPanel()
        {
            HideAllPanels();
            ProductPanel.Visible = true;
            rbProduct.Checked = true;
        }
        private void ShowProductSubmodule()
        {
            if (!_isProductLoaded) //avoid redundant loading (optimization purposes)
            {
                var products = _productPresenter.LoadProducts();
                DisplayProducts(products);

                LoadComboboxesForProductPanel();
                _isProductLoaded = true; //set the flag
            }

            ProductPanel.Visible = true;
            rbProduct.Checked = true; //synchronize radio button state
            pnlSelection.Visible = true;
            productUnderline.Visible = true;
        }

        private void ShowCategoryPanel()
        {
            HideAllPanels();
            BrandCategoryPanel.Visible = true;
            submoduleDataGrid.DataSource = _productPresenter.LoadCategories();
            lblAddCtgyBd.Text = "Add a new category";
            CtgyBdTextboxLabel.Text = "Category Name";
        }

        private void ShowBrandPanel()
        {
            HideAllPanels();
            BrandCategoryPanel.Visible = true;
            submoduleDataGrid.DataSource = _productPresenter.LoadBrands();
            lblAddCtgyBd.Text = "Add a new brand";
            CtgyBdTextboxLabel.Text = "Brand Name";
        }

        private void ShowAttributePanel()
        {
            HideAllPanels();
            AttributePanel.Visible = true;
            submoduleDataGrid.DataSource = _attributePresenter.LoadAttributes();
        }

        //ADD METHODS
        private void SaveCategoryOrBrand()
        {
            string name = tbCtgyBd.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowValidationMessage("Name cannot be empty.");
                return;
            }
            if (rbCategory.Checked)
                _productPresenter.SaveCategory(name);
            else if (rbBrand.Checked)
                _productPresenter.SaveBrand(name);

            tbCtgyBd.Clear();
        }

        private void SaveAttributeType()
        {
            string attrType = tbAddAttrType.Text.Trim();
            if (string.IsNullOrWhiteSpace(attrType) || !Regex.IsMatch(attrType, @"^[A-Za-z\s]+$"))
            {
                ShowValidationMessage("Attribute type must only contain letters and spaces.");
                return;
            }

            _attributePresenter.SaveAttributeType(attrType);
            tbAddAttrType.Clear();
            PopulateAttributeTypes(_productPresenter.LoadAttributeTypes());
        }

        private void SaveAttributeValue()
        {
            string attrValue = tbAddAttrValue.Text.Trim();
            if (string.IsNullOrWhiteSpace(attrValue) || !Regex.IsMatch(attrValue, @"^[A-Za-z0-9\s]+$"))
            {
                ShowValidationMessage("Attribute value must only contain letters, numbers, and spaces.");
                return;
            }

            if (cbSelectAttrType.SelectedItem is null)
            {
                ShowValidationMessage("Please select an attribute type.");
                return;
            }

            int attrTypeID = (int)cbSelectAttrType.SelectedValue;
            _attributePresenter.SaveAttributeValue(attrTypeID, attrValue);
            tbAddAttrValue.Clear();
            cbSelectAttrType.SelectedItem = null;
        }
            
        //#regionstart Edit and Archive method
        private void ConfigureEditAndArchiveButtons()
        {
            btnEdit.Click += (s, e) => EditSelectedRow();
            btnArchive.Click += (s, e) => ArchiveSelectedRow();
        }

        private void EditSelectedRow()
        {
            if (submoduleDataGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to edit.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedRow = submoduleDataGrid.SelectedRows[0];

            if (UserPanel.Visible)
            {
                editingId = (int)selectedRow.Cells["UserID"].Value;
                PopulateUserFields(selectedRow);

                tbPassword.Enabled = false;
                tbConfirmPassword.Enabled = false;

                lblAddUser.Text = "Edit User";
                btnSaveUser.Text = "Save";
                btnCancelUser.Visible = true;
            }
            else if (ProductPanel.Visible)
            {
                editingId = (int)selectedRow.Cells["Product Code"].Value;
                PopulateProductFields(selectedRow);
                lblAddProduct.Text = "Edit Product";
                lblAddProduct2.Text = "Edit Product";
                btnSaveProduct.Text = "Save";
            }
            else if (BrandCategoryPanel.Visible)
            {
                PopulateCategoryBrandFields(selectedRow);
                if(rbCategory.Checked)
                {
                    editingId = (int)selectedRow.Cells["Category ID"].Value;
                }
                else if(rbBrand.Checked)
                {
                    editingId = (int)selectedRow.Cells["Brand ID"].Value;
                }
                lblAddCtgyBd.Text = rbCategory.Checked ? "Edit Category" : "Edit Brand";
                btnSaveCtgyBd.Text = "Save";
            }
            else if (InventoryPanel.Visible)
            {
                editingId = (int)selectedRow.Cells["Inventory ID"].Value;
            }
            else
            {
                MessageBox.Show("Editing is not supported for this submodule.", "Unsupported", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ArchiveSelectedRow()
        {
            if (submoduleDataGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to archive.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if(AttributePanel.Visible || InventoryPanel.Visible)
            {
                MessageBox.Show("Archiving is not supported for this submodule.", "Unsupported", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var selectedRow = submoduleDataGrid.SelectedRows[0];
            string currentStatus = selectedRow.Cells["Status"].Value.ToString();
            int id;

            if (UserPanel.Visible)
            {   
                id = (int)selectedRow.Cells["UserID"].Value;
                if (currentStatus == "Archived")
                    _userPresenter.UnarchiveUser(id);
                else
                    _userPresenter.ArchiveUser(id);
            }
            else if (ProductPanel.Visible || ProductContinuePanel.Visible)
            {
                id = (int)selectedRow.Cells["Product Code"].Value;
                if (currentStatus == "Archived")
                    _productPresenter.UnarchiveProduct(id);
                else
                    _productPresenter.ArchiveProduct(id);
            }
            else if (BrandCategoryPanel.Visible)
            {
                if (rbCategory.Checked)
                {
                    id = (int)selectedRow.Cells["Category ID"].Value;
                    if (currentStatus == "Archived")
                        _productPresenter.UnarchiveCategory(id);
                    else
                        _productPresenter.ArchiveCategory(id);
                }
                    
                else
                {
                    id = (int)selectedRow.Cells["Brand ID"].Value;
                    if (currentStatus == "Archived")
                        _productPresenter.UnarchiveBrand(id);
                    else
                        _productPresenter.ArchiveBrand(id);
                }
                    
            }
            else if (SupplierPanel.Visible)
            {
                id = (int)selectedRow.Cells["ID"].Value;
            }
            else
            {
                MessageBox.Show("Archiving is not supported for this module.", "Unsupported", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            ReloadCurrentSubmoduleData();
        }

        //methods for populating fields for different submodules (for edit functionality)
        private void PopulateUserFields(DataGridViewRow row)
        {
            tbEmpName.Text = row.Cells["Employee Name"].Value.ToString();
            tbUsername.Text = row.Cells["Username"].Value.ToString();
            tbUserContact.Text = row.Cells["Contact Number"].Value?.ToString() ?? string.Empty;
        }

        private void PopulateProductFields(DataGridViewRow row)
        {
            tbProductName.Text = row.Cells["Product Name"].Value.ToString();
            cbCategory.SelectedValue = row.Cells["CategoryID"].Value;
            cbBrand.SelectedValue = row.Cells["BrandID"].Value;
            cbSupplier.SelectedValue = row.Cells["SupplierID"].Value;
            cbUoM.SelectedValue = row.Cells["UoMID"].Value;
            tbPrice.Text = row.Cells["Unit Price"].Value.ToString();
        }

        private void PopulateCategoryBrandFields(DataGridViewRow row)
        {
            if (rbBrand.Checked)
            {
                tbCtgyBd.Text = row.Cells["Brand Name"].Value.ToString();
            }
            else if (rbCategory.Checked)
            {
                tbCtgyBd.Text = row.Cells["Category Name"].Value.ToString();
            }
            else
            {
                MessageBox.Show("Invalid column name");
            }
                
        }
        //RELOAD FIX BUG
        private void ReloadCurrentSubmoduleData()
        {
            if (UserPanel.Visible)
            {
                _userPresenter.LoadUsers();
            }
            else if (ProductPanel.Visible)
            {
                _productPresenter.LoadProducts();
            }
            else if (BrandCategoryPanel.Visible)
            {
                if (rbCategory.Checked)
                    _productPresenter.LoadCategories();
                else
                    _productPresenter.LoadBrands();
            }
        }
        public void ResetEditState() //reset labels and button texts
        {
            editingId = null;
            if (UserPanel.Visible)
            {
                tbPassword.Enabled = true;
                tbConfirmPassword.Enabled = true;

                lblAddUser.Text = "Add a new user";
                btnSaveUser.Text = "Add";
            }
            else if (ProductPanel.Visible)
            {
                lblAddProduct.Text = "Add a new product";
                lblAddProduct2.Text = "Add a new product";
                btnSaveProduct.Text = "Add";
            }
            else if(BrandCategoryPanel.Visible)
            {
                btnSaveCtgyBd.Text = "Add";
                lblAddCtgyBd.Text = rbCategory.Checked ? "Add a new category" : "Add a new brand";
            }
        }

        //#regionstart supplier submodule
        public string SupplierName => tbSupplier.Text.Trim();
        public string AgentName => tbContactPerson.Text.Trim();
        public string SupplierContact => tbSupplierContactNum.Text.Trim();
        public string Remarks => tbRemarks.Text.Trim();

        public void DisplaySuppliers(DataTable suppliers) //load data into data grid
        {
            submoduleDataGrid.DataSource = suppliers;
            submoduleDataGrid.Columns["ID"].Visible = false;
            lblTotal.Text = $"Total Suppliers: {suppliers.Rows.Count}";
        }

        private void ConfigureSupplierSubmodule() //set event handlers
        {
            btnSaveSupplier.Click += (s, e) => _supplierPresenter.SaveSupplier();
            btnCancelSupplier.Click += (s, e) => ClearSupplierInputs();

            btnEdit.Click += (s, e) =>
            {
                if (SupplierPanel.Visible)
                {
                    EditSelectedSupplier();
                }
            };
            btnArchive.Click += (s, e) => {
                if (SupplierPanel.Visible)
                {
                    ArchiveSelectedSupplier();
                }
            };

            tbSearch.TextChanged += (s, e) =>
            {
                if (SupplierPanel.Visible)
                {
                    _supplierPresenter.LoadSuppliers(tbSearch.Text);
                }
            };

            CheckCancelSupplierVisibility();
        }

        private void EditSelectedSupplier()
        {
            if (submoduleDataGrid.SelectedRows.Count == 0)
            {
                ShowValidationMessage("Please select a supplier to edit.");
                return;
            }

            var row = submoduleDataGrid.SelectedRows[0];
            editingId = (int)row.Cells["ID"].Value;

            tbSupplier.Text = row.Cells["Supplier"].Value.ToString();
            tbContactPerson.Text = row.Cells["Agent"].Value.ToString();
            tbSupplierContactNum.Text = row.Cells["Contact Number"].Value.ToString();
            tbRemarks.Text = row.Cells["Remarks"].Value?.ToString() ?? string.Empty;

            lblSupplier.Text = "Edit Supplier";
            btnSaveSupplier.Text = "Save";
            btnCancelSupplier.Visible = true;
        }

        private void ArchiveSelectedSupplier()
        {
            if (submoduleDataGrid.SelectedRows.Count == 0)
            {
                ShowValidationMessage("Please select a supplier to archive/unarchive.");
                return;
            }

            var row = submoduleDataGrid.SelectedRows[0];
            int supplierId = (int)row.Cells["ID"].Value;
            string status = row.Cells["Status"].Value.ToString();

            _supplierPresenter.ArchiveSupplier(supplierId, status);
        }

        public void ClearSupplierInputs()
        {
            tbSupplier.Clear();
            tbContactPerson.Clear();
            tbSupplierContactNum.Clear();
            tbRemarks.Clear();

            lblSupplier.Text = "Add a new supplier";
            btnSaveSupplier.Text = "Add";
            editingId = null;
            btnCancelSupplier.Visible = false;
        }

        private void CheckCancelSupplierVisibility()
        {
            EventHandler visibilityHandler = (s, e) =>
            {
                bool visible = !string.IsNullOrWhiteSpace(tbSupplier.Text) ||
                               !string.IsNullOrWhiteSpace(tbContactPerson.Text) ||
                               !string.IsNullOrWhiteSpace(tbSupplierContactNum.Text) ||
                               !string.IsNullOrWhiteSpace(tbRemarks.Text);
                btnCancelSupplier.Visible = visible;
            };

            tbSupplier.TextChanged += visibilityHandler;
            tbContactPerson.TextChanged += visibilityHandler;
            tbSupplierContactNum.TextChanged += visibilityHandler;
            tbRemarks.TextChanged += visibilityHandler;
        }
        //#endregion
        //same shit for user and category/brand submodule
        private void CheckCancelUserVisibility()
        {
            EventHandler userVisibilityHandler = (s, e) =>
            {
                bool visible = !string.IsNullOrWhiteSpace(tbUsername.Text) ||
                               !string.IsNullOrWhiteSpace(tbEmpName.Text) ||
                               !string.IsNullOrWhiteSpace(tbUserContact.Text);
                btnCancelUser.Visible = visible;
            };
            tbUsername.TextChanged += userVisibilityHandler;
            tbEmpName.TextChanged += userVisibilityHandler;
            tbUserContact.TextChanged += userVisibilityHandler;
        }
        private void CheckCancelCtgyBdVisibility()
        {
            EventHandler ctgyBdVisibilityHandler = (s, e) =>
            {
                bool visible = !string.IsNullOrWhiteSpace(tbCtgyBd.Text);
                btnCancelCtgyBd.Visible = visible;
            };
            tbCtgyBd.TextChanged += ctgyBdVisibilityHandler;
        }
        //
        //#regionstart Inventory Submodule
        public string Quantity => tbQuantity.Text.Trim();
        public string MinimumStock => tbMinimumStock.Text.Trim();
        public string MaximumStock => tbMaximumStock.Text.Trim();
        public string ReorderPoint => tbReorderPoint.Text.Trim();
        public string Location => tbLocation.Text.Trim();
        public string SelectedProduct => tbInvProductCode.Text.Trim();
        public void DisplayInventory(DataTable inventory)
        {
            submoduleDataGrid.DataSource = inventory;

            submoduleDataGrid.Columns["Inventory ID"].Visible = false;
            foreach (DataGridViewRow row in submoduleDataGrid.Rows)
            {
                if (row.Cells["Current Stock"].Value == null)
                    row.Cells["Current Stock"].Value = "Not Set";

                if (row.Cells["Minimum Stock"].Value == null)
                    row.Cells["Minimum Stock"].Value = "Not Set";

                if (row.Cells["Maximum Stock"].Value == null)
                    row.Cells["Maximum Stock"].Value = "Not Set";

                if (row.Cells["Reorder Point"].Value == null)
                    row.Cells["Reorder Point"].Value = "Not Set";

                if (row.Cells["Location"].Value == null)
                    row.Cells["Location"].Value = "Not Set";
            }

            lblTotal.Text = $"Total Inventory Items: {inventory.Rows.Count}"; 
        }

        private void ConfigureInventorySubmodule()
        {
            btnSaveInventory.Click += (s, e) => _inventoryPresenter.SaveInventory();
            btnCancelInventory.Click += (s, e) => ClearInventoryInputs();

            tbSearch.TextChanged += (s, e) =>
            {
                if (InventoryPanel.Visible)
                {
                    _inventoryPresenter.LoadInventory(tbSearch.Text);
                }
            };

            submoduleDataGrid.RowEnter += SubmoduleDataGrid_RowEnter;
            ValidateNumericInputs();
        }
        private void SubmoduleDataGrid_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            if ((e.RowIndex >= 0 && e.RowIndex < submoduleDataGrid.Rows.Count) && InventoryPanel.Visible)
            {
                var selectedRow = submoduleDataGrid.Rows[e.RowIndex];
                tbInvProductCode.Text = selectedRow.Cells["Product Code"].Value.ToString();
                tbQuantity.Text = selectedRow.Cells["Current Stock"].Value.ToString();
                tbMinimumStock.Text = selectedRow.Cells["Minimum Stock"].Value?.ToString() ?? string.Empty;
                tbMaximumStock.Text = selectedRow.Cells["Maximum Stock"].Value?.ToString() ?? string.Empty;
                tbReorderPoint.Text = selectedRow.Cells["Reorder Point"].Value?.ToString() ?? string.Empty;
                tbLocation.Text = selectedRow.Cells["Location"].Value?.ToString() ?? string.Empty;

                object inventoryIdValue = selectedRow.Cells["Inventory ID"].Value;
                editingId = inventoryIdValue != DBNull.Value && inventoryIdValue != null ? (int?)Convert.ToInt32(inventoryIdValue) : null;
            }
        }
        private void EditSelectedInventory()
        {
            if (submoduleDataGrid.SelectedRows.Count == 0)
            {
                ShowValidationMessage("Please select an inventory item to edit.");
                return;
            }

            var row = submoduleDataGrid.SelectedRows[0];

            tbQuantity.Text = row.Cells["Current Stock"].Value.ToString();
            tbMinimumStock.Text = row.Cells["Minimum Stock"].Value?.ToString() ?? string.Empty;
            tbMaximumStock.Text = row.Cells["Maximum Stock"].Value?.ToString() ?? string.Empty;
            tbReorderPoint.Text = row.Cells["Reorder Point"].Value?.ToString() ?? string.Empty;
            tbLocation.Text = row.Cells["Location"].Value?.ToString() ?? string.Empty;

            lblStatusInventory.Text = "Edit Inventory";
            btnSaveInventory.Text = "Save";
            btnCancelInventory.Visible = true;
        }


        public void ClearInventoryInputs()
        {
            tbInvProductCode.Clear(); // Clear product selection
            tbQuantity.Clear();
            tbMinimumStock.Clear();
            tbMaximumStock.Clear();
            tbReorderPoint.Clear();
            tbLocation.Clear();

            lblStatusInventory.Text = "Add an Inventory";
            btnSaveInventory.Text = "Add";
            editingId = null;
            btnCancelInventory.Visible = false;
        }

        private void ValidateNumericInputs()
        {
            KeyPressEventHandler keyPressHandler = (s, e) =>
            {
                e.Handled = !char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar);
            };

            tbQuantity.KeyPress += keyPressHandler;
            tbMinimumStock.KeyPress += keyPressHandler;
            tbMaximumStock.KeyPress += keyPressHandler;
            tbReorderPoint.KeyPress += keyPressHandler;
        }
    }
}
