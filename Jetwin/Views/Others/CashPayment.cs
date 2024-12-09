using Jetwin.Presenter;
using System;
using System.Data;
using System.Windows.Forms;

namespace Jetwin.Views.Others
{
    public partial class CashPayment : Form
    {
        private readonly decimal _balance;
        private readonly DataTable _cartItems;
        private readonly NewTransactionPresenter _presenter;
        public CashPayment(decimal balance, NewTransactionPresenter presenter, DataTable cartItems)
        {
            InitializeComponent();

            _balance = balance;
            _cartItems = cartItems;
            _presenter = presenter;

            tbBalance.Text = balance.ToString("N2");

            btnConfirmPay.Click += (s, e) =>
            {
                if (decimal.TryParse(tbAmount.Text, out decimal amountPaid) && amountPaid >= _balance)
                {
                    _presenter.ConfirmTransaction(cartItems, "Cash", amountPaid);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Insufficient payment amount.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            tbAmount.TextChanged += UpdateChange;
            tbAmount.KeyPress += AllowNumbersOnly;
            btnExit.Click += (s, e) => this.Close();
        }
        private void UpdateChange(object sender, EventArgs e)
        {
            if (decimal.TryParse(tbAmount.Text, out decimal amountPaid))
            {
                var change = amountPaid - _balance;
                tbChange.Text = (change > 0 ? change : 0).ToString("N2");
            }
            else
            {
                tbChange.Text = "0.00";
            }
        }
        private void AllowNumbersOnly(object sender, KeyPressEventArgs e)
        {
            // Allow only digits and control characters
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

    }
}
