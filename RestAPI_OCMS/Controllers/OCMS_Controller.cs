using Microsoft.AspNetCore.Mvc;
using Npgsql;
using RestAPI_OCMS.Models;

namespace RestAPI_OCMS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OCMS_Controller : ControllerBase
    {
        private readonly IConfigurationWrapper _configurationWrapper;
        private readonly IDBApplication _dbApplication;

        public OCMS_Controller(IConfigurationWrapper configurationWrapper, IDBApplication dbApplication)
        {
            _configurationWrapper = configurationWrapper;
            _dbApplication = dbApplication;
        }

        //GetAllInventorys API
        [HttpGet]
        [Route("GetAllInventory")]
        public Response GetAllInventory()
        {
            using (NpgsqlConnection con = new NpgsqlConnection(_configurationWrapper.GetConnectionString("inventoryConnection")))
            {
                return _dbApplication.GetAllInventory(con);
            }
        }

        //Search 1 inventory by ID
        [HttpGet]
        [Route("GetInventoryById/{id}")]
        public Response GetInventoryById(int id)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(_configurationWrapper.GetConnectionString("inventoryConnection")))
            {
                return _dbApplication.GetInventoryById(con, id);
            }
        }
    }
}
