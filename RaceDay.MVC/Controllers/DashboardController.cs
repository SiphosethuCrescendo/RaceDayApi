using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Core.Entities;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers
{
    public class OrganiserDashboardController : Controller
    {
        private readonly ApiService _api;

        public OrganiserDashboardController(ApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Account");

            var events = await _api.GetAsync<List<Event>>("events");
            return View(events ?? new List<Event>());
        }
    }
}