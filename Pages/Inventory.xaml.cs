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
    /// Interaction logic for Inventory.xaml
    /// </summary>
    public partial class Inventory : Window
    {
        private NpgsqlConnection con;
        public Inventory()
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();
            LoadAllInventory();
            this.Closed += Inventory_Closed;
        }

        public DataTable GetAllInventory()
        {
            string query = @"SELECT i.inventory_id, i.quantity, i.last_update,
                                l.lens_id, l.lens_treatment, l.type, l.lens_price,
                                f.frame_id, f.brand, f.model, f.colour, f.size, f.frame_price,
                                s.store_id, s.name    
                                FROM optic.inventory i
                                LEFT JOIN optic.lens l on i.lens_id = l.lens_id
                                LEFT JOIN optic.frame f on i.frame_id = f.frame_id
                                LEFT JOIN optic.store s on i.store_id = s.store_id";

            NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(query, con);

            DataTable dataTable = new DataTable();

            dataAdapter.Fill(dataTable);

            return dataTable;
        }

        public void LoadAllInventory()
        {
            DataTable inventory = GetAllInventory();
            dataGridInventory.ItemsSource = inventory.DefaultView;
        }

        public DataTable SearchInventory(string searchTerm)
        {
            string query = @"SELECT i.inventory_id, i.quantity, i.last_update,
                                l.lens_id, l.lens_treatment, l.type, l.lens_price,
                                f.frame_id, f.brand, f.model, f.colour, f.size, f.frame_price,
                                s.store_id, s.name  
                                FROM optic.inventory i
                                LEFT JOIN optic.lens l on i.lens_id = l.lens_id
                                LEFT JOIN optic.frame f on i.frame_id = f.frame_id
                                LEFT JOIN optic.store s on i.store_id = s.store_id
                                WHERE CAST(i.inventory_id AS TEXT) ILIKE @SearchTerm 
                                    OR l.type ILIKE @SearchTerm";

            NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(query, con);
            dataAdapter.SelectCommand.Parameters.AddWithValue("@SearchTerm", searchTerm);
            DataTable dataTable = new DataTable();
            dataAdapter.Fill(dataTable);
            return dataTable;
        }

        public void LoadSpecificInventory()
        {
            string searchTerm = searchInfo.Text;
            DataTable specificInventory = SearchInventory(searchTerm);
            dataGridInventory.ItemsSource = specificInventory.DefaultView;
        }

        private void search_Click(object sender, RoutedEventArgs e)
        {
            LoadSpecificInventory();
        }

        private void Inventory_Closed(object sender, EventArgs e)
        {
            if (con != null)
            {
                con.Close();
                con.Dispose();
            }
        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            InventoryDetails inventoryDetails = new InventoryDetails();
            inventoryDetails.Show();
        }

        private void DataGridInventory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var grid = (DataGrid)sender;
            var selectedInventory = (DataRowView)grid.SelectedItem;
            if (selectedInventory != null)
            {
                var inventoryDetails = new InventoryDetails(selectedInventory.Row);
                inventoryDetails.Show();
            }
        }

    }
}

