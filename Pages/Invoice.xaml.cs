using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for Invoice.xaml
    /// </summary>
    public partial class Invoice : Window
    {
        private readonly int? custID;
        private static int currentInventoryId = -1;

        DateTime? selectedDate;
        private NpgsqlConnection con;
        private NpgsqlCommand cmd;

        public Invoice()
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();
            InitializeComboBoxes();
        }

        public Invoice(int custID)
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();
            this.custID = custID;
            InitializeComboBoxes();
        }

        public Invoice(DataRow selectedRow)
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();
            FillInvoiceDetails(selectedRow);
            InitializeComboBoxes();
        }

        private void InitializeComboBoxes()
        {
            //Set the staff combobox
            staff.DataContext = FetchStaff();
            staff.DisplayMemberPath = "Name";
            staff.SelectedValuePath = "StaffId";

            // Set the stores combobox
            store.DataContext = FetchStores();
            store.DisplayMemberPath = "StoreName";
            store.SelectedValuePath = "StoreId";

            //Set the frame combobox
            frame.DataContext = FetchFrames();
            frame.DisplayMemberPath = "FrameDescription";
            frame.SelectedValuePath = "FrameId";

            //Set the lens combobox
            lens.DataContext = FetchLens();
            lens.DisplayMemberPath = "LensDescription";
            lens.SelectedValuePath = "LensId";
        }

        private void FillInvoiceDetails(DataRow row)
        {
            orderID.Text = $"{row["order_id"]}";
            orderStatus.Text = $"{row["order_status"]}";
            appointID.Text = $"{row["appoint_id"]}";
            verifyDate.Text = $"{row["date"]}";
            clientName.Text = $"{row["first_name"]} {row["last_name"]}";
            quantity.Text = $"{row["order_quantity"]}";
            totalAmount.Text = $"{row["total_amount"]}";
            invoiceDate.Text = $"{row["order_date"]}";
        
            if (row["store_id"] != DBNull.Value)
            {
                int storeId = Convert.ToInt32(row["store_id"]);
                store.SelectedValue = storeId;
            }

            if (row["staff_id"] != DBNull.Value)
            {
                int staffId = Convert.ToInt32(row["staff_id"]);
                staff.SelectedValue = staffId;
            }

            if (row["frame_id"] != DBNull.Value)
            {
                int frameId = Convert.ToInt32(row["frame_id"]);
                frame.SelectedValue = frameId;
            }

            if (row["lens_id"] != DBNull.Value)
            {
                int lensId = Convert.ToInt32(row["lens_id"]);
                lens.SelectedValue = lensId;
            }

        }

        public class StaffInfo
        {
            public int StaffId { get; set; }
            public string Name { get; set; }
        }

        public class StoreInfo
        {
            public int StoreId { get; set; }
            public string StoreName { get; set; }
        }

        public class FrameInfo
        {
            public int FrameId { get; set; }
            public string FrameDescription { get; set; }
            public string FrameType { get; set; }
        }

        public class LensInfo
        {
            public int LensId { get; set; }
            public string LensDescription { get; set; }
            public string LensType { get; set; }

        }

        private List<StaffInfo> FetchStaff()
        {
            List<StaffInfo> employees = new List<StaffInfo>();

            // SQL query to join staff, and person tables
            string query = @"SELECT MIN(s.staff_id), p.first_name, p.last_name
                           FROM optic.staff s
                           JOIN optic.person p ON s.person_id = p.person_id
                           GROUP BY p.first_name, p.last_name";
            try
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employees.Add(new StaffInfo
                            {
                                StaffId = reader.GetInt32(0),
                                Name = reader.GetString(1) + " " + reader.GetString(2)
                            });
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"An error occurred while fetching employees: {ex.Message}");
            }
            /*finally
            {
              con.Close();
            }*/

            staff.ItemsSource = employees;

            return employees;
        }

        private List<StoreInfo> FetchStores()
        {
            List<StoreInfo> stores = new List<StoreInfo>();

            // SQL query to get the store_id and store_name from the store table
            string query = "SELECT store_id, name FROM optic.store";

            try
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

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

        private List<FrameInfo> FetchFrames()
        {
            List<FrameInfo> frames = new List<FrameInfo>();

            // SQL query to get the frame_id and frame description (a combination of brand, model, colour, size and price) from the frame table
            string query = @"SELECT frame_id, brand || ' ' || model || ', Colour: ' || colour || ', Size: ' || size || ', Price: $' || frame_price::text AS FrameDescription 
                     FROM optic.frame";

            try
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            frames.Add(new FrameInfo
                            {
                                FrameId = reader.GetInt32(0),
                                FrameType = "Frame",
                                FrameDescription = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"An error occurred while fetching frames: {ex.Message}");
            }
            /* finally
             {
                 con.Close();
             }*/

            frame.ItemsSource = frames;
            return frames;
        }

        // In some method where you need to get the Frame by ID
        public FrameInfo GetFrameById(int frameId)
        {
            var frames = FetchFrames();
            return frames.FirstOrDefault(frame => frame.FrameId == frameId);
        }

        public FrameInfo GetFrameByType(string frameType)
        {
            var frames = FetchFrames();
            return frames.FirstOrDefault(frame => frame.FrameType == frameType);
        }

        private void Frame_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                var selectedFrameId = (int)frame.SelectedValue;
                int storeId = Convert.ToInt32(store.SelectedValue);
                currentInventoryId = GetInventoryId(selectedFrameId, "Frame", storeId);

                DisableOtherComboBox(lens);
            }
        }

        private List<LensInfo> FetchLens()
        {
            List<LensInfo> lenses = new List<LensInfo>();

            // SQL query to get the lens_id and lens description (a combination of type, lens_treatment and price) from the lens table
            string query = @"SELECT lens_id, type || ', Treatment: ' || COALESCE(lens_treatment, 'No Treatment')  || ', Price: $' || lens_price AS LensDescription
                             FROM optic.lens";

            try
            {
                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
                {
                    
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lenses.Add(new LensInfo
                            {
                                LensId = reader.GetInt32(0),
                                LensDescription = reader.GetString(1)
                            });
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"An error occurred while fetching frames: {ex.Message}");
            }
            /* finally
             {
                 con.Close();
             }*/

            lens.ItemsSource = lenses;
            return lenses;
        }

        private void Lens_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                var selectedLensId = (int)lens.SelectedValue;
                int storeId = Convert.ToInt32(store.SelectedValue);

                // Assuming lens_id is what you use to get the inventory id for lenses
                currentInventoryId = GetInventoryId(selectedLensId, "Lens", storeId);

                DisableOtherComboBox(frame);
            }
        }

        private void DisableOtherComboBox(ComboBox comboBoxToDisable)
        {
            if (comboBoxToDisable != null)
            {
                comboBoxToDisable.IsEnabled = false;
                comboBoxToDisable.SelectedIndex = -1; // This will clear the selection
            }
        }

        private void EnableComboBoxes()
        {
            frame.IsEnabled = true;
            lens.IsEnabled = true;
        }

        private void clear_Click(object sender, RoutedEventArgs e)
        {
            // Clear the selection from both ComboBoxes
            frame.SelectedIndex = -1;
            lens.SelectedIndex = -1;

            // Clear the quantity TextBox
            quantity.Text = string.Empty;

            EnableComboBoxes();
        }

        private void yesExam_Checked(object sender, RoutedEventArgs e)
        {
            appointID.IsEnabled = true;

            // Disable the ComboBoxes and the TextBox when the eye exam is checked
            lens.IsEnabled = false;
            frame.IsEnabled = false;
            quantity.IsEnabled = false;
        }

        private void yesExam_Unchecked(object sender, RoutedEventArgs e)
        {
            // Enable the ComboBoxes and the TextBox when the eye exam is unchecked
            lens.IsEnabled = true;
            frame.IsEnabled = true;
            quantity.IsEnabled = true;
        }

        private void verify_Click(object sender, RoutedEventArgs e)
        {
            int appointmentId;

            if (int.TryParse(appointID.Text, out appointmentId))
            {
                try
                {
                    // SQL query to fetch appointment date and person's name based on appointment ID
                    string query = @"SELECT a.date, a.eye_exam_fee, p.first_name, p.last_name 
                             FROM optic.appointment a
                             JOIN optic.customer c ON a.customer_id = c.customer_id
                             JOIN optic.person p ON c.person_id = p.person_id
                             WHERE a.appoint_id = @appointmentId";

                    using (var cmd = new NpgsqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@appointmentId", appointmentId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                verifyDate.SelectedDate = reader.GetDateTime(0);
                                totalAmount.Text = reader.GetDecimal(1).ToString();
                                clientName.Text = reader.GetString(2) + " " + reader.GetString(3);
                            }
                            else
                            {
                                MessageBox.Show("Appointment not found. Please check the ID and try again.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while fetching appointment details: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Invalid appointment ID. Please enter a numeric value.");
            }
        }

        private int GetInventoryId(int itemId, string itemType, int storeID)
        {
            string query;
            switch (itemType)
            {
                case "Frame":
                    query = "SELECT inventory_id FROM optic.inventory WHERE frame_id = @itemId AND store_id = @storeID";
                    break;
                default:
                case "Lens":
                    query = "SELECT inventory_id FROM optic.inventory WHERE lens_id = @itemId AND store_id = @storeID";
                    break;
                    throw new ArgumentException("Invalid item type specified.");
            }

            if (con.State != ConnectionState.Open)
            {
                con.Open();
            }

            using (var cmd = new NpgsqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@itemId", itemId);
                cmd.Parameters.AddWithValue("@storeID", storeID);

                var result = cmd.ExecuteScalar();
                con.Close();

                return result != DBNull.Value && result != null ? Convert.ToInt32(result) : -1;
            }
        }

        private void submit_Click(object sender, RoutedEventArgs e)
        {
            bool isEyeExam = yesExam.IsChecked ?? false;

            selectedDate = invoiceDate.SelectedDate; // Replace with your actual DatePicker for invoiceDate.
            string orderStatusValue = orderStatus.Text.Trim(); // Replace orderStatus with your actual TextBox or ComboBox for order status.
            int storeValue = (int)store.SelectedValue; // Replace storeComboBox with your actual ComboBox for store selection.
            int staffValue = (int)staff.SelectedValue; // Replace staffComboBox with your actual ComboBox for staff selection.
            string totalAmountValue = totalAmount.Text; // Replace totalAmount with your actual TextBox for total amount.
            int quantityValue = string.IsNullOrWhiteSpace(quantity.Text) ? 0 : Convert.ToInt32(quantity.Text); // Replace quantityTextBox with your actual TextBox for quantity.
            int? appointIDValue = string.IsNullOrWhiteSpace(appointID.Text) ? (int?)null : Convert.ToInt32(appointID.Text); // Replace appointID with your actual TextBox for appointment ID.

            // Assuming con is an open NpgsqlConnection.
            if (isEyeExam)
            {
                // Process for eye exam order
                using (var transaction = con.BeginTransaction())
                {
                    try
                    {
                        selectedDate = invoiceDate.SelectedDate;

                        if (selectedDate.HasValue)
                        {
                            string insertOrderQuery = @"
                            INSERT INTO optic.order (order_date, order_status, order_quantity, store_id, staff_id, inventory_id, appoint_id, customer_id, total_amount)
                            VALUES (@orderDate, @orderStatus, @quantity, @storeId, @staffId, @inventoryId, @appointId, @customerId, @totalAmount)
                            RETURNING order_id;";

                            int orderId;
                        
                            cmd = new NpgsqlCommand(insertOrderQuery, con);
                            cmd.Transaction = transaction;

                            cmd.Parameters.AddWithValue("@orderDate", selectedDate ?? DateTime.Now);
                            cmd.Parameters.AddWithValue("@orderStatus", orderStatusValue); // Assuming "Eye Exam" is the status.
                            cmd.Parameters.AddWithValue("@quantity", 1); // Quantity is always 1 for eye exams.
                            cmd.Parameters.AddWithValue("@storeId", storeValue);
                            cmd.Parameters.AddWithValue("@staffId", staffValue);
                            cmd.Parameters.AddWithValue("@inventoryId", DBNull.Value); // No inventory ID for eye exams.
                            cmd.Parameters.AddWithValue("@appointId", appointIDValue);
                            cmd.Parameters.AddWithValue("@customerId", custID ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@totalAmount", decimal.Parse(totalAmountValue));

                            orderId = (int)cmd.ExecuteScalar();
                       
                            transaction.Commit();
                            MessageBox.Show($"Eye exam order submitted successfully with Order ID: {orderId} and Total Amount: {totalAmountValue}");
                        }
                        else
                        {
                            MessageBox.Show("Please select a valid date.");
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Failed to submit eye exam order: {ex.Message}");
                    }
                }
            }
            else
            {
                con.Open();
                // Process for regular order
                using (var transaction = con.BeginTransaction())
                {
                    try
                    {
                        selectedDate = invoiceDate.SelectedDate;

                        if (selectedDate.HasValue)
                        {
                            string insertOrderQuery = @"
                            INSERT INTO optic.order (order_date, order_status, order_quantity, store_id, staff_id, inventory_id, appoint_id, customer_id)
                            VALUES (@orderDate, @orderStatus, @quantity, @storeId, @staffId, @inventoryId, @appointId, @customerId)
                            RETURNING order_id;";

                            int orderId;

                            cmd = new NpgsqlCommand(insertOrderQuery, con);
                            cmd.Transaction = transaction;

                            cmd.Parameters.AddWithValue("@orderDate", selectedDate ?? DateTime.Now);
                            cmd.Parameters.AddWithValue("@orderStatus", orderStatusValue);
                            cmd.Parameters.AddWithValue("@quantity", quantityValue);
                            cmd.Parameters.AddWithValue("@storeId", storeValue);
                            cmd.Parameters.AddWithValue("@staffId", staffValue);
                            //MessageBox.Show(currentInventoryId.ToString());
                            cmd.Parameters.AddWithValue("@inventoryId", currentInventoryId);
                            cmd.Parameters.AddWithValue("@appointId", DBNull.Value);
                            cmd.Parameters.AddWithValue("@customerId", custID ?? (object)DBNull.Value);

                            orderId = (int)cmd.ExecuteScalar();
                            MessageBox.Show(orderId.ToString());
                       
                        // Now retrieve the total amount
                        decimal totalAmountValueDecimal = 0;
                        string selectTotalAmountQuery = "SELECT total_amount FROM optic.order WHERE order_id = @orderId;";

                            cmd = new NpgsqlCommand(selectTotalAmountQuery, con);
                            cmd.Transaction = transaction;

                            cmd.Parameters.AddWithValue("@orderId", orderId);
                            // Execute the select query to retrieve the total amount
                            totalAmountValueDecimal = (decimal)cmd.ExecuteScalar();
                           
                            transaction.Commit();

                            // Update the textbox and show a message with the total amount
                            totalAmount.Text = totalAmountValueDecimal.ToString("C"); // "C" formats the number as a currency
                            MessageBox.Show($"Order submitted successfully with Order ID: {orderId} and Total Amount: {totalAmountValueDecimal.ToString("C")}");
                        }
                        else
                        {
                            MessageBox.Show("Please select a valid date.");
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Failed to submit order: {ex.Message}");
                    }
                }
            }
        }

        private void sendEmail_Click(object sender, RoutedEventArgs e)
        {

        }

        private void print_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


    }
}
