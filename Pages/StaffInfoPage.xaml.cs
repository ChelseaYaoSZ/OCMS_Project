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
            string query = @"SELECT a.address, a.city, a.postal_code, p.person_id, a.address_id,s.staff_id,
                                p.first_name, p.last_name, p.birth_date, p.phone, p.email, a.address_id,s.username, s.password, s.user_type, s.active
                                FROM optic.staff s
                                LEFT JOIN optic.person p on p.person_id = s.person_id
                                LEFT JOIN optic.address a on a.address_id = s.address_id";

            NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(query, con);

            DataTable dataTable = new DataTable();

            dataAdapter.Fill(dataTable);

            return dataTable;
        }

        public void LoadAllStaffs()
        {
            DataTable staffs = GetAllStaffs();
            dataGridStaffs.ItemsSource = staffs.DefaultView;
        }

        public DataTable SearchStaffs(string searchTerm)
        {
            string query = @"SELECT a.address, a.city, a.postal_code, p.person_id, a.address_id,s.staff_id,
                                p.first_name, p.last_name, p.birth_date, p.phone, p.email, a.address_id,s.username, s.password, s.user_type, s.active
                                FROM optic.staff s
                                LEFT JOIN optic.person p on p.person_id = s.person_id
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
            DataTable specificStaff = SearchStaffs(searchTerm);
            dataGridStaffs.ItemsSource = specificStaff.DefaultView;
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

        private void DataGridStaffs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var grid = (DataGrid)sender;
            var selectedStaff = (DataRowView)grid.SelectedItem;
            if (selectedStaff != null)
            {
                var staffDetails = new EmployeeDetails(selectedStaff.Row);
                staffDetails.Show();
            }
        }
    }
}
