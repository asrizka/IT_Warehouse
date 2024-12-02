using IT_Inventory.Data;
using IT_Inventory.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;


namespace IT_Inventory.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext context;


        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            this.context = context; 
        }

        public IActionResult Index()
        {
            var devices = context.ITW_Device
        .GroupBy(d => d.device_name.StartsWith("ApeosPort-V") ? "ApeosPort-V" : d.device_name)
        .Select(g => new
        {
            device_type = g.First().device_type,
            device_name = g.Key, // Use the group key for the device name
            qty_available = g.Sum(d => d.qty_available),
            CategoryName = g.First().ITW_Category.category_name
        })
        .ToList();

            ViewData["Devices"] = devices;

            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
