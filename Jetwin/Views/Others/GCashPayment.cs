using Jetwin.Presenter;
using System;
using System.Data;
using System.Windows.Forms;

namespace Jetwin.Views.Others
{
    public partial class GCashPayment : Form
    {
        private readonly decimal _balance;
        private readonly DataTable _cartItems;
        private readonly NewTransactionPresenter _presenter;
        public GCashPayment(decimal balance, NewTransactionPresenter presenter, DataTable cartItems)
        {
            InitializeComponent();
            _balance = balance;
            _presenter = presenter;
            _cartItems = cartItems;

            tbBalance.Text = balance.ToString("N2");

            btnConfirmPay.Click += ConfirmPayment;
            btnExit.Click += (s, e) => this.Close();
            tbReferenceNum.KeyPress += AllowNumbersOnly;
            tbAmount.KeyPress += AllowNumbersOnly;
        }
        private void ConfirmPayment(object sender, EventArgs e)
        {
            if (!decimal.TryParse(tbAmount.Text, out decimal amountPaid) || amountPaid < _balance)
            {
                MessageBox.Show("Insufficient or invalid payment amount.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(tbReferenceNum.Text) || (tbReferenceNum.Text.Length != 9 && tbReferenceNum.Text.Length != 13))
            {
                MessageBox.Show("Invalid GCash reference number.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string paymentReference = tbReferenceNum.Text.Trim();
            if (!_presenter.IsReferenceNumberUnique(paymentReference)) return;
            try
            {
                _presenter.ConfirmTransaction(_cartItems, "Gcash", amountPaid, paymentReference);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to complete transaction: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void AllowNumbersOnly(object sender, KeyPressEventArgs e)
        {
            // Allow only digits, control characters, and decimal points
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}
