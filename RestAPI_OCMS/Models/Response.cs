namespace RestAPI_OCMS.Models
{
    public class Response
    {
        public int statusCode { get; set; }
        public string messageCode { get; set; }
        public Inventory inventory { get; set; }
        public List<Inventory> inventories { get; set; }
    }
}
