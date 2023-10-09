using Npgsql;
using System.Data;
using System.Windows;

namespace OCMS
{
    public class DatabaseHelper
    {
        public NpgsqlConnection GetConnection()
        {
            NpgsqlConnection con = new NpgsqlConnection(GetConnectionString());
            con.Open();
            return con;
        }

        private string GetConnectionString()
        {
            string host = "Host=localhost;";
            string port = "Port=5432;";
            string dbName = "Database=optic;";
            string userName = "Username=postgres;";
            string password = "Password=133080;";

            string connectionString = string.Format("{0}{1}{2}{3}{4}", host, port, dbName, userName, password);
            return connectionString;
        }     
    }
}
