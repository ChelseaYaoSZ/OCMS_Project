using Npgsql;
using Npgsql.Replication.PgOutput.Messages;
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
    /// Interaction logic for CustomerDetails.xaml
    /// </summary>
    public partial class CustomerDetails : Window
    {
        // Retrieve the selected date from the Date Picker control and format it
        DateTime? selectedDate;
        private NpgsqlConnection con;
        private NpgsqlCommand cmd;

        private int generatedPersonId;
        private int generatedAddressId;
        private int generatedCustomerId;

        public CustomerDetails()
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();
        }

        public CustomerDetails(DataRow selectedRow)
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();
            FillCustomerDetails(selectedRow);
        }

        private void FillCustomerDetails(DataRow row)
        {
            personID.Text = $"{row["person_id"]}";
            firstName.Text = $"{row["first_name"]}";
            lastName.Text = $"{row["last_name"]}";
            birthDate.Text = $"{row["birth_date"]}";
            phoneNum.Text = $"{row["phone"]}";
            email.Text = $"{row["email"]}";
            addressID.Text = $"{row["address_id"]}";
            address.Text = $"{row["address"]}";
            city.Text = $"{row["city"]}";
            postalCode.Text = $"{row["postal_code"]}";
            custID.Text = $"{row["customer_id"]}";
            prescription.Text = $"{row["prescription"]}";
        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            using (NpgsqlTransaction transaction = con.BeginTransaction())
            {
                try
                {
                    selectedDate = birthDate.SelectedDate;

                    if (selectedDate.HasValue)
                    {
                        // SQL query with parameters
                        string personQuery = "INSERT INTO optic.person (first_name, last_name, birth_date, email, phone) " +
                                       " VALUES (@first_name, @last_name, @birth_date, @email, @phone) " +
                                       " RETURNING person_id";

                        cmd = new NpgsqlCommand(personQuery, con);
                        cmd.Transaction = transaction;

                        // Add values for the parameters in the query
                        cmd.Parameters.AddWithValue("@first_name", firstName.Text);
                        cmd.Parameters.AddWithValue("@last_name", lastName.Text);
                        cmd.Parameters.AddWithValue("@birth_date", selectedDate);
                        cmd.Parameters.AddWithValue("@phone", phoneNum.Text);
                        cmd.Parameters.AddWithValue("@email", email.Text);

                        // Execute the query and retrieve the generated person_id
                        generatedPersonId = (int)cmd.ExecuteScalar();

                        //Insert data into the address table 
                        string addressQuery = "INSERT INTO optic.address (address, city, postal_code) " +
                                              " VALUES (@address, @city, @postal_code) " +
                                              " RETURNING address_id";

                        cmd = new NpgsqlCommand(addressQuery, con);
                        cmd.Transaction = transaction;

                        //Add values for the parameters in the
                        cmd.Parameters.AddWithValue("@address", address.Text);
                        cmd.Parameters.AddWithValue("@city", city.Text);
                        cmd.Parameters.AddWithValue("@postal_code", postalCode.Text);

                        // Execute the query and retrieve the generated person_id
                        generatedAddressId = (int)cmd.ExecuteScalar();

                        // SQL query with parameters for customer
                        string customerQuery = "INSERT INTO optic.customer (person_id, address_id, prescription) " +
                                               "VALUES (@person_id, @address_id, @prescription) " +
                                               "RETURNING customer_id";

                        cmd = new NpgsqlCommand(customerQuery, con);
                        cmd.Transaction = transaction;

                        // Add values for the parameters in the query
                        cmd.Parameters.AddWithValue("@person_id", generatedPersonId);
                        cmd.Parameters.AddWithValue("@address_id", generatedAddressId);
                        cmd.Parameters.AddWithValue("@prescription", prescription.Text);

                        // Execute the query and retrieve the generated customer_id
                        generatedCustomerId = (int)cmd.ExecuteScalar();

                        // Commit the transaction if everything succeeds
                        transaction.Commit();

                        // Set the generated person_id to the personID TextBox
                        personID.Text = generatedPersonId.ToString();
                        addressID.Text = generatedAddressId.ToString();
                        custID.Text = generatedCustomerId.ToString();
                        MessageBox.Show("Customer and Address were added successfully!");
                    }
                    else
                    {
                        MessageBox.Show("Please select a valid date of birth.");
                    }
                }
                catch (NpgsqlException ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"An error occured: {ex.Message}");
                }
            }
        }

        private bool updateFirstName = false;
        private bool updateLastName = false;
        private bool updateEmail = false;
        private bool updatePhone = false;
        private bool updateBirthDate = false;
        private bool updateAddress = false;
        private bool updateCity = false;
        private bool updatePostalCode = false;
        private bool updatePrescription = false;

        private void CheckForUpdates()
        {
            if (!string.IsNullOrWhiteSpace(firstName.Text))
            {
                updateFirstName = true;
            }

            if (!string.IsNullOrWhiteSpace(lastName.Text))
            {
                updateLastName = true;
            }

            if (!string.IsNullOrWhiteSpace(email.Text))
            {
                updateEmail = true;
            }

            if (!string.IsNullOrWhiteSpace(phoneNum.Text))
            {
                updatePhone = true;
            }

            if (birthDate.SelectedDate != null)
            {
                updateBirthDate = true;
            }

            if (!string.IsNullOrWhiteSpace(address.Text))
            {
                updateAddress = true;
            }

            if (!string.IsNullOrWhiteSpace(city.Text))
            {
                updateCity = true;
            }

            if (!string.IsNullOrWhiteSpace(postalCode.Text))
            {
                updatePostalCode = true;
            }

            if (!string.IsNullOrWhiteSpace(prescription.Text))
            {
                updatePrescription = true;
            }
        }

        private void updatePerson(int personId, bool updateFirstName, bool updateLastName, bool updateEmail, bool updatePhone, bool updateBirthDate)
        {
            using (NpgsqlTransaction transaction = con.BeginTransaction())
            {
                try
                {
                    selectedDate = birthDate.SelectedDate;

                    string personQuery = "UPDATE optic.person SET ";

                    if (updateFirstName)
                    {
                        personQuery += "first_name = @first_name, ";
                    }

                    if (updateLastName)
                    {
                        personQuery += "last_name = @last_name, ";
                    }

                    if (updateEmail)
                    {
                        personQuery += "email = @email, ";
                    }

                    if (updatePhone)
                    {
                        personQuery += "phone = @phone, ";
                    }

                    if (updateBirthDate)
                    {
                        personQuery += "birth_date = @birth_date, ";
                    }

                    // Remove the trailing comma and space
                    personQuery = personQuery.TrimEnd(',', ' ');

                    personQuery += " WHERE person_id = @person_id";

                    cmd = new NpgsqlCommand(personQuery, con);
                    cmd.Transaction = transaction;

                    if (updateFirstName)
                    {
                        cmd.Parameters.AddWithValue("@first_name", firstName.Text);
                    }

                    if (updateLastName)
                    {
                        cmd.Parameters.AddWithValue("@last_name", lastName.Text);
                    }

                    if (updateEmail)
                    {
                        cmd.Parameters.AddWithValue("@email", email.Text);
                    }

                    if (updatePhone)
                    {
                        cmd.Parameters.AddWithValue("@phone", phoneNum.Text);
                    }

                    if (updateBirthDate)
                    {
                        cmd.Parameters.AddWithValue("@birth_date", selectedDate);
                    }

                    cmd.Parameters.AddWithValue("@person_id", personId);

                    cmd.ExecuteNonQuery();

                    transaction.Commit();

                    MessageBox.Show("Customer information has been updated successfully.");
                }
                catch (NpgsqlException ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"An error occurred while updating customer information: {ex.Message}");
                }
            }
        }

        private void updateCustAddress(int addressId, bool updateAddress, bool updateCity, bool updatePostalCode)
        {
            using (NpgsqlTransaction transaction = con.BeginTransaction())
            {
                try
                {
                    string addressQuery = "UPDATE optic.address SET ";

                    if (updateAddress)
                    {
                        addressQuery += "address = @address, ";
                    }

                    if (updateCity)
                    {
                        addressQuery += "city = @city, ";
                    }

                    if (updatePostalCode)
                    {
                        addressQuery += "postal_code = @postal_code, ";
                    }

                    // Remove the trailing comma and space
                    addressQuery = addressQuery.TrimEnd(',', ' ');

                    addressQuery += " WHERE address_id = @address_id";

                    cmd = new NpgsqlCommand(addressQuery, con);
                    cmd.Transaction = transaction;

                    if (updateAddress)
                    {
                        cmd.Parameters.AddWithValue("@address", address.Text);
                    }

                    if (updateCity)
                    {
                        cmd.Parameters.AddWithValue("@city", city.Text);
                    }

                    if (updatePostalCode)
                    {
                        cmd.Parameters.AddWithValue("@postal_code", postalCode.Text);
                    }

                    cmd.Parameters.AddWithValue("@address_id", addressId);

                    cmd.ExecuteNonQuery();

                    transaction.Commit();

                    MessageBox.Show("Address information has been updated successfully.");
                }
                catch (NpgsqlException ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"An error occurred while updating address information: {ex.Message}");
                }
            }
        }

        private void updateCustomer(int generatedCustomerId, bool updatePrescription)
        {
            using (NpgsqlTransaction transaction = con.BeginTransaction())
            {
                try
                {
                    string customerQuery = "UPDATE optic.customer SET ";
                    bool needToUpdate = false; // Flag to track if we have something to update

                    if (updatePrescription)
                    {
                        customerQuery += "prescription = @prescription";
                        needToUpdate = true;
                    }

                    // Only append WHERE clause if we have something to update
                    if (needToUpdate)
                    {
                        customerQuery += " WHERE customer_id = @customer_id";

                        cmd = new NpgsqlCommand(customerQuery, con);
                        cmd.Transaction = transaction;

                        if (updatePrescription)
                        {
                            cmd.Parameters.AddWithValue("@prescription", prescription.Text);
                        }

                        cmd.Parameters.AddWithValue("@customer_id", generatedCustomerId);

                        cmd.ExecuteNonQuery();

                        transaction.Commit();

                        MessageBox.Show("Customer information has been updated successfully.");
                    }
                    else
                    {
                        // Handle the case where there's nothing to update
                        MessageBox.Show("No information was updated because no changes were specified.");
                    }
                }
                catch (NpgsqlException ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"An error occurred while updating customer information: {ex.Message}");
                }
            }
        }


        private void update_Click(object sender, RoutedEventArgs e)
        {
            CheckForUpdates();

            if (!string.IsNullOrEmpty(personID.Text) && !string.IsNullOrEmpty(addressID.Text) && !string.IsNullOrEmpty(custID.Text))
            {
                updatePerson(generatedPersonId, updateFirstName, updateLastName, updateEmail, updatePhone, updateBirthDate);
                updateCustAddress(generatedAddressId, updateAddress, updateCity, updatePostalCode);
                updateCustomer(generatedCustomerId, updatePrescription);
            }
            else if (!string.IsNullOrEmpty(personID.Text) && string.IsNullOrEmpty(addressID.Text))
            {
                updatePerson(generatedPersonId, updateFirstName, updateLastName, updateEmail, updatePhone, updateBirthDate);
                AddCustAddressAndCustomerMapping(int.Parse(personID.Text));
            }
            else
            {
                MessageBox.Show("No valid IDs found for updating.");
            }
        }

        private void AddCustAddressAndCustomerMapping(int personId)
        {
            using (NpgsqlTransaction transaction = con.BeginTransaction())
            {
                try
                {
                    string addressQuery = "INSERT INTO optic.address (address, city, postal_code) VALUES (@address, @city, @postal_code) RETURNING address_id";

                    cmd = new NpgsqlCommand(addressQuery, con);
                    cmd.Transaction = transaction;

                    cmd.Parameters.AddWithValue("@address", address.Text);
                    cmd.Parameters.AddWithValue("@city", city.Text);
                    cmd.Parameters.AddWithValue("@postal_code", postalCode.Text);

                    int generatedAddressId = (int)cmd.ExecuteScalar();
                    addressID.Text = generatedAddressId.ToString();

                    string customerQuery = "INSERT INTO optic.customer (address_id, person_id) VALUES (@address_id, @person_id) RETURNING customer_id";
                    cmd = new NpgsqlCommand(customerQuery, con);
                    cmd.Transaction = transaction;

                    cmd.Parameters.AddWithValue("@address_id", generatedAddressId);
                    cmd.Parameters.AddWithValue("@person_id", personId); // Assuming you pass personId as a parameter

                    cmd.ExecuteNonQuery();

                    transaction.Commit();

                    MessageBox.Show($"New address and customer mapping has been added successfully with Address ID: {generatedAddressId}.");

                }
                catch (NpgsqlException ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"An error occurred while adding a new address: {ex.Message}");
                }
            }
        }

        private void appointment_Click(object sender, RoutedEventArgs e)
        {
            Appointments appointments = new Appointments(custID.Text, firstName.Text, lastName.Text);
            appointments.Show();
        }

        private void purch_hist_Click(object sender, RoutedEventArgs e)
        {
            string customerId = custID.Text;
            Orders orders = new Orders(customerId);
            orders.Show();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}