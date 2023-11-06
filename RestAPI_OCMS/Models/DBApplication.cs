using Npgsql;
using System.Data;

namespace RestAPI_OCMS.Models
{
    public class DBApplication
    {
        public Response GetAllInventory(NpgsqlConnection con)
        {
            string Query = "Select * from optic.inventory";
            NpgsqlDataAdapter da = new NpgsqlDataAdapter(Query, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            Response response = new Response();

            List<Inventory> inventories = new List<Inventory>();

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Inventory inventory = new Inventory();
                    inventory.inventory_id = (int)dt.Rows[i]["inventory_id"];
                    inventory.lens_id = dt.Rows[i]["lens_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["lens_id"]) : (int?)null;
                    inventory.frame_id = dt.Rows[i]["frame_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[i]["frame_id"]) : (int?)null;
                    inventory.quantity = (int)dt.Rows[i]["quantity"];
                    inventory.last_update = (DateTime)dt.Rows[i]["last_update"];

                    inventories.Add(inventory);
                }
            }

            if (inventories.Count > 0)
            {
                response.statusCode = 200;
                response.messageCode = "Data Retrieved Successfully";
                response.inventory = null;
                response.inventories = inventories;
            }
            else
            {
                response.statusCode = 100;
                response.messageCode = "Data failed to retrieve or table is empty";
                response.inventory = null;
                response.inventories = null;
            }

            return response;
        }

        //Get inventory by id method
        public Response GetInventoryById(NpgsqlConnection con, int id)
        {
            Response response = new Response();
            string Query = "Select * from optic.inventory where inventory_id='" + id + "'"; //inline param with query

            NpgsqlDataAdapter da = new NpgsqlDataAdapter(Query, con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            List<Inventory> inventories = new List<Inventory>();

            if (dt.Rows.Count > 0)
            {
                Inventory inventory = new Inventory();
                inventory.inventory_id = (int)dt.Rows[0]["inventory_id"];
                inventory.lens_id = dt.Rows[0]["lens_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["lens_id"]) : (int?)null;
                inventory.frame_id = dt.Rows[0]["frame_id"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["frame_id"]) : (int?)null;
                inventory.quantity = (int)dt.Rows[0]["quantity"];
                inventory.last_update = (DateTime)dt.Rows[0]["last_update"];
                inventories.Add(inventory);

                response.statusCode = 200;
                response.messageCode = "Inventory Successfully Retrieved";
                response.inventory = inventory;
                response.inventories = inventories;
            }
            else
            {
                response.statusCode = 100;
                response.messageCode = "Data couldn't be found...check the id";
                response.inventory = null;
                response.inventories = null;
            }

            return response;

        }
    }
}
