using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS.Modules
{
    internal class Response
    {
        public int statusCode { get; set; }
        public string messageCode { get; set; }
        public Inventory inventory { get; set; }
        public List<Inventory> inventories { get; set; }
    }
}
