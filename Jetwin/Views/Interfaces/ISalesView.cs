using System.Data;

namespace Jetwin.Views.Interfaces
{
    public interface ISalesView
    {
        void PopulateTransactionGrid(DataTable data);
        void SetNumberOfSales(int totalSales);
        string SelectedStatusFilter { get; }
        int SelectedTransactionID { get; } // For loading details
        void HighlightActiveButton(string buttonName);
    }
}
