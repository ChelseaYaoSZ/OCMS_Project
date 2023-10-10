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
using System.Windows.Shapes;

namespace OCMS
{
    /// <summary>
    /// Interaction logic for MainFunctionPage.xaml
    /// </summary>
    public partial class Dashboard : Window
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        public void UpdateDashboardButtons(string selectedUserType)
        {
            switch (selectedUserType)
            {
                case "administrator":
                    // All buttons enabled
                    staff.IsEnabled = true;
                    client.IsEnabled = true;
                    appointment.IsEnabled = true;
                    order.IsEnabled = true;
                    inventory.IsEnabled = true;
                    break;

                case "sales":
                    staff.IsEnabled = false; 
                    client.IsEnabled = true;
                    appointment.IsEnabled = true;
                    order.IsEnabled = true;
                    inventory.IsEnabled = true;
                    break;

                case "doctor":
                    staff.IsEnabled = false;
                    client.IsEnabled = true;
                    appointment.IsEnabled = true;
                    order.IsEnabled = false;
                    inventory.IsEnabled = false;
                    break;

                default:
                    break;
            }
        }

        private void client_Click(object sender, RoutedEventArgs e)
        {
            CustomerInfoPage customerInfoPage = new CustomerInfoPage();
            customerInfoPage.Show();
        }

        private void staff_Click(object sender, RoutedEventArgs e)
        {
            StaffInfoPage staffInfoPage = new StaffInfoPage();
            staffInfoPage.Show();
        }

        private void appointment_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Appointment");
        }

        private void order_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Order");
        }

        private void inventory_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Inventory");
        }
    }
}
