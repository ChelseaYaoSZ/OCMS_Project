using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace OCMS
{
    internal class AuthenticationForLogin
    {
        private readonly DatabaseHelper databaseHelper;
        private NpgsqlConnection con;

        public AuthenticationForLogin(DatabaseHelper dbHelper)
        {
            databaseHelper = dbHelper;
            con = dbHelper.GetConnection();
        }

        public bool AuthenticateUser(string username, string password, string userType)
        {
            // Define your SQL query to check user authentication.
            string query = "SELECT 1 FROM optic.staff " +
                           "WHERE username = @username " +
                           "AND password = @password " +
                           "AND user_type = @userType";

            using (NpgsqlCommand cmd = new NpgsqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@userType", userType);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    // If the query returns any rows, the user is authenticated.
                    return reader.HasRows;
                }
            }
        }
    }
}



