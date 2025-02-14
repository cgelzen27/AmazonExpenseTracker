namespace AmazonOrderLibrary
{
    public class OrderManager
    {
        public List<Order> OrderList { get; }

        // TODO: Make it a singleton principle that gathers data from the database
        public OrderManager(List<Order> orderList)
        {
            OrderList = orderList;
        }
        public void RemovePayedOrder()
        {
            OrderList.RemoveAll(order =>
            {
                return order.IsPayed;
            });
        }
        public decimal CalculateAllOrder(string name, bool pay = false)
        {
            var filter = OrderList.Where(order => order.Name.Equals(name));
            CheckIfPaying(pay, filter);
            return filter.Sum(order => order.Price);
        }

        private void CheckIfPaying(bool pay, IEnumerable<Order> filter)
        {
            if (pay)
            {
                OrderList.ForEach(order =>
                {
                    if (filter.Contains(order))
                    {
                        order.IsPayed = true;
                    }
                });
            }
        }
        public decimal CalculateAllOrderByDate(string name, DateTime fromDate,DateTime toDate, bool pay = false)
        {
            var filter = OrderList.Where(order => order.Name.Equals(name) 
                                                && fromDate.CompareTo(order.DateOrdered) <= 0 
                                                && toDate.CompareTo(order.DateOrdered) >= 0);
            CheckIfPaying(pay, filter);
            return filter.Sum(order => order.Price);
        }
        public List<Order> GetAllOrderByDate(string name, DateTime fromDate, DateTime toDate, bool pay = false)
        {
            var filter = OrderList.Where(order => order.Name.Equals(name)
                                                && fromDate.CompareTo(order.DateOrdered) <= 0
                                                && toDate.CompareTo(order.DateOrdered) >= 0);
            return filter.ToList();
        }

    }
}
