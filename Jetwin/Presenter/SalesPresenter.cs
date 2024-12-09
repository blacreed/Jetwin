using Jetwin.Models.Interfaces;
using Jetwin.Views.Interfaces;
using Jetwin.Modules;
using System.Windows.Forms;

namespace Jetwin.Presenter
{
    public class SalesPresenter //SALES MODULE -> SALES VIEW
    {
        private readonly ISalesView _view;
        private readonly ISalesRepository _repository;

        public SalesPresenter(ISalesView view, ISalesRepository repository)
        {
            _view = view;
            _repository = repository;
        }

        //get transaction data for all transaction (regardless of status completed or returned)
        public void LoadAllTransactions()
        {
            var data = _repository.GetAllTransactions();
            _view.PopulateTransactionGrid(data);
            _view.SetNumberOfSales(data.Rows.Count);
            _view.HighlightActiveButton("btnAllTransact");
        }
        //get transaction data for status (completed or returned)
        public void LoadFilteredTransactions(string status)
        {
            var data = _repository.GetTransactionsByStatus(status);
            _view.PopulateTransactionGrid(data);
            _view.SetNumberOfSales(data.Rows.Count);
            _view.HighlightActiveButton(status);
        }
        //functionality for displaying detailed transaction on double click
        public void ShowTransactionDetails()
        {
            if (_view.SelectedTransactionID == 0) return;

            //get transaction details of the specific transaction (on row double click)
            var data = _repository.GetTransactionDetails(_view.SelectedTransactionID);
            //logic to open DisplaySale form and populate it
            var displayForm = new DisplaySale(data);
            displayForm.ShowDialog();
        }

        //display panel of new transaction
        public void ShowAddTransactionPanel(Button btnNew, Panel pnlView, Panel addView, Button btnBack)
        {
            btnNew.Visible = false;
            pnlView.Visible = false;
            addView.Visible = true;
            btnBack.Visible = true;
        }
        //display panel of viewing transaction (this is default panel)
        public void ShowViewTransactionPanel(Button btnNew, Panel pnlView, Panel addView, Button btnBack)
        {
            btnNew.Visible = true;
            pnlView.Visible = true;
            addView.Visible = false;
            btnBack.Visible = false;
        }
    }
}
