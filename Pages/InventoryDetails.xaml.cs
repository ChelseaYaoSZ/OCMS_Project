﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Globalization;



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
        private int generatedInventoryID;

        public InventoryDetails()
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();

            store.DataContext = FetchStores();
            store.DisplayMemberPath = "StoreName";
            store.SelectedValuePath = "StoreId";
        }

        public InventoryDetails(DataRow selectedRow)
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();
            FillInventoryDetails(selectedRow);

            store.DataContext = FetchStores();
            store.DisplayMemberPath = "StoreName";
            store.SelectedValuePath = "StoreId";
        }

        private void FillInventoryDetails(DataRow row)
        {
            bool isFrame = row["frame_id"] != DBNull.Value;
            bool isLens = row["lens_id"] != DBNull.Value;

            if (isFrame)
            {
                frameID.Text = $"{row["frame_id"]}";
                brand.Text = $"{row["brand"]}";
                model.Text = $"{row["model"]}";
                colour.Text = $"{row["colour"]}";
                size.Text = $"{row["size"]}";
                framePrice.Text = $"{row["frame_price"]}";
                frameQuantity.Text = $"{row["quantity"]}";
                frameInventoryID.Text = $"{row["inventory_id"]}";

                lensID.Clear();
                treatment.Clear();
                type.Clear();
                lensPrice.Clear();
                lensQuantity.Clear();
                lensInventoryID.Clear();
            }
            else if (isLens)
            {
                lensID.Text = $"{row["lens_id"]}";
                treatment.Text = $"{row["lens_treatment"]}";
                type.Text = $"{row["type"]}";
                lensPrice.Text = $"{row["lens_price"]}";
                lensQuantity.Text = $"{row["quantity"]}";
                lensInventoryID.Text = $"{row["inventory_id"]}";

                frameID.Clear();
                brand.Clear();
                model.Clear();
                colour.Clear();
                size.Clear();
                framePrice.Clear();
                frameQuantity.Clear();
                frameInventoryID.Clear();
            }

            if (row["store_id"] != DBNull.Value)
            {
                int storeId = Convert.ToInt32(row["store_id"]);
                store.SelectedValue = storeId; 
            }
        }

        public class StoreInfo
        {
            public int StoreId { get; set; }
            public string StoreName { get; set; }
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

            store.ItemsSource = stores;
            return stores;
        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
                using (NpgsqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        int? generatedFrameId = null;
                        int? generatedLensId = null;
                        int? generatedInventoryId = null;

                        // Adding a frame
                        if (!string.IsNullOrWhiteSpace(framePrice.Text)) // Replace with your actual check for adding a frame
                        {
                            string frameQuery = "INSERT INTO optic.frame (brand, model, colour, size, frame_price) " +
                                                "VALUES (@brand, @model, @colour, @size, @price) " +
                                                "RETURNING frame_id";

                            using (var frameCmd = new NpgsqlCommand(frameQuery, con, transaction))
                            {
                                frameCmd.Parameters.AddWithValue("@brand", brand.Text);
                                frameCmd.Parameters.AddWithValue("@model", model.Text);
                                frameCmd.Parameters.AddWithValue("@colour", colour.Text);
                                frameCmd.Parameters.AddWithValue("@size", size.Text);

                                if (decimal.TryParse(framePrice.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal framePriceValue))
                                {
                                    frameCmd.Parameters.AddWithValue("@price", framePriceValue);
                                    generatedFrameId = (int)frameCmd.ExecuteScalar();
                                }
                                else
                                {
                                    MessageBox.Show("Please enter a valid frame price.");
                                    return;
                                }
                            }
                        }

                        // Adding a lens
                        if (!string.IsNullOrWhiteSpace(lensPrice.Text)) // Replace with your actual check for adding a lens
                        {
                            string lensQuery = "INSERT INTO optic.lens (lens_treatment, type, lens_price) " +
                                               "VALUES (@treatment, @type, @price) " +
                                               "RETURNING lens_id";

                            using (var lensCmd = new NpgsqlCommand(lensQuery, con, transaction))
                            {
                                lensCmd.Parameters.AddWithValue("@treatment", treatment.Text);
                                lensCmd.Parameters.AddWithValue("@type", type.Text);

                                if (decimal.TryParse(lensPrice.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal lensPriceValue))
                                {
                                    lensCmd.Parameters.AddWithValue("@price", lensPriceValue);
                                    generatedLensId = (int)lensCmd.ExecuteScalar();
                                }
                                else
                                {
                                    MessageBox.Show("Please enter a valid lens price.");
                                    return;
                                }
                            }
                        }

                        // Ensure at least one ID is generated
                        if (!generatedFrameId.HasValue && !generatedLensId.HasValue)
                        {
                            MessageBox.Show("Please enter details to add either a frame or a lens.");
                            transaction.Rollback(); // Rollback transaction as nothing has been added
                            return;
                        }

                        // Insert into inventory with the generated IDs
                        string inventoryQuery = "INSERT INTO optic.inventory (frame_id, lens_id, quantity, store_id) " +
                                                "VALUES (@frameID, @lensID, @quantity, @storeID) " +
                                                "RETURNING inventory_id";

                        using (var inventoryCmd = new NpgsqlCommand(inventoryQuery, con, transaction))
                        {
                            inventoryCmd.Parameters.AddWithValue("@frameID", generatedFrameId.HasValue ? (object)generatedFrameId : DBNull.Value);
                            inventoryCmd.Parameters.AddWithValue("@lensID", generatedLensId.HasValue ? (object)generatedLensId : DBNull.Value);
                            inventoryCmd.Parameters.AddWithValue("@quantity", generatedFrameId.HasValue ? int.Parse(frameQuantity.Text) : int.Parse(lensQuantity.Text));
                            inventoryCmd.Parameters.AddWithValue("@storeID", store.SelectedValue); // Make sure this is the correct value

                           generatedInventoryId = (int)inventoryCmd.ExecuteScalar();
                        }

                        // Commit transaction if all is well
                        transaction.Commit();
                       
                    // Set the generated IDs to the TextBoxes
                        if (generatedFrameId.HasValue)
                        {
                            frameID.Text = generatedFrameId.Value.ToString();
                            frameInventoryID.Text = generatedInventoryId.Value.ToString();
                        }

                        if (generatedLensId.HasValue)
                        {
                            lensID.Text = generatedLensId.Value.ToString();
                            lensInventoryID.Text = generatedInventoryId.Value.ToString();
                        }

                    MessageBox.Show("Item was added successfully.");
                    }
                    catch (Exception ex) // Catch a more general exception
                    {
                        // If an error occurs, rollback the transaction and show the message
                        transaction.Rollback();
                        MessageBox.Show($"An error occurred: {ex.Message}");
                    }
                }
            }





        private void update_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            bool deletionOccurred = false; // A flag to indicate if a deletion has been made

            if (int.TryParse(frameInventoryID.Text, out int frameInventoryId) && frameInventoryId != 0)
            {
                // Frame inventory ID is valid and not zero, proceed with deletion as a frame
                DeleteInventoryItem(frameInventoryId, true);
                deletionOccurred = true;
            }
            else if (int.TryParse(lensInventoryID.Text, out int lensInventoryId) && lensInventoryId != 0)
            {
                // Lens inventory ID is valid and not zero, proceed with deletion as a lens
                DeleteInventoryItem(lensInventoryId, false);
                deletionOccurred = true;
            }

            // Check if neither deletion occurred, meaning both inputs were invalid or not provided
            if (!deletionOccurred)
            {
                MessageBox.Show("Please enter a valid Inventory ID for either a frame or a lens.");
            }
        }

        private void DeleteInventoryItem(int inventoryId, bool isFrame)
        {
            string deleteInventoryQuery = "DELETE FROM optic.inventory WHERE inventory_id = @inventoryId";
            string deleteFrameQuery = "DELETE FROM optic.frame WHERE frame_id = @frameId";
            string deleteLensQuery = "DELETE FROM optic.lens WHERE lens_id = @lensId";

            int rowsAffected = 0;

            // Start a transaction to ensure both inventory and corresponding frame or lens are deleted
            using (NpgsqlTransaction transaction = con.BeginTransaction())
            {
                try
                {
                    // Delete the inventory item
                    using (NpgsqlCommand cmdInventory = new NpgsqlCommand(deleteInventoryQuery, con))
                    {
                        cmdInventory.Transaction = transaction;
                        cmdInventory.Parameters.AddWithValue("@inventoryId", inventoryId);
                        rowsAffected = cmdInventory.ExecuteNonQuery();
                    }

                    // If the inventory item was deleted successfully and it is a frame, delete the frame as well
                    if (isFrame && rowsAffected > 0)
                    {
                        using (NpgsqlCommand cmdFrame = new NpgsqlCommand(deleteFrameQuery, con))
                        {
                            cmdFrame.Transaction = transaction;
                            cmdFrame.Parameters.AddWithValue("@frameId", inventoryId); // Assuming inventoryId is the frameId
                            cmdFrame.ExecuteNonQuery();
                        }
                    }
                    // If it's a lens, delete the lens
                    else if (!isFrame && rowsAffected > 0)
                    {
                        using (NpgsqlCommand cmdLens = new NpgsqlCommand(deleteLensQuery, con))
                        {
                            cmdLens.Transaction = transaction;
                            cmdLens.Parameters.AddWithValue("@lensId", inventoryId); // Assuming inventoryId is the lensId
                            cmdLens.ExecuteNonQuery();
                        }
                    }

                    // Commit the transaction
                    transaction.Commit();

                    // Confirmation message and clearing of controls
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("The inventory item and associated frame/lens were successfully deleted.");

                  
                        // Clear text boxes
                        frameInventoryID.Text = "";
                        lensInventoryID.Text = "";
                        frameID.Text = "";
                        lensID.Text = "";
                        brand.Text = "";
                        model.Text = "";
                        colour.Text = "";
                        framePrice.Text = "";
                        lensPrice.Text = "";
                        treatment.Text = "";
                        type.Text = "";
                        size.Text = "";
                        lensQuantity.Text = "";
                        frameQuantity.Text = "";

                        // Reset combo boxes
                        store.SelectedIndex = -1;
                    }
                    else
                    {
                        MessageBox.Show("No inventory item was found with the given ID.");
                    }
                }
                catch (NpgsqlException ex)
                {
                    // Rollback the transaction in case of an error
                    transaction.Rollback();
                    MessageBox.Show($"An error occurred while deleting the item: {ex.Message}");
                }
            }
        }


       
    }
}