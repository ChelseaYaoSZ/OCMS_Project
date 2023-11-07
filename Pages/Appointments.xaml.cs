using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for Appointments.xaml
    /// </summary>
    public partial class Appointments : Window
    {
        private NpgsqlConnection con;
        private string _customerId;
        public Appointments()
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();
            LoadAllAppointments();
            this.Closed += AppointmentInfoPage_Closed;
        }

        public Appointments(string customerId)
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();
            _customerId = customerId;
            LoadSpecificAppointment();
        }

        public DataTable GetAllAppointments()
        {
            string query = @"SELECT a.appoint_id AS AppointmentID, 
                                    a.date AS AppointmentDate, 
                                    a.time AS AppointmentTime, 
                                    cust.first_name AS CustomerFirstName, 
                                    cust.last_name AS CustomerLastName, 
                                    doc.first_name AS DoctorFirstName, 
                                    doc.last_name AS DoctorLastName,
                                    cu.person_id AS CustomerPersonID
                            FROM optic.appointment a
                            JOIN optic.customer cu ON a.customer_id = cu.customer_id
                            JOIN optic.person cust ON cu.person_id = cust.person_id
                            JOIN optic.doctor d ON a.doctor_id = d.doctor_id
                            JOIN optic.staff s ON d.staff_id = s.staff_id
                            JOIN optic.person doc ON s.person_id = doc.person_id";

            NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(query, con);

            DataTable dataTable = new DataTable();

            dataAdapter.Fill(dataTable);

            return dataTable;
        }

        public void LoadAllAppointments()
        {
            DataTable appointments = GetAllAppointments();
            dataGridAppointments.ItemsSource = appointments.DefaultView;
        }

        public DataTable SearchAppointments(string searchTerm, DateTime? appointmentDate, int? personId)
        {
            string query = @"SELECT a.appoint_id AS AppointmentID, 
                                    a.date AS AppointmentDate, 
                                    a.time AS AppointmentTime, 
                                    cust.first_name AS CustomerFirstName, 
                                    cust.last_name AS CustomerLastName, 
                                    doc.first_name AS DoctorFirstName, 
                                    doc.last_name AS DoctorLastName,
                                    cu.person_id AS CustomerPersonID
                            FROM optic.appointment a
                            JOIN optic.customer cu ON a.customer_id = cu.customer_id
                            JOIN optic.person cust ON cu.person_id = cust.person_id
                            JOIN optic.doctor d ON a.doctor_id = d.doctor_id
                            JOIN optic.staff s ON d.staff_id = s.staff_id
                            JOIN optic.person doc ON s.person_id = doc.person_id";

            List<string> conditions = new List<string>();

            // Check if a search term was provided and add it to the conditions
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string searchCondition = @"(doc.first_name ILIKE @SearchTerm OR 
                                            cust.first_name ILIKE @SearchTerm OR 
                                            cust.last_name ILIKE @SearchTerm)";
                conditions.Add(searchCondition);
            }

            // Check if a date was provided and add it to the conditions
            if (appointmentDate.HasValue)
            {
                conditions.Add("a.date = @AppointmentDate");
            }

            // Check if a person ID was provided and add it to the conditions
            if (personId.HasValue)
            {
                conditions.Add("cu.person_id = @PersonID");
            }

            // Combine the conditions with OR or AND depending on your logic
            if (conditions.Count > 0)
            {
                query += " WHERE " + string.Join(" OR ", conditions); // Use OR or AND as needed
            }

             NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(query, con);

            // Add parameters if they are applicable
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                dataAdapter.SelectCommand.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
            }
            if (appointmentDate.HasValue)
            {
                dataAdapter.SelectCommand.Parameters.AddWithValue("@AppointmentDate", appointmentDate.Value);
            }
            if (personId.HasValue)
            {
                dataAdapter.SelectCommand.Parameters.AddWithValue("@PersonID", personId.Value);
            }

            DataTable dataTable = new DataTable();
            dataAdapter.Fill(dataTable);
            return dataTable;
        }

        public void LoadSpecificAppointment()
        {
            string searchTerm = "";
            DateTime? selectedDate = null;
            int? personId = null;

            if (!string.IsNullOrEmpty(_customerId))
            {
                personId = int.Parse(_customerId);

            } else { 
                searchTerm = doctor.Text;
                selectedDate = date.SelectedDate;
            }

            DataTable appointments = SearchAppointments(searchTerm, selectedDate, personId);
            dataGridAppointments.ItemsSource = appointments.DefaultView;
        }

        private void search_Click(object sender, RoutedEventArgs e)
        {
            LoadSpecificAppointment();
        }

        private void AppointmentInfoPage_Closed(object sender, EventArgs e)
        {
            if (con != null)
            {
                con.Close();
                con.Dispose();
            }
        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            string customerId = _customerId;
            AppointmentDetails appointmentDetails = new AppointmentDetails(customerId);
            appointmentDetails.Show();
        }

        private void DataGridAppointments_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var grid = (DataGrid)sender;
            var selectedAppointment = (DataRowView)grid.SelectedItem;
            if (selectedAppointment != null)
            {
                var appointmentDetails = new AppointmentDetails(selectedAppointment.Row);
                appointmentDetails.Show();
            }
        }

    }
}

