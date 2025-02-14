namespace AmazonOrderLibrary
{
    public class Order
    {
        public string Name { get; }
        public string OrderId { get; }
        public List<string> OrderedItems { get; }
        public string Last4DigitCard { get; }
        public decimal Price { get; }
        public DateTime DateOrdered { get; }
        public bool IsPayed { get; set; } = false;

        public Order(string name, string orderId, List<string> orderedItems, string last4DigitCard, decimal price, DateTime dateOrdered)
        {
            Name = name;
            OrderId = orderId;
            OrderedItems = orderedItems;
            Last4DigitCard = last4DigitCard;
            Price = price;
            DateOrdered = dateOrdered;
        }
        public override bool Equals(object? obj)
        {
            return obj is Order order && OrderId.Equals(order.OrderId);
        }
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
