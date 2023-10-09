﻿using Npgsql;
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
    /// Interaction logic for CustomerInfoPage.xaml
    /// </summary>
    public partial class CustomerInfoPage : Window
    {
        private NpgsqlConnection con;

        public CustomerInfoPage()
        {
            InitializeComponent();
            DatabaseHelper dbHelper = new DatabaseHelper();
            con = dbHelper.GetConnection();
            LoadAllCustomers();
            this.Closed += CustomerInfoPage_Closed;
        }

        public DataTable GetAllCustomers()
        {
            string query = @"SELECT p.first_name, p.last_name, p.birth_date, p.phone, p.email, a.address 
                                FROM optic.customer c
                                LEFT JOIN optic.person p on p.person_id = c.person_id
                                LEFT JOIN optic.address a on a.address_id = c.address_id";

            NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(query, con);

            DataTable dataTable = new DataTable();

            dataAdapter.Fill(dataTable);

            return dataTable;
        }

        public void LoadAllCustomers()
        {
            DataTable customers = GetAllCustomers();
            dataGridCustomers.ItemsSource = customers.DefaultView;
        }

        public DataTable SearchCustomers(string searchTerm)
        {
            string query = @"SELECT p.first_name, p.last_name, p.birth_date, p.phone, p.email, a.address 
                            FROM optic.customer c
                            Left JOIN optic.person p on p.person_id = c.person_id 
                            Left JOIN optic.address a on a.address_id = c.address_id 
                            WHERE p.first_name ILIKE @SearchTerm 
                             OR p.phone ILIKE @SearchTerm 
                             OR p.email ILIKE @SearchTerm";

            NpgsqlDataAdapter dataAdapter = new NpgsqlDataAdapter(query, con);
            dataAdapter.SelectCommand.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
            DataTable dataTable = new DataTable();
            dataAdapter.Fill(dataTable);
            return dataTable;
        }

        public void LoadSpecificCustomer()
        {
            string searchTerm = searchInfo.Text;
            DataTable specificCustomer = SearchCustomers(searchTerm);
            dataGridCustomers.ItemsSource = specificCustomer.DefaultView;
        }

        private void search_Click(object sender, RoutedEventArgs e)
        {
            LoadSpecificCustomer();
        }

        private void CustomerInfoPage_Closed(object sender, EventArgs e)
        {
            if (con != null)
            {
                con.Close();
                con.Dispose();
            }
        }
        
    }
}
