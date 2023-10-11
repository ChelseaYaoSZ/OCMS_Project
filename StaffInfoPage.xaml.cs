using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Npgsql;
using System.Data;
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
    /// Interaction logic for StaffInfoPage.xaml
    /// </summary>
    public partial class StaffInfoPage : Window
    {
        private NpgsqlConnection con;

        public StaffInfoPage()
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();
            LoadAllStaffs();
            this.Closed += StaffInfoPage_Closed;
        }

        public DataTable GetAllStaffs()
        {
            string query = @"SELECT p.first_name, p.last_name, p.birth_date, p.phone, p.email, a.address ,s.user_type, s.active
                            FROM optic.staff s
                            LEFT JOIN optic.person p on p.person_id = s.staff_id
                            LEFT JOIN optic.address a on a.address_id = s.address_id";

            NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(query, con);

            DataTable dataTable = new DataTable();

            dataAdapter.Fill(dataTable);

            return dataTable;
        }

        public void LoadAllStaffs()
        {
            DataTable customers = GetAllStaffs();
            dataGridStuffs.ItemsSource = customers.DefaultView;
        }

        public DataTable SearchCustomers(string searchTerm)
        {
            string query = @"SELECT p.first_name, p.last_name, p.birth_date, p.phone, p.email, a.address ,s.user_type, s.active
                            FROM optic.staff s
                            LEFT JOIN optic.person p on p.person_id = s.staff_id
                            LEFT JOIN optic.address a on a.address_id = s.address_id 
                            WHERE p.first_name ILIKE @SearchTerm 
                             OR p.phone ILIKE @SearchTerm 
                             OR p.email ILIKE @SearchTerm";

            NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(query, con);
            dataAdapter.SelectCommand.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
            DataTable dataTable = new DataTable();
            dataAdapter.Fill(dataTable);
            return dataTable;
        }

        public void LoadSpecificStaff()
        {
            string searchTerm = searchInfo.Text;
            DataTable specificCustomer = SearchCustomers(searchTerm);
            dataGridStuffs.ItemsSource = specificCustomer.DefaultView;
        }

        private void search_Click(object sender, RoutedEventArgs e)
        {
            LoadSpecificStaff();
        }

        private void StaffInfoPage_Closed(object sender, EventArgs e)
        {
            if (con != null)
            {
                con.Close();
                con.Dispose();
            }
        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            EmployeeDetails employeeDetails = new EmployeeDetails();
            employeeDetails.Show();
        }
    }
}
