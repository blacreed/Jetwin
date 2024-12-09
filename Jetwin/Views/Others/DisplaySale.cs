using System;
using System.Data;
using System.Windows.Forms;

namespace Jetwin.Modules
{
    public partial class DisplaySale : Form
    {
        private readonly DataTable _transactionDetails;

        public DisplaySale(DataTable transactionDetails)
        {
            InitializeComponent();

            _transactionDetails = transactionDetails;

            Load += DisplaySaleForm_Load;
            btnExit.Click += (s, e) => this.Close();
        }

        private void DisplaySaleForm_Load(object sender, EventArgs e)
        {
            //populate the product list grid
            productListDataGrid.DataSource = _transactionDetails;

            //calculate and display summary values
            decimal subtotal = 0, discount = 0, total = 0;

            foreach (DataRow row in _transactionDetails.Rows)
            {
                subtotal += Convert.ToDecimal(row["Quantity"]) * Convert.ToDecimal(row["Unit Price"]);
                discount += Convert.ToDecimal(row["Discount"]);
                total += Convert.ToDecimal(row["Total Amount"]);
            }

            lblSubtotal.Text = subtotal.ToString("N2");
            lblDiscount.Text = discount.ToString("N2");
            lblTotal.Text = total.ToString("N2");
        }
    }
}
