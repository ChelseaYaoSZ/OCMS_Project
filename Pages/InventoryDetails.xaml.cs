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
    /// Interaction logic for InventoryDetails.xaml
    /// </summary>
    public partial class InventoryDetails : Window
    {
        private NpgsqlConnection con;
        private NpgsqlCommand cmd;

        private int generatedFrameId;
        private int generatedLensId;

        public InventoryDetails()
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();
        }

        public InventoryDetails(DataRow selectedRow)
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();
            //FillInventoryDetails(selectedRow);
        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            using (NpgsqlTransaction transaction = con.BeginTransaction())
            {
                try
                {
                        // SQL query with parameters
                        string frameQuery = "INSERT INTO optic.frame(brand, model, colour, size, price) " +
                                       " VALUES (@brand, @model, @colour, @size, @price) " +
                                       " RETURNING frame_id";

                        cmd = new NpgsqlCommand(frameQuery, con);
                        cmd.Transaction = transaction;

                        // Add values for the parameters in the query
                        cmd.Parameters.AddWithValue("@brand", brand.Text);
                        cmd.Parameters.AddWithValue("@model", model.Text);
                        cmd.Parameters.AddWithValue("@colour", colour.Text);
                        cmd.Parameters.AddWithValue("@size", size.Text);
                        cmd.Parameters.AddWithValue("@price", framePrice.Text);

                        // Execute the query and retrieve the generated person_id
                        generatedFrameId = (int)cmd.ExecuteScalar();

                        
                }
                catch (NpgsqlException ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"An error occured: {ex.Message}");
                }
            }
        }

       /* private void add_Click(object sender, RoutedEventArgs e)
        {

        }*/

        private void update_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
