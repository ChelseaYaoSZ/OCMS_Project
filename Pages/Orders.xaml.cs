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
    /// Interaction logic for Orders.xaml
    /// </summary>
    public partial class Orders : Window
    {
        private NpgsqlConnection con;
        private string _customerId;
        public Orders()
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();
            LoadAllOrders();
            this.Closed += OrderInfoPage_Closed;
        }

        public Orders(string customerId)
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();
            _customerId = customerId;
            LoadSpecificOrder();
        }


        public DataTable GetAllOrders()
        {
            string query = @"SELECT o.order_id, o.order_date, o.order_status, o.order_quantity, o.total_amount,
                                    p.first_name,p.last_name, p.person_id, o.staff_id,
                                    a.appoint_id, a.eye_exam_fee, a.date, o.order_date,
                                    i.inventory_id, l.lens_id, l.lens_treatment, f.frame_id,
									f.brand, s.store_id
                            FROM optic.order o
                            left JOIN optic.customer c ON o.customer_id = c.customer_id
                            left JOIN optic.person p ON c.person_id = p.person_id
                            left JOIN optic.inventory i ON i.inventory_id = o.inventory_id
                            left JOIN optic.appointment a ON a.appoint_id = o.appoint_id
                            left JOIN optic.lens l ON i.lens_id = l.lens_id
                            left JOIN optic.frame f ON f.frame_id = i.frame_id
                            left JOIN optic.store s ON i.store_id = s.store_id";

            NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(query, con);

            DataTable dataTable = new DataTable();

            dataAdapter.Fill(dataTable);

            return dataTable;
        }

        public void LoadAllOrders()
        {
            DataTable orders = GetAllOrders();
            dataGridOrders.ItemsSource = orders.DefaultView;
        }

        public DataTable SearchOrders(string searchTerm, DateTime? orderDate, int? personId)
        {
            string query = @"SELECT o.order_id, o.order_date, o.order_status, o.order_quantity, o.total_amount,
                                    p.person_id,p.first_name,p.last_name, o.staff_id, o.order_date,
                                    a.appoint_id, a.eye_exam_fee, a.date,
                                    i.inventory_id,l.lens_id,l.lens_treatment, f.frame_id,
									f.brand, s.store_id
                            FROM optic.order o
                            left JOIN optic.customer c ON o.customer_id = c.customer_id
                            left JOIN optic.person p ON c.person_id = p.person_id
                            left JOIN optic.inventory i ON i.inventory_id = o.inventory_id
                            left JOIN optic.appointment a ON a.appoint_id = o.appoint_id
                            left JOIN optic.lens l ON i.lens_id = l.lens_id
                            left JOIN optic.frame f ON f.frame_id = i.frame_id
                            left JOIN optic.store s ON i.store_id = s.store_id";

            List<string> conditions = new List<string>();

            // Check if a search term was provided and add it to the conditions
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string searchCondition = @"(p.first_name ILIKE @SearchTerm OR 
                                            p.last_name ILIKE @SearchTerm)";
                conditions.Add(searchCondition);
            }

            // Check if a date was provided and add it to the conditions
            if (orderDate.HasValue)
            {
                conditions.Add("o.order_date = @OrderDate");
            }

            // Check if a person ID was provided and add it to the conditions
            if (personId.HasValue)
            {
                conditions.Add("p.person_id = @PersonID");
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
            if (orderDate.HasValue)
            {
                dataAdapter.SelectCommand.Parameters.AddWithValue("@OrderDate", orderDate.Value);
            }
            if (personId.HasValue)
            {
                dataAdapter.SelectCommand.Parameters.AddWithValue("@PersonID", personId.Value);
            }

            DataTable dataTable = new DataTable();
            dataAdapter.Fill(dataTable);
            return dataTable;
        }

        public void LoadSpecificOrder()
        {
            string searchTerm = "";
            DateTime? selectedDate = null;
            int? personId = null;

            if (!string.IsNullOrEmpty(_customerId))
            {
                personId = int.Parse(_customerId);

            }
            else
            {
                searchTerm = name.Text;
                selectedDate = date.SelectedDate;
            }

            DataTable orders = SearchOrders(searchTerm, selectedDate, personId);
            dataGridOrders.ItemsSource = orders.DefaultView;
        }

        private void search_Click(object sender, RoutedEventArgs e)
        {
            LoadSpecificOrder();
        }

        private void OrderInfoPage_Closed(object sender, EventArgs e)
        {
            if (con != null)
            {
                con.Close();
                con.Dispose();
            }
        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            Invoice orderDetails = new Invoice();
            orderDetails.Show();
        }

        private void DataGridOrders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var grid = (DataGrid)sender;
            var selectedOrder = (DataRowView)grid.SelectedItem;
            if (selectedOrder != null)
            {
                var orderDetails = new Invoice(selectedOrder.Row);
                orderDetails.Show();
            }
        }

    }
}
