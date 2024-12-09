using System.Data;

namespace Jetwin.Views.Interfaces
{
    public interface IMaintenanceView
    {
        //For users
        void DisplayUsers(DataTable users);
        void ClearUserInputs();
        void ShowValidationMessage(string message);
        

        string EmpName { get; }
        string Username { get; }
        string Password { get; }
        string ConfirmPassword { get; }
        string ContactNumber { get; }
        string SearchInput { get; }

        //For products
        void DisplayAttributes(DataTable products);
        void PopulateCategories(DataTable categories);
        void PopulateBrands(DataTable brands);
        void PopulateSuppliers(DataTable suppliers);
        void PopulateUnitsOfMeasurement(DataTable units);
        void PopulateAttributeTypes(DataTable attributeTypes);
        void PopulateAttributeValues(DataTable attributeValues);
        void ClearProductInputs();
        void DisplayCategories(DataTable dataTable);
        void DisplayBrands(DataTable dataTable);
        void DisplayProducts(DataTable products);
        void DisplaySuppliers(DataTable suppliers);

    }
}
