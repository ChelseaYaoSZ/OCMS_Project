using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
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
    /// Interaction logic for AppointmentDetails.xaml
    /// </summary>
    public partial class AppointmentDetails : Window
    {
        private NpgsqlConnection con;
        private NpgsqlCommand cmd;
        DateTime? selectedDate;

        int generatedAppointmentId;
        private string _custId;

        public AppointmentDetails(string custID)
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();

            //Set the doctors combobox
            doctor.DataContext = FetchDoctors();
            doctor.DisplayMemberPath = "Name";
            doctor.SelectedValuePath = "DoctorId";

            // Set the stores combobox
            store.ItemsSource = FetchStores();
            store.DisplayMemberPath = "StoreName";
            store.SelectedValuePath = "StoreId";

            _custId = custID;
        }

        public AppointmentDetails(DataRow selectedRow)
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();
            FillAppointmentDetails(selectedRow);

            doctor.DataContext = FetchDoctors();
            doctor.DisplayMemberPath = "Name";
            doctor.SelectedValuePath = "DoctorId";

            // Set the stores combobox
            store.DataContext = FetchStores();
            store.DisplayMemberPath = "StoreName";
            store.SelectedValuePath = "StoreId";
        }

        private void FillAppointmentDetails(DataRow row)
        {
            appointID.Text = row.Table.Columns.Contains("AppointmentID") ? row["AppointmentID"].ToString() : string.Empty;
            date.Text = row.Table.Columns.Contains("AppointmentDate") ? row["AppointmentDate"].ToString() : string.Empty;
            time.Text = row.Table.Columns.Contains("AppointmentTime") ? row["AppointmentTime"].ToString() : string.Empty;
            fee.Text = row.Table.Columns.Contains("EyeExamFee") ? row["EyeExamFee"].ToString() : string.Empty;
            //store.SelectedItem = row.Table.Columns.Contains("StoreId") ? row["StoreId"].ToString() : null;
            if (row.Table.Columns.Contains("StoreId") && row["StoreId"] != DBNull.Value)
            {
                int storeId = Convert.ToInt32(row["StoreId"]);
                store.SelectedValue = storeId;
            }

            if (row.Table.Columns.Contains("DoctorId") && row["DoctorId"] != DBNull.Value)
            {
                int doctorId = Convert.ToInt32(row["DoctorId"]);
                doctor.SelectedValue = doctorId;
            }

            custID.Text = row.Table.Columns.Contains("CustomerCustomerID") ? row["CustomerCustomerID"].ToString() : string.Empty;
            lastName.Text = row.Table.Columns.Contains("CustomerLastName") ? row["CustomerLastName"].ToString() : string.Empty;
            firstName.Text = row.Table.Columns.Contains("CustomerFirstName") ? row["CustomerFirstName"].ToString() : string.Empty;
        }

        public class DoctorInfo
        {
            public int DoctorId { get; set; }
            public string Name { get; set; }
        }

        public class StoreInfo
        {
            public int StoreId { get; set; }
            public string StoreName { get; set; }
        }

        private List<DoctorInfo> FetchDoctors()
        {
            List<DoctorInfo> doctors = new List<DoctorInfo>();

            // SQL query to join the doctor, staff, and person tables
            string query = @"SELECT d.doctor_id, p.first_name, p.last_name 
                         FROM optic.doctor d 
                         JOIN optic.staff s ON d.staff_id = s.staff_id
                         JOIN optic.person p ON s.person_id = p.person_id";

            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            doctors.Add(new DoctorInfo
                            {
                                DoctorId = reader.GetInt32(0),
                                Name = reader.GetString(1) + " " + reader.GetString(2)
                            });
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"An error occurred while fetching doctors: {ex.Message}");
            }
            /*finally
            {
              con.Close();
            }*/

            doctor.ItemsSource = doctors;

            return doctors;
        }

        private List<StoreInfo> FetchStores()
        {
            List<StoreInfo> stores = new List<StoreInfo>();

            // SQL query to get the store_id and store_name from the store table
            string query = "SELECT store_id, name FROM optic.store";

            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stores.Add(new StoreInfo
                            {
                                StoreId = reader.GetInt32(0),
                                StoreName = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"An error occurred while fetching stores: {ex.Message}");
            }
            /* finally
             {
                 con.Close();
             }*/

            store.ItemsSource = stores;
            return stores;
        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            int selectedDoctorId = (int)doctor.SelectedValue;
            int selectedStoreId = (int)store.SelectedValue;

            using (NpgsqlTransaction transaction = con.BeginTransaction())
            {
                try
                {
                    selectedDate = date.SelectedDate;

                    if (selectedDate.HasValue)
                    {
                        // SQL query with parameters
                        string appointmentQuery = "INSERT INTO optic.appointment (date, time, eye_exam_fee, customer_id, store_id, doctor_id) " +
                                       " VALUES (@date, @time, @eye_exam_fee, @customer_id, @store_id, @doctor_id) " +
                                       " RETURNING appoint_id";

                        cmd = new NpgsqlCommand(appointmentQuery, con);
                        cmd.Transaction = transaction;

                        // Add values for the parameters in the query
                        cmd.Parameters.AddWithValue("@date", selectedDate);
                        cmd.Parameters.AddWithValue("@time", TimeSpan.Parse(time.Text));
                        cmd.Parameters.AddWithValue("@eye_exam_fee", Convert.ToDecimal(fee.Text));
                        cmd.Parameters.AddWithValue("@customer_id", int.Parse(_custId));
                        cmd.Parameters.AddWithValue("@store_id", selectedStoreId);
                        cmd.Parameters.AddWithValue("@doctor_id", selectedDoctorId);

                        // Execute the query and retrieve the generated appointment_id
                        generatedAppointmentId = (int)cmd.ExecuteScalar();

                        // Commit the transaction if everything succeeds
                        transaction.Commit();

                        // Set the generated IDs to the respective TextBoxes
                        appointID.Text = generatedAppointmentId.ToString();

                        MessageBox.Show("Appointment was added successfully!");
                    }
                    else
                    {
                        MessageBox.Show("Please select a valid date.");
                    }
                }
                catch (NpgsqlException ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"An error occured: {ex.Message}");
                }
            }
        }

        private void DeleteAppointment(int appointmentId)
        {
            string deleteQuery = "DELETE FROM optic.appointment WHERE appoint_id = @appointmentId";

            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(deleteQuery, con))
                {
                    // Add parameters to the SQL command
                    cmd.Parameters.AddWithValue("@appointmentId", appointmentId);

                    // Execute the command
                    int result = cmd.ExecuteNonQuery();

                    // Check if any row was affected
                    if (result > 0)
                    {
                        MessageBox.Show("Appointment deleted successfully.");
                        // Additional code to update the UI, if necessary

                        // Clear text boxes
                        appointID.Text = "";
                        time.Text = "";
                        fee.Text = "";

                        // Reset combo boxes
                        date.SelectedDate = null;
                        store.SelectedIndex = -1;
                        doctor.SelectedIndex = -1;
                    }
                    else
                    {
                        MessageBox.Show("No appointment was found with the given ID.");
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"An error occurred while deleting the appointment: {ex.Message}");
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            // Assuming appointID is a TextBox containing the appointment ID
            if (int.TryParse(appointID.Text, out int appointmentId))
            {
                DeleteAppointment(appointmentId);
            }
            else
            {
                MessageBox.Show("Please select a valid appointment to delete.");
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
