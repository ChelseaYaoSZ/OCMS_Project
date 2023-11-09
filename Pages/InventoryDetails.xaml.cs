using Npgsql;
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
        private int generatedInventoryId;

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

        private bool updateBrand = false;
        private bool  updateModel= false;
        private bool updateColour= false;
        private bool updateSize= false;
        private bool updateFramePrice = false;
        private bool updateLensType = false;
        private bool updateLensTreatment = false;
        private bool updateLensPrice = false;
        private bool updateFrameInvQuantity = false;
        private bool updateLensInvQuantity = false;

        private void CheckForUpdates()
        {
            if (!string.IsNullOrWhiteSpace(brand.Text))
            {
                updateBrand = true;
            }

            if (!string.IsNullOrWhiteSpace(model.Text))
            {
                updateModel = true;
            }

            if (!string.IsNullOrWhiteSpace(colour.Text))
            {
                updateColour = true;
            }

            if (!string.IsNullOrWhiteSpace(size.Text))
            {
                updateSize = true;
            }

            if (!string.IsNullOrWhiteSpace(framePrice.Text))
            {
                updateFramePrice = true;
            }

            if (!string.IsNullOrWhiteSpace(type.Text))
            {
                updateLensType = true;
            }

            if (!string.IsNullOrWhiteSpace(treatment.Text))
            {
                updateLensTreatment = true;
            }

            if (!string.IsNullOrWhiteSpace(lensPrice.Text))
            {
                updateLensPrice = true;
            }

            if (!string.IsNullOrWhiteSpace(lensQuantity.Text))
            {
                updateLensInvQuantity = true;
            }

            if (!string.IsNullOrWhiteSpace(frameQuantity.Text))
            {
                updateFrameInvQuantity = true;
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

        private void updateFrame(int generatedFrameId, bool updateBrand, bool updateModel, bool updateColour, bool updateSize, bool updateFramePrice)
        {
            int frameid = generatedFrameId;
            MessageBox.Show(frameid.ToString());
            using (NpgsqlTransaction transaction = con.BeginTransaction())
            {
                try
                {
                    string frameQuery = "UPDATE optic.frame SET ";

                    if (updateBrand)
                    {
                        frameQuery += "brand = @brand, ";
                    }

                    if (updateModel)
                    {
                        frameQuery += "model = @model, ";
                    }

                    if (updateColour)
                    {
                        frameQuery += "colour = @colour, ";
                    }

                    if (updateSize)
                    {
                        frameQuery += "size = @size, ";
                    }

                    if (updateFramePrice)
                    {
                        frameQuery += "frame_price = @price, ";
                    }

                    frameQuery = frameQuery.TrimEnd(',', ' ');
                    frameQuery += " WHERE frame_id = @frame_id";

                    NpgsqlCommand cmd = new NpgsqlCommand(frameQuery, con);
                    cmd.Transaction = transaction;

                    if (updateBrand)
                    {
                        cmd.Parameters.AddWithValue("@brand", brand.Text);
                    }

                    if (updateModel)
                    {
                        cmd.Parameters.AddWithValue("@model", model.Text);
                    }

                    if (updateColour)
                    {
                        cmd.Parameters.AddWithValue("@colour", colour.Text);
                    }

                    if (updateSize)
                    {
                        cmd.Parameters.AddWithValue("@size", size.Text);
                    }

                    if (updateFramePrice)
                    {
                        cmd.Parameters.AddWithValue("@price", decimal.Parse(framePrice.Text));
                    }

                    cmd.Parameters.AddWithValue("@frame_id", generatedFrameId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    MessageBox.Show($"{rowsAffected} rows were updated.");

                    transaction.Commit();

                    MessageBox.Show("Frame information has been updated successfully.");
                }
                catch (NpgsqlException ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"An error occurred while updating frame information: {ex.Message}");
                }
            }
        }

        private void updateLens(int generatedLensId, bool updateLensType, bool updateLensTreatment, bool updateLensPrice)
        {
            int lensid = generatedLensId;
            MessageBox.Show(lensid.ToString());
            using (NpgsqlTransaction transaction = con.BeginTransaction())
            {
                try
                {
                    string lensQuery = "UPDATE optic.lens SET ";

                    if (updateLensType)
                    {
                        lensQuery += "type = @type, ";
                    }

                    if (updateLensTreatment)
                    {
                        lensQuery += "lens_treatment = @treatment, ";
                    }

                    if (updateLensPrice)
                    {
                        lensQuery += "lens_price = @price, ";
                    }

                    // Remove the trailing comma and space only if we added something to update
                    if (updateLensType || updateLensTreatment || updateLensPrice)
                    {
                        lensQuery = lensQuery.TrimEnd(',', ' ');
                    }

                    // Now we can safely append the WHERE clause
                    lensQuery += " WHERE lens_id = @lens_id";

                    NpgsqlCommand cmd = new NpgsqlCommand(lensQuery, con);
                    cmd.Transaction = transaction;

                    if (updateLensType)
                    {
                        cmd.Parameters.AddWithValue("@type", type.Text);
                    }

                    if (updateLensTreatment)
                    {
                        cmd.Parameters.AddWithValue("@treatment", treatment.Text);
                    }

                    if (updateLensPrice)
                    {
                        cmd.Parameters.AddWithValue("@price", decimal.Parse(lensPrice.Text));
                    }

                    cmd.Parameters.AddWithValue("@lens_id", generatedLensId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    MessageBox.Show($"{rowsAffected} rows were updated.");

                    transaction.Commit();

                    MessageBox.Show("Lens information has been updated successfully.");
                }
                catch (NpgsqlException ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"An error occurred while updating lens information: {ex.Message}");
                }
            }
        }


        private void updateFrameInventory(int frameInventoryId, bool updateFrameInvQuantity)
        {
            int frameinv = frameInventoryId;
            MessageBox.Show(frameInventoryId.ToString());
            using (NpgsqlTransaction transaction = con.BeginTransaction())
            {
                try
                {
                    string inventoryQuery = "UPDATE optic.inventory SET ";

                    if (updateFrameInvQuantity)
                    {
                        inventoryQuery += "quantity = @quantity ";
                    }

                    inventoryQuery += "WHERE inventory_id = @inventory_id";

                    NpgsqlCommand cmd = new NpgsqlCommand(inventoryQuery, con);
                    cmd.Transaction = transaction;

                    if (updateFrameInvQuantity)
                    {
                        cmd.Parameters.AddWithValue("@quantity", int.Parse(frameQuantity.Text));
                    }

                    cmd.Parameters.AddWithValue("@inventory_id", frameInventoryId);

                    cmd.ExecuteNonQuery();

                    transaction.Commit();

                    MessageBox.Show("Inventory has been updated successfully.");
                }
                catch (NpgsqlException ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"An error occurred while updating inventory: {ex.Message}");
                }
            }
        }

        private void updateLensInventory(int lensInventoryId, bool updateLensInvQuantity)
        {
            int lensinv = lensInventoryId;
            MessageBox.Show(lensInventoryId.ToString());
            using (NpgsqlTransaction transaction = con.BeginTransaction())
            {
                try
                {
                    string inventoryQuery = "UPDATE optic.inventory SET ";

                    if (updateLensInvQuantity)
                    {
                        inventoryQuery += "quantity = @quantity ";
                    }

                    inventoryQuery += "WHERE inventory_id = @inventory_id";

                    NpgsqlCommand cmd = new NpgsqlCommand(inventoryQuery, con);
                    cmd.Transaction = transaction;

                    if (updateLensInvQuantity)
                    {
                        cmd.Parameters.AddWithValue("@quantity", int.Parse(lensQuantity.Text));
                    }

                    cmd.Parameters.AddWithValue("@inventory_id", lensInventoryId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    MessageBox.Show($"{rowsAffected} rows were updated.");

                    transaction.Commit();

                    MessageBox.Show("Inventory has been updated successfully.");
                }
                catch (NpgsqlException ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"An error occurred while updating inventory: {ex.Message}");
                }
            }
        }

        private void update_Click(object sender, RoutedEventArgs e)
        {
            CheckForUpdates();

            bool updateFrameBool = false;
            bool updateLensBool = false;
            bool updateFrameInventoryBool = false;
            bool updateLensInventoryBool = false;

            // Parse IDs from the TextBoxes for frame and lens
            if (int.TryParse(frameID.Text, out int localFrameId))
            {
                updateFrameBool = true; // only set to true if parsing was successful
                updateFrame(localFrameId, updateBrand, updateModel, updateColour, updateSize, updateFramePrice);
            }
            else if (!string.IsNullOrEmpty(frameID.Text)) // only show the message if something was entered
            {
                MessageBox.Show("Invalid Frame ID");
            }

            if (int.TryParse(lensID.Text, out int localLensId))
            {
                updateLensBool = true; // only set to true if parsing was successful
                updateLens(localLensId, updateLensTreatment, updateLensType, updateLensPrice);
            }
            else if (!string.IsNullOrEmpty(lensID.Text)) // only show the message if something was entered
            {
                MessageBox.Show("Invalid Lens ID");
            }

            // Parse IDs from the TextBoxes for inventory
            if (int.TryParse(frameInventoryID.Text, out int localFrameInventoryId))
            {
                updateFrameInventoryBool = true; // only set to true if parsing was successful
                updateFrameInventory(localFrameInventoryId, updateFrameInvQuantity); // Consider changing the method to accept int if appropriate
            }
            else if (!string.IsNullOrEmpty(frameInventoryID.Text)) // only show the message if something was entered
            {
                MessageBox.Show("Invalid Frame Inventory ID");
            }

            if (int.TryParse(lensInventoryID.Text, out int localLensInventoryId))
            {
                updateLensInventoryBool = true; // only set to true if parsing was successful
                updateLensInventory(localLensInventoryId, updateLensInvQuantity); // Consider changing the method to accept int if appropriate
            }
            else if (!string.IsNullOrEmpty(lensInventoryID.Text)) // only show the message if something was entered
            {
                MessageBox.Show("Invalid Lens Inventory ID");
            }

            // Check if no valid IDs were provided for any of the updates
            if (!updateFrameBool && !updateLensBool && !updateFrameInventoryBool && !updateLensInventoryBool)
            {
                MessageBox.Show("No valid IDs found for updating.");
            }
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
