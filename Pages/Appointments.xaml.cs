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
        private string _firstName;
        private string _lastName;
        public Appointments()
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();
            LoadAllAppointments();
            this.Closed += AppointmentInfoPage_Closed;
        }

        public Appointments(string customerId, string firstName, string lastName)
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();
            _customerId = customerId;
            _firstName = firstName;
            _lastName = lastName;
            LoadSpecificAppointment();
            this.Closed += AppointmentInfoPage_Closed;
        }

        public DataTable GetAllAppointments()
        {
            string query = @"SELECT a.appoint_id AS AppointmentID, 
                               a.date AS AppointmentDate, 
                               a.time AS AppointmentTime,
                               a.eye_exam_fee as EyeExamFee,
                               cust.first_name AS CustomerFirstName, 
                               cust.last_name AS CustomerLastName, 
                               doc.first_name AS DoctorFirstName, 
                               doc.last_name AS DoctorLastName,
                               d.doctor_id AS DoctorId,
                               cu.person_id AS CustomerCustomerID,
                               store.store_id AS StoreId
                        FROM optic.appointment a
                        JOIN optic.customer cu ON a.customer_id = cu.customer_id
                        JOIN optic.person cust ON cu.person_id = cust.person_id
                        JOIN optic.doctor d ON a.doctor_id = d.doctor_id
                        JOIN optic.staff s ON d.staff_id = s.staff_id
                        JOIN optic.person doc ON s.person_id = doc.person_id
                        JOIN optic.store_staff ss ON s.staff_id = ss.staff_id
                        JOIN optic.store ON ss.store_id = store.store_id";
                           

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

        public DataTable SearchAppointments(string searchTerm, DateTime? appointmentDate, int? customerId)
        {
            string query = @"SELECT a.appoint_id AS AppointmentID, 
                                    a.date AS AppointmentDate, 
                                    a.time AS AppointmentTime, 
                                    a.eye_exam_fee as EyeExamFee,
                                    cust.first_name AS CustomerFirstName, 
                                    cust.last_name AS CustomerLastName, 
                                    doc.first_name AS DoctorFirstName, 
                                    doc.last_name AS DoctorLastName,
                                    d.doctor_id AS DoctorId,
                                    cu.customer_id AS CustomerCustomerID
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
            if (customerId.HasValue)
            {
                conditions.Add("cu.customer_id = @CustomerID");
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
            if (customerId.HasValue)
            {
                dataAdapter.SelectCommand.Parameters.AddWithValue("@CustomerID", customerId.Value);
            }

            DataTable dataTable = new DataTable();
            dataAdapter.Fill(dataTable);
            return dataTable;
        }

        public void LoadSpecificAppointment()
        {
            string searchTerm = "";
            DateTime? selectedDate = null;
            int? customerId = null;

            if (!string.IsNullOrEmpty(_customerId))
            {
                customerId = int.Parse(_customerId);

            } else { 
                searchTerm = doctor.Text;
                selectedDate = date.SelectedDate;
            }

            DataTable appointments = SearchAppointments(searchTerm, selectedDate, customerId);
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
            AppointmentDetails appointmentDetails = new AppointmentDetails(_customerId, _firstName, _lastName);
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

