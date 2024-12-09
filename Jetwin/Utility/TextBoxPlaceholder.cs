using System.Drawing;
using System.Windows.Forms;


namespace Jetwin.Utility
{
    internal static class TextBoxPlaceholder
    {
        //#regionstart
        public static void SetPlaceholder(TextBox textBox, string placeholderText)
        {
            //determine if this is a password field
            bool isPasswordField = textBox.UseSystemPasswordChar;

            //initialize placeholder behavior
            void ShowPlaceholder()
            {
                textBox.UseSystemPasswordChar = false; // Disable password masking
                textBox.Text = placeholderText;
                textBox.ForeColor = Color.Gray;
            }

            void HidePlaceholder()
            {
                textBox.ForeColor = Color.White; //set default text color for user input
                if (isPasswordField)
                {
                    textBox.UseSystemPasswordChar = true; //re-enable password masking
                }
                textBox.Text = string.Empty;
            }

            //set initial state to placeholder
            ShowPlaceholder();

            //handle textBox gaining focus
            textBox.GotFocus += (sender, e) =>
            {
                if (textBox.Text == placeholderText)
                {
                    HidePlaceholder(); //remove placeholder on focus
                }
            };

            // Handle TextBox losing focus
            textBox.LostFocus += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    ShowPlaceholder(); //restore placeholder if input is empty
                }
            };

            //handle real-time text changes to ensure color consistency
            textBox.TextChanged += (sender, e) =>
            {
                //if placeholder is active, ensure text remains gray
                if (textBox.Text == placeholderText)
                {
                    textBox.ForeColor = Color.Gray;
                }
                else if (!string.IsNullOrWhiteSpace(textBox.Text))
                {
                    //ensure user input is always displayed in white
                    textBox.ForeColor = Color.White;
                }
            };
        }
        //#regionend
    }
}
