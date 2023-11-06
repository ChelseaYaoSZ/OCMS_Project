namespace RestAPI_OCMS.Models
{
    public class Inventory
    {
        public int inventory_id { get; set; }

        public int? lens_id { get; set; }

        public int? frame_id { get; set; }

        public int quantity { get; set; }

        public DateTime last_update { get; set; }
    }
}
