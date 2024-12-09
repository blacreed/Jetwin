using Jetwin.Models;
using Jetwin.Modules;
using Jetwin.Utility;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Jetwin
{
    public partial class frmLogin : Form
    {
        private readonly Login login;
        private readonly IServiceProvider serviceProvider;

        private readonly Color buttonColorOnHover = Color.FromArgb(151, 40, 90);
        private readonly Color defaultButtonColor = Color.FromArgb(31, 22, 25);
        public frmLogin(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            this.serviceProvider = serviceProvider;
            login = new Login();

            SetPlaceholders(); //textbox placeholders
            AcceptButton = btnLogin; //keyboard enter for login functionality
        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            if(isFieldsValid())
            {
                if(ExecuteLogin())
                    OpenMainForm();
            }   
        }

        private bool ExecuteLogin()
        {
            string username = tbUsername.Text;
            string password = tbPassword.Text;

            if (!login.ValidateCredentials(username, password))
            {
                MessageBox.Show("Invalid username or password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            // get user details from the database
            var userDetails = login.GetUserDetails(username); // returns staff id and role
            if (userDetails.StaffID == 0) //add role check if staff or admin
            {
                MessageBox.Show("Unable to fetch user details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // set session (sets the user that is currently logged in as)
            UserSession.SetUser(userDetails.StaffID, username, userDetails.Role);
            MessageBox.Show("Login successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            return true;
        }

        private bool isFieldsValid() =>
            InputValidator.IsFieldFilled(tbUsername.Text, "Username") &&
            InputValidator.IsFieldFilled(tbPassword.Text, "Password");

        private void SetPlaceholders()
        {
            TextBoxPlaceholder.SetPlaceholder(tbUsername, "Enter username..");
            TextBoxPlaceholder.SetPlaceholder(tbPassword, "Enter password..");
        }

        private void OpenMainForm()
        {
            var mainForm = Program.ServiceProvider.GetRequiredService<MainForm>();
            Hide();
            mainForm.Show();
        }

        //#region DRAGGABLE FORM
        private bool _dragging = false;
        private Point _offset;
        private Point _start_point = new Point(0, 0);

        private void pnlMenuBar_MouseDown(object sender, MouseEventArgs e)
        {
            _dragging = true;
            _start_point = new Point(e.X, e.Y);
        }

        private void pnlMenuBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - this._start_point.X, p.Y - this._start_point.Y);
            }
        }

        private void pnlMenuBar_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
        }

        private void lblApplicationExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lblApplicationExit_MouseHover(object sender, EventArgs e)
        {
            lblApplicationExit.BackColor = buttonColorOnHover;
        }

        private void lblApplicationExit_MouseLeave(object sender, EventArgs e)
        {
            lblApplicationExit.BackColor = defaultButtonColor;
        }
        //#endregion
    }
}
