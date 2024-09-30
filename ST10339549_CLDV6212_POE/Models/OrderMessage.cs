namespace ST10339549_CLDV6212_POE.Models
{
    public class OrderMessage
    {
        public string OrderId { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public string Action { get; set; }
        public string Timestamp { get; set; }
    }
}
