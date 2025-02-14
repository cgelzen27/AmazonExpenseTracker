using AmazonOrderLibrary;
using DataAccess;


DataAutomate data = new DataAutomate("https://www.amazon.com/gp/your-account/order-history?orderFilter=year-2023&ref_=ppx_yo2ov_dt_b_filter_all_y2023");
await data.Run();
OrderManager manager = new OrderManager(data.OrderList);

/*foreach (var order in manager.OrderList)
{
    if(!order.Name.Equals("Coleen crisanto"))
    {
        continue;
    }
    Console.WriteLine($"{order.OrderId} = ${order.Price}");
}*/
//Me
foreach (var item in manager.GetAllOrderByDate("Gelzen Crisanto", new DateTime(2023, 04, 10), new DateTime(2023, 08, 01)))
{
    Console.WriteLine($"{item.OrderId} ({item.DateOrdered} = ${item.Price})");
}

Console.WriteLine($"Gelzen Total = {manager.CalculateAllOrderByDate("Gelzen Crisanto", new DateTime(2023, 04, 10), new DateTime(2023, 08, 01))}");
Console.WriteLine("----------------------------------------------------------");
//Bobie
foreach (var item in manager.GetAllOrderByDate("Coleen crisanto", new DateTime(2023, 04, 10), new DateTime(2023, 08, 01)))
{
    Console.WriteLine($"{item.OrderId} ({item.DateOrdered} = ${item.Price})");
}

Console.WriteLine($"Coleen Total = {manager.CalculateAllOrderByDate("Coleen crisanto", new DateTime(2023, 04, 10), new DateTime(2023, 08, 01))}");
Console.WriteLine("----------------------------------------------------------");
// MOM
foreach (var item in manager.GetAllOrderByDate("Armie Crisanto", new DateTime(2023, 04, 10), new DateTime(2023, 08, 01)))
{
    Console.WriteLine($"{item.OrderId} ({item.DateOrdered} = ${item.Price})");
}

Console.WriteLine($"Armie Total = {manager.CalculateAllOrderByDate("Armie Crisanto", new DateTime(2023, 04, 10), new DateTime(2023, 08, 01))}");
Console.WriteLine("----------------------------------------------------------");
// ATE JHO
foreach (var item in manager.GetAllOrderByDate("Jowarlie mamerto", new DateTime(2023, 04, 10), new DateTime(2023, 08, 01)))
{
    Console.WriteLine($"{item.OrderId} ({item.DateOrdered} = ${item.Price})");
}
Console.WriteLine($"Jowarlie Total = {manager.CalculateAllOrderByDate("Jowarlie mamerto", new DateTime(2023, 04, 10), new DateTime(2023, 08, 01))}");
