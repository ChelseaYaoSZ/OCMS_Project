using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCMS.Modules
{
    internal class Inventory
    {
        public int inventory_id { get; set; }

        public int? lens_id { get; set; }

        public int? frame_id { get; set; }

        public int quantity { get; set; }

        public DateTime last_update { get; set; }
    }
}
