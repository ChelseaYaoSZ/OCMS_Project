using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Npgsql;

namespace OCMS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NpgsqlConnection con;
        private readonly AuthenticationForLogin authLogin;

        public MainWindow()
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();
            authLogin = new AuthenticationForLogin(dbHelper);
        }

        private void login_Click(object sender, RoutedEventArgs e)
        {
            string username = userName.Text;
            string userPW = password.Password;
            string selectedUserType = ((ComboBoxItem)userType.SelectedItem)?.Content.ToString();

            if (authLogin.AuthenticateUser(username, userPW, selectedUserType))
            {
                MessageBox.Show("Login successful!");

                // Create an instance of the Dashboard
                Dashboard dashboard = new Dashboard();
                dashboard.UpdateDashboardButtons(selectedUserType);
                dashboard.Show();
                this.Close();
            }
            else
            {
                // Authentication failed
                MessageBox.Show("Invalid credentials. Please try again.");
            }
        }

    }
}



