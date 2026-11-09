using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;
using RaceDay.MVC.ViewModels;

namespace RaceDay.MVC.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApiService _api;
        public EventsController(ApiService api) { _api = api; }

        public async Task<IActionResult> Index()
        {
            var events = await _api.GetAsync<List<EventModel>>("events");
            if (events == null) { ViewBag.Error = "We couldn't load events right now."; events = new(); }
            return View(events.OrderBy(e => e.EventDate).ToList());
        }

        public async Task<IActionResult> Details(int id)
        {
            var ev = await _api.GetAsync<EventModel>($"events/{id}");
            if (ev == null) { TempData["Error"] = "That event could not be found."; return RedirectToAction(nameof(Index)); }
            ev.Categories = await _api.GetAsync<List<CategoryModel>>($"categories/event/{id}") ?? new();
            return View(ev);
        }

        [HttpPost]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> Enrol(int categoryId, int eventId, string tShirtSize, string emergencyContactName, string emergencyContactNumber, string? medicalConditions)
        {
            var dto = new { categoryID = categoryId, tShirtSize = tShirtSize, emergencyContactName = emergencyContactName, emergencyContactNumber = emergencyContactNumber, medicalConditions = medicalConditions ?? "" };
            var response = await _api.PostAsync("enrolments", dto);
            if (response.IsSuccessStatusCode) { TempData["Success"] = "You are enrolled!"; return RedirectToAction("MyEnrolments", "Participant"); }
            TempData["Error"] = await ApiService.ErrorAsync(response, "We couldn't complete your enrolment.");
            return RedirectToAction("Details", new { id = eventId });
        }
    }
}