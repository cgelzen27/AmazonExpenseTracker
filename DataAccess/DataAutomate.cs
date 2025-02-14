using AmazonOrderLibrary;
using Microsoft.Extensions.Configuration;
using PuppeteerExtraSharp.Plugins.ExtraStealth;
using PuppeteerExtraSharp;
using PuppeteerSharp;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace DataAccess;
public class DataAutomate
{
    private Browser? _browser;
    private readonly string _email;
    private readonly string _password;
    private readonly string _url;
    private readonly List<string> _allOrderLinks;
    private readonly List<Order> _orderList;
    private readonly PuppeteerExtra _extra;

    public List<Order> OrderList { get; }
    public DataAutomate(string url)
    {
        _extra = new PuppeteerExtra();

        // Use stealth plugin
        _extra.Use(new StealthPlugin());
        var config = new ConfigurationBuilder().AddUserSecrets<DataAutomate>().Build().GetRequiredSection("MySecret").Get<Credentials>();
        _allOrderLinks= new List<string>();
        _orderList = new List<Order>();
        _email = config.Email;
        _password = config.Password;
        _url = url;
        OrderList = _orderList;
    }
    public async Task Run()
    {
        var orderListPage = await NavigateToUrl();
        
        var pageLinks = await orderListPage.QuerySelectorAllAsync("li.a-normal>a");
        var tasks = new List<Task>
        {
            GetOrderLinks(orderListPage)
        };
        for (int i = 0; i < pageLinks.Length; i++)
        {
            var nextLink = await GetProp(pageLinks[i], "href");
            var nextPage = await BlockImage(await _browser.NewPageAsync());
            await nextPage.GoToAsync(nextLink);
            tasks.Add(GetOrderLinks(nextPage));
        }
        await Task.WhenAll(tasks);
        
        var pages = await _browser.PagesAsync();
        
        for (int i = 0; i < pages.Length; i++)
        {
            if(i < pages.Length - 1)
            {
                await pages[i].CloseAsync();
            }
        }

        tasks.Clear();
        
        foreach (var orderLink in _allOrderLinks)
        {
            tasks.Add(GetAllOrders(orderLink));
        }
        await Task.WhenAll(tasks);
        await _browser.CloseAsync();
    }

    private async Task GetAllOrders(string orderLink)
    {
        var nextPage = await BlockImage(await _browser.NewPageAsync());
        await nextPage.GoToAsync(orderLink);

        string cardNum = (await (await nextPage.QuerySelectorAsync("div.a-spacing-mini>span:nth-child(2)")).GetPropertyAsync("textContent")).RemoteObject.Value.ToString().Replace("**** ", "");

        if (!cardNum.Equals("8787"))
        {
            await nextPage.CloseAsync();
            return;
        }
        
        string name = (await (await nextPage.QuerySelectorAsync("li.displayAddressFullName")).GetPropertyAsync("textContent")).RemoteObject.Value.ToString();
        string orderId = (await (await nextPage.QuerySelectorAsync("bdi[dir=ltr]")).GetPropertyAsync("textContent")).RemoteObject.Value.ToString();

        List<string> orderedItems = new();
        Parallel.ForEach<ElementHandle>(await nextPage.QuerySelectorAllAsync("div.a-fixed-left-grid-col>div.a-row>a.a-link-normal"), async item =>
        {
            orderedItems.Add(string.Join(" ",(await GetProp(item, "textContent")).Split(" ").Take(5)));
        });
        decimal price = decimal.Parse((await (await nextPage.QuerySelectorAsync("div.a-column.a-span5.a-text-right.a-span-last>span.a-text-bold")).GetPropertyAsync("textContent")).RemoteObject.Value.ToString().Replace("$", ""));
        DateTime dateOrdered = DateTime.Parse((await (await nextPage.QuerySelectorAsync("span.order-date-invoice-item")).GetPropertyAsync("textContent")).RemoteObject.Value.ToString().Replace("Ordered on ", ""));

        await nextPage.CloseAsync();
        
        Order order = new(name, orderId, orderedItems, cardNum, price, dateOrdered);

        if (!_orderList.Contains(order))
        {
            _orderList.Add(order);
            Console.WriteLine($"An Order from {order.Name} has been added.");
        }
    }

    private async Task GetOrderLinks(Page page)
    {
        var orders = await page.QuerySelectorAllAsync("div.order-card.js-order-card");

        foreach (var order in orders)
        {
            var orderStat = (await (await order.QuerySelectorAsync("span.a-size-medium.a-text-bold")).GetPropertyAsync("innerText")).RemoteObject.Value.ToString();

            var orderLink = (await (await order.QuerySelectorAsync("a.a-link-normal:first-child")).GetPropertyAsync("href")).RemoteObject.Value.ToString();

            if (!Regex.IsMatch(orderStat, @"^(Return|Refunded)"))
            {
                _allOrderLinks.Add(orderLink);
            }
        }
    }
    private async Task<Page> NavigateToUrl()
    {
        _browser = await _extra.LaunchAsync(new LaunchOptions
        {
            Headless = false,
            ExecutablePath = @"C:\Program Files (x86)\Google\Chrome\Application\Chrome.exe",
            UserDataDir = @"C:\Users\cgelz\AppData\Local\Google\Chrome\User Data\Default"
        });

        var page = await BlockImage(await _browser.NewPageAsync());
        //var page = await _browser.NewPageAsync();
        await page.GoToAsync(_url);
        /*await page.TypeAsync("input[type=email]", _email);
        await page.ClickAsync("input[id=continue]");
        await page.WaitForNavigationAsync();

        await page.TypeAsync("input[type=password]", _password);
        await page.ClickAsync("input[id=signInSubmit]");
        await page.WaitForNavigationAsync();
        */
        return page;
    }

    private static async Task<Page> BlockImage(Page page)
    {
        await page.SetRequestInterceptionAsync(true);
        page.Request += (sender, e) =>
        {
            if (e.Request.ResourceType == ResourceType.Image || e.Request.ResourceType == ResourceType.StyleSheet || e.Request.ResourceType == ResourceType.Font)
            {
                e.Request.AbortAsync();
            }
            else
            {
                e.Request.ContinueAsync();
            }
        };
        return page;
    }

    private static async Task<string> GetProp(ElementHandle elementHandle, string property)
    {
        return (await elementHandle.GetPropertyAsync(property)).ToString().Replace("JSHandle:", "").Trim();
    }
}
