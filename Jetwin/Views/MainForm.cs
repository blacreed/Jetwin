using Jetwin.Utility;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Jetwin.Modules
{
    public partial class MainForm : Form
    {
        private readonly IServiceProvider serviceProvider;
        private readonly Dictionary<Type, Form> openForms = new Dictionary<Type, Form>();
        private Button activeButton;

        private readonly Color defaultButtonColor = Color.FromArgb(17, 21, 24);
        private readonly Color activeButtonColor = Color.FromArgb(40, 48, 57);
        public MainForm(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            InitializeDateTimeDisplay();
            InitializeRolePermissions();
            this.serviceProvider = serviceProvider;
            //REMOVE 3D BORDER AROUND MDI CLIENET
            this.SetBevel(false);
        }
        //#regionstart timer for current date and time
        private void InitializeDateTimeDisplay()
        {
            Timer timer = new Timer
            {
                Interval = 1000 //every second
            };
            timer.Tick += Timer_Tick;
            timer.Start();

            UpdateDateTime();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDateTime();
        }

        private void UpdateDateTime()
        {
            lblDateAndTime.Text = DateTime.Now.ToString("hh:mm:ss tt dddd MM/dd/yyyy");
        }
        //#endregion
        
        //#regionstart open modules using factory design pattern
        private void OpenForm<T>(Button btn) where T : Form
        {
            SetActiveButton(btn);

            //creates a form(if doesn't exist) and shows it on button click (prevents multiple instance of form)
            if (!openForms.ContainsKey(typeof(T)) || openForms[typeof(T)] == null)
            {
                var form = GetFormInstance<T>();
                form.MdiParent = this;
                form.TopLevel = false;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Dock = DockStyle.Fill;

                form.FormClosed += (s, args) => openForms[typeof(T)] = null;

                openForms[typeof(T)] = form;

                form.BringToFront();
                form.Show();
            }
            else
            {
                openForms[typeof(T)].BringToFront();
                openForms[typeof(T)].Activate();
            }

            if (typeof(T) == typeof(SalesView))
            {
                var salesView = (SalesView)openForms[typeof(T)];
                salesView.ResetToDefault();
            }
        }

        private T GetFormInstance<T>() where T : Form //dependency injection
        {
            return serviceProvider.GetRequiredService<T>();
        }
        //#endregion
        private void SetActiveButton(Button btn)
        {
            if (activeButton != null)
            {
                activeButton.BackColor = defaultButtonColor;
            }
            activeButton = btn;
            activeButton.BackColor = activeButtonColor;
        }

        //#regionstart open modules on button click
        private void btnDashboard_Click(object sender, EventArgs e) => OpenForm<DashboardView>((Button)sender);

        private void btnSales_Click(object sender, EventArgs e) => OpenForm<SalesView>((Button)sender);
        private void btnPurchaseOrder_Click(object sender, EventArgs e) => OpenForm<PurchaseOrderView>((Button)sender);

        private void btnInventory_Click(object sender, EventArgs e) => OpenForm<InventoryView>((Button)sender);

        private void btnReports_Click(object sender, EventArgs e) => OpenForm<ReportView>((Button)sender);

        private void btnMaintenance_Click(object sender, EventArgs e) => OpenForm<MaintenanceView>((Button)sender);
        private void btnProfile_Click(object sender, EventArgs e)
        {
            OpenForm<ProfileView>((Button)sender);
            ((Button)sender).BackColor = defaultButtonColor;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //set default module
            OpenForm<DashboardView>(btnDashboard);
        }

        private void InitializeRolePermissions() //hide maintenance module if user is not admin
        {
            // get user role if admin or staff
            var userRole = UserSession.Role;

            // hide maintenance button if user is not admin
            if (userRole != "admin")
            {
                btnMaintenance.Visible = false;
            }
        }
        //#startregion LOGOUT
        private void btnLogout_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to log out?", "Log Out", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Logout();
            }
        }
        private void Logout()
        {
            // reset the user session (set staff id and role to null as there is no user logged in)
            UserSession.ClearSession();

            // dispose dashboard module on logout (to fix bug because of memory leaks)
            if (openForms.ContainsKey(typeof(DashboardView)) && openForms[typeof(DashboardView)] != null)
            {
                openForms[typeof(DashboardView)].Dispose();
                openForms[typeof(DashboardView)] = null;
            }

            // close main form and return to login
            var loginForm = serviceProvider.GetRequiredService<frmLogin>();
            loginForm.Show();
            Close();
        }
        //#endregion

    }
}
