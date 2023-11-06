using Newtonsoft.Json;
using OCMS.Modules;
using System;
using System.Net.Http;
using System.Windows;

namespace OCMS
{
    /// <summary>
    /// Interaction logic for InventoryWithRestAPI.xaml
    /// </summary>
    public partial class InventoryWithRestAPI : Window
    {
        HttpClient client = new HttpClient();
        public InventoryWithRestAPI()
        {
            client.BaseAddress = new Uri("https://localhost:7119/OCMS_/");

            //Step 02: we need to clear the default network packet header informaiton
            client.DefaultRequestHeaders.Accept.Clear();

            // Step 03: Add our packet information to the http request
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")

                );

            InitializeComponent();
            LoadAllInventory();
        }

        public async void LoadAllInventory()
        {
            //Step 01: Create/ Call the method to get all the products
            var server_response = await client.GetStringAsync("GetAllInventory");

            // step 02: Decrypt/extract the server_response
            Response response_JSON = JsonConvert.DeserializeObject<Response>(server_response);

            dataGridInventory.ItemsSource = response_JSON.inventories;
            DataContext = this;
        }

        private async void search_Click(object sender, RoutedEventArgs e)
        {
            // Step 01: Create/ Call the method to search out one student from the database
            var server_response =
                await client.GetStringAsync("GetInventoryById/" + int.Parse(searchInfo.Text));

            Response response_JSON = JsonConvert.DeserializeObject<Response>(server_response);

            dataGridInventory.ItemsSource = response_JSON.inventories;
            DataContext = this;
        }
    }

}
