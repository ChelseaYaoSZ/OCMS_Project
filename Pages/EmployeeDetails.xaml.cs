using Npgsql;
using NpgsqlTypes;
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
    /// Interaction logic for EmployeeDetails.xaml
    /// </summary>
    public partial class EmployeeDetails : Window
    {
        // Retrieve the selected date from the Date Picker control and format it
        DateTime? selectedDate;
        private NpgsqlConnection con;
        private NpgsqlCommand cmd;

        private int generatedPersonId;
        private int generatedAddressId;
        private int generatedStaffId;

        public EmployeeDetails()
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();
        }

        public EmployeeDetails(DataRow selectedRow)
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();
            FillEmployeeDetails(selectedRow);
        }

        private void FillEmployeeDetails(DataRow row)
        {
            p_ID.Text = $"{row["person_id"]}";
            firstName.Text = $"{row["first_name"]}";
            lastName.Text = $"{row["last_name"]}";
            birthDate.Text = $"{row["birth_date"]}";
            phoneNum.Text = $"{row["phone"]}";
            email.Text = $"{row["email"]}";
            addressID.Text = $"{row["address_id"]}";
            address.Text = $"{row["address"]}";
            city.Text = $"{row["city"]}";
            postalCode.Text = $"{row["postal_code"]}";
            employeeID.Text = $"{row["staff_id"]}";
            username.Text = $"{row["username"]}";
            password.Text = $"{row["password"]}";
            userType.Text = $"{row["user_type"]}";
        }

        private bool updateFirstName = false;
        private bool updateLastName = false;
        private bool updateEmail = false;
        private bool updatePhone = false;
        private bool updateBirthDate = false;
        private bool updateAddress = false;
        private bool updateCity = false;
        private bool updatePostalCode = false;
        private bool updateUserName = false;
        private bool updatePassword = false;
        private bool updateUserType = false;
        private bool updateManagerId = false;
        private bool updateActive = false;

        private int originalStaffId;


        private void add_Click(object sender, RoutedEventArgs e)
        {
            using (NpgsqlTransaction transaction = con.BeginTransaction())
            {
                try
                {
                    selectedDate = birthDate.SelectedDate;

                    if (selectedDate.HasValue)
                    {
                        string newUsername = username.Text.Trim(); // Trim any leading/trailing spaces

                        if (UsernameExists(newUsername))
                        {
                            MessageBox.Show("Error: Username already exists.");
                            return; // Do not proceed with the insertion
                        }

                        // SQL query with parameters for person insertion
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

                        generatedPersonId = (int)cmd.ExecuteScalar();

                        // SQL query with parameters for address insertion
                        string addressQuery = "INSERT INTO optic.address (address, city, postal_code) " +
                                              " VALUES (@address, @city, @postal_code) " +
                                              " RETURNING address_id";

                        cmd = new NpgsqlCommand(addressQuery, con);
                        cmd.Transaction = transaction;

                        // Add values for the parameters in the query
                        cmd.Parameters.AddWithValue("@address", address.Text);
                        cmd.Parameters.AddWithValue("@city", city.Text);
                        cmd.Parameters.AddWithValue("@postal_code", postalCode.Text);

                        generatedAddressId = (int)cmd.ExecuteScalar();

                        // SQL query with parameters for staff insertion
                        string staffQuery = "INSERT INTO optic.staff (address_id, person_id, username, password, user_type, active, manager_staff_id) " +
                                            " VALUES (@address_id, @person_id, @username, @password, @user_type, @active, @manager_staff_id) " +
                                            " RETURNING staff_id";

                        cmd = new NpgsqlCommand(staffQuery, con);
                        cmd.Transaction = transaction;

                        // Add values for the parameters in the query
                        cmd.Parameters.AddWithValue("@person_id", generatedPersonId); // Use the generated person_id
                        cmd.Parameters.AddWithValue("@address_id", generatedAddressId);
                        cmd.Parameters.AddWithValue("@username", newUsername); // Use the sanitized username
                        cmd.Parameters.AddWithValue("@password", password.Text);
                        cmd.Parameters.AddWithValue("@user_type", userType.Text);
                        cmd.Parameters.AddWithValue("@active", activeTrue.IsChecked ?? false); // Default to false if neither checkbox is checked

                        if (int.TryParse(managerID.Text, out int managerId))
                        {
                            cmd.Parameters.AddWithValue("@manager_staff_id", managerId);
                        }
                        else
                        {
                            MessageBox.Show("Error: please enter either 1 or 2");
                            return; // Do not proceed with the insertion
                        }

                        generatedStaffId = (int)cmd.ExecuteScalar();
                        originalStaffId = generatedStaffId; // Store the original staff ID

                        // Execute the query
                        cmd.ExecuteNonQuery();

                        // Commit the transaction if everything succeeds
                        transaction.Commit();

                        // Set the generated person_id to the p_ID TextBox
                        p_ID.Text = generatedPersonId.ToString();
                        addressID.Text = generatedAddressId.ToString();
                        employeeID.Text = generatedStaffId.ToString();

                        MessageBox.Show("Employee's Personal Information, Profile, and Address were added successfully!");
                    }
                    else
                    {
                        MessageBox.Show("Please select a valid date of birth.");
                    }
                }
                catch (NpgsqlException ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
        }

        private bool UsernameExists(string username)
        {
            string query = "SELECT COUNT(*) FROM optic.staff WHERE username = @username";
            using (var checkCmd = new NpgsqlCommand(query, con))
            {
                checkCmd.Parameters.AddWithValue("@username", username);
                int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                return count > 0;
            }
        }

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

            if (!string.IsNullOrWhiteSpace(username.Text))
            {
                updateUserName = true;
            }

            if (!string.IsNullOrWhiteSpace(password.Text))
            {
                updatePassword = true;
            }

            if (!string.IsNullOrWhiteSpace(userType.Text))
            {
                updateUserType = true;
            }

            if (!string.IsNullOrWhiteSpace(managerID.Text))
            {
                updateManagerId = true;
            }

            if (activeTrue.IsChecked == true || activeFalse.IsChecked == true)
            {
                updateActive = true;
            }
        }

        private void updateEmployeeProfile(int originalStaffId, bool updateUserName, bool updatePassword, bool updateUserType, bool updateActive, bool updateManagerId)
        {
            using (NpgsqlTransaction transaction = con.BeginTransaction())
            {
                try
                {
                    string staffQuery = "UPDATE optic.staff SET ";

                    if (updateUserName)
                    {
                        staffQuery += "username = @username, ";
                    }

                    if (updatePassword)
                    {
                        staffQuery += "password = @password, ";
                    }

                    if (updateUserType)
                    {
                        staffQuery += "user_type = @user_type, ";
                    }

                    if (updateActive)
                    {
                        staffQuery += "active = @active, ";
                    }

                    if (updateManagerId)
                    {
                        staffQuery += "manager_staff_id = @manager_staff_id, ";
                    }

                    // Remove the trailing comma and space
                    staffQuery = staffQuery.TrimEnd(',', ' ');

                    staffQuery += " WHERE staff_id = @staff_id";

                    cmd = new NpgsqlCommand(staffQuery, con);
                    cmd.Transaction = transaction;

                    if (updateUserName)
                    {
                        cmd.Parameters.AddWithValue("@username", username.Text);
                    }

                    if (updatePassword)
                    {
                        cmd.Parameters.AddWithValue("@password", password.Text);
                    }

                    if (updateUserType)
                    {
                        cmd.Parameters.AddWithValue("@user_type", userType.Text);
                    }


                    if (updateActive)
                    {
                        bool isActive = activeTrue.IsChecked ?? false;
                        cmd.Parameters.AddWithValue("@active", isActive);
                    }

                    if (int.TryParse(managerID.Text, out int managerId))
                    {
                        cmd.Parameters.AddWithValue("@manager_staff_id", managerId);
                    }

                    cmd.Parameters.AddWithValue("@staff_id", originalStaffId);

                    cmd.ExecuteNonQuery();

                    transaction.Commit();

                    MessageBox.Show("Employee Profile has been updated successfully.");

                }
                catch (NpgsqlException ex)
                {
                    // Roll back the transaction on failure
                    transaction.Rollback();
                    MessageBox.Show($"An error occurred while updating employee profile: {ex.Message}");
                }
            }
        }

        private void updatePerson(int generatedPersonId, bool updateFirstName, bool updateLastName, bool updateEmail, bool updatePhone, bool updateBirthDate)
        {
            using (NpgsqlTransaction transaction = con.BeginTransaction())
            {
                try
                {
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
                        selectedDate = birthDate.SelectedDate;
                        cmd.Parameters.AddWithValue("@birth_date", selectedDate);
                    }

                    cmd.Parameters.AddWithValue("@person_id", generatedPersonId);

                    cmd.ExecuteNonQuery();

                    transaction.Commit();

                    MessageBox.Show("Personal employee information has been updated successfully.");
                }
                catch (NpgsqlException ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"An error occurred while updating customer information: {ex.Message}");
                }
            }
        }

        private void updateEmployeeAddress(int addressId, bool updateAddress, bool updateCity, bool updatePostalCode)
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


        private void update_Click(object sender, RoutedEventArgs e)
        {
            CheckForUpdates();

            if (generatedPersonId != 0 && generatedAddressId != 0 && generatedStaffId != 0)
            {
                updateEmployeeProfile(generatedStaffId, updateUserName, updatePassword, updateUserType, updateActive, updateManagerId); ;
                updatePerson(generatedPersonId, updateFirstName, updateLastName, updateEmail, updatePhone, updateBirthDate);
                updateEmployeeAddress(generatedAddressId, updateAddress, updateCity, updatePostalCode);
            }
            else
            {
                MessageBox.Show("No valid IDs found for updating.");
            }
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
