using Microsoft.AspNetCore.Mvc;
using Npgsql;
using RestAPI_OCMS.Models;

namespace RestAPI_OCMS.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OCMS_Controller : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public OCMS_Controller(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //GetAllInventorys API
        [HttpGet]
        [Route("GetAllInventory")]

        //Create API Method
        public Response GetAllInventory()
        {
            Response response = new Response();

            NpgsqlConnection con =
                new NpgsqlConnection(_configuration.GetConnectionString("inventoryConnection"));

            DBApplication dBA = new DBApplication();
            response = dBA.GetAllInventory(con);

            return response;
        }

        //Search 1 inventory by ID
        [HttpGet]
        [Route("GetInventoryById/{id}")]

        public Response GetInventoryById(int id)
        {
            Response response = new Response();
            NpgsqlConnection con =
                new NpgsqlConnection(_configuration.GetConnectionString("inventoryConnection"));
            DBApplication dBA = new DBApplication();
            response = dBA.GetInventoryById(con, id);
            return response;
        }
    }
}
