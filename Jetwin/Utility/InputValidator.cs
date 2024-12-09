using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Jetwin.Utility
{
    internal static class InputValidator
    {
        public static bool IsFieldFilled(string input, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show($"{fieldName} is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        //FOR USER
        public static bool ValidateUserInputs(
        string empName, string username, string password, string confirmPassword, string contactNumber,
        out string validationMessage)
        {
            // Validate employee name
            if (string.IsNullOrWhiteSpace(empName) || empName.Length > 50 || !Regex.IsMatch(empName, @"^[A-Za-z\s]+$"))
            {
                validationMessage = "Employee name must only contain letters and spaces, and be at most 50 characters.";
                return false;
            }

            // Validate username
            if (string.IsNullOrWhiteSpace(username) || username.Length < 5 || username.Length > 20 || !Regex.IsMatch(username, @"^[A-Za-z0-9]+$"))
            {
                validationMessage = "Username must be alphanumeric and 5-20 characters long.";
                return false;
            }

            // Validate password
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                validationMessage = "Password must be at least 8 characters long.";
                return false;
            }

            // Confirm password
            if (password != confirmPassword)
            {
                validationMessage = "Passwords do not match.";
                return false;
            }

            // Validate contact number
            if (!string.IsNullOrWhiteSpace(contactNumber) && (!Regex.IsMatch(contactNumber, @"^\d{9,15}$")))
            {
                validationMessage = "Contact number must be numeric and between 9 to 15 digits.";
                return false;
            }

            validationMessage = null;
            return true;
        }
        public static bool ValidateUserInputs(
        string empName, string username, string contactNumber,
        out string validationMessage)
        {
            // Validate employee name
            if (string.IsNullOrWhiteSpace(empName) || empName.Length > 50 || !Regex.IsMatch(empName, @"^[A-Za-z\s]+$"))
            {
                validationMessage = "Employee name must only contain letters and spaces, and be at most 50 characters.";
                return false;
            }

            // Validate username
            if (string.IsNullOrWhiteSpace(username) || username.Length < 5 || username.Length > 20 || !Regex.IsMatch(username, @"^[A-Za-z0-9]+$"))
            {
                validationMessage = "Username must be alphanumeric and 5-20 characters long.";
                return false;
            }

            // Validate contact number
            if (!string.IsNullOrWhiteSpace(contactNumber) && (!Regex.IsMatch(contactNumber, @"^\d{9,15}$")))
            {
                validationMessage = "Contact number must be numeric and between 9 to 15 digits.";
                return false;
            }

            validationMessage = null;
            return true;
        }
    }
}
