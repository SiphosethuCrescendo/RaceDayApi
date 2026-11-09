using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;
using RaceDay.MVC.ViewModels;

namespace RaceDay.MVC.Controllers
{
    [SessionAuthorize("Organiser")]
    public class OrganiserController : Controller
    {
        private readonly ApiService _api;
        public OrganiserController(ApiService api) { _api = api; }
        private int UserId => HttpContext.Session.GetInt32("UserId") ?? 0;

        private async Task<List<EventModel>> MyEventsAsync()
        {
            var all = await _api.GetAsync<List<EventModel>>("events") ?? new();
            return all.Where(e => e.OrganiserID == UserId).OrderBy(e => e.EventDate).ToList();
        }

        public async Task<IActionResult> Dashboard()
        {
            var rows = new List<DashboardRow>();
            foreach (var e in await MyEventsAsync())
            {
                var enr = await _api.GetAsync<List<EnrolmentModel>>($"enrolments/event/{e.EventID}") ?? new();
                rows.Add(new DashboardRow { Event = e, Enrolments = enr.Count(x => x.Status != "Cancelled") });
            }
            return View(rows);
        }

        public async Task<IActionResult> Events() => View(await MyEventsAsync());

        [HttpGet] public IActionResult CreateEvent() => View("EventForm", new EventFormViewModel());

        [HttpPost]
        public async Task<IActionResult> CreateEvent(EventFormViewModel vm, IFormFile? banner)
        {
            if (!ModelState.IsValid) return View("EventForm", vm);
            if (!await ApplyBanner(vm, banner)) return View("EventForm", vm);

            var r = await _api.PostAsync("events", Payload(vm));
            if (!r.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", await ApiService.ErrorAsync(r, "The event could not be created."));
                return View("EventForm", vm);
            }
            TempData["Success"] = "Event created.";
            return RedirectToAction(nameof(Events));
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var e = await _api.GetAsync<EventModel>($"events/{id}");
            if (e == null || e.OrganiserID != UserId) { TempData["Error"] = "Event not found."; return RedirectToAction(nameof(Events)); }

            return View("EventForm", new EventFormViewModel
            {
                EventID = e.EventID,
                Name = e.EventName,
                Description = e.Description,
                EventDate = e.EventDate,
                Location = e.Location,
                Distance = e.Distance,
                EventType = e.EventType,
                ImageUrl = e.ImageUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditEvent(EventFormViewModel vm, IFormFile? banner)
        {
            if (!ModelState.IsValid) return View("EventForm", vm);
            if (!await ApplyBanner(vm, banner)) return View("EventForm", vm);

            var r = await _api.PutAsync($"events/{vm.EventID}", Payload(vm));
            if (!r.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", await ApiService.ErrorAsync(r, "The event could not be updated."));
                return View("EventForm", vm);
            }
            TempData["Success"] = "Event updated.";
            return RedirectToAction(nameof(Events));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var r = await _api.DeleteAsync($"events/{id}");
            if (r.IsSuccessStatusCode) TempData["Success"] = "Event deleted.";
            else TempData["Error"] = await ApiService.ErrorAsync(r, "The event could not be deleted.");
            return RedirectToAction(nameof(Events));
        }

        // ---------- Categories ----------
        public async Task<IActionResult> Categories(int eventId)
        {
            var ev = await _api.GetAsync<EventModel>($"events/{eventId}");
            if (ev == null) { TempData["Error"] = "Event not found."; return RedirectToAction(nameof(Events)); }
            ViewBag.EventId = eventId;
            ViewBag.EventName = ev.EventName;
            return View(await _api.GetAsync<List<CategoryModel>>($"categories/event/{eventId}") ?? new());
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(int eventId, string name, string categoryType)
        {
            if (string.IsNullOrWhiteSpace(name)) { TempData["Error"] = "Enter a category name."; return RedirectToAction(nameof(Categories), new { eventId }); }
            var r = await _api.PostAsync("categories", new { eventID = eventId, categoryName = name });
            if (r.IsSuccessStatusCode) TempData["Success"] = "Category added.";
            else TempData["Error"] = await ApiService.ErrorAsync(r, "The category could not be added.");
            return RedirectToAction(nameof(Categories), new { eventId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id, int eventId)
        {
            var r = await _api.DeleteAsync($"categories/{id}");
            if (r.IsSuccessStatusCode) TempData["Success"] = "Category removed.";
            else TempData["Error"] = "The category could not be removed.";
            return RedirectToAction(nameof(Categories), new { eventId });
        }

        // ---------- Enrolments ----------
        public async Task<IActionResult> Enrolments(int eventId)
        {
            ViewBag.EventId = eventId;
            return View(await _api.GetAsync<List<EnrolmentModel>>($"enrolments/event/{eventId}") ?? new());
        }

        [HttpPost]
        public async Task<IActionResult> SetStatus(int id, int eventId, string status)
        {
            var r = await _api.PutAsync($"enrolments/{id}/status", status);
            if (r.IsSuccessStatusCode) TempData["Success"] = "Status updated.";
            else TempData["Error"] = await ApiService.ErrorAsync(r);
            return RedirectToAction(nameof(Enrolments), new { eventId });
        }

        // ---------- Results ----------
        public async Task<IActionResult> Results(int eventId)
        {
            ViewBag.EventId = eventId;
            var list = await _api.GetAsync<List<EnrolmentModel>>($"enrolments/event/{eventId}") ?? new();
            return View(list.Where(e => e.Status == "Confirmed" || e.Status == "Completed").ToList());
        }

        [HttpPost]
        public async Task<IActionResult> SaveResult(int eventId, int enrolmentId, string finishTime, int position)
        {
            if (!TimeSpan.TryParse(finishTime, out _) || position < 1)
            {
                TempData["Error"] = "Enter a valid finish time (hh:mm:ss) and a position of 1 or more.";
                return RedirectToAction(nameof(Results), new { eventId });
            }
            var r = await _api.PostAsync("results", new { enrolmentID = enrolmentId, finishTime, position });
            if (r.IsSuccessStatusCode) TempData["Success"] = "Result published.";
            else TempData["Error"] = await ApiService.ErrorAsync(r);
            return RedirectToAction(nameof(Results), new { eventId });
        }

        // ---------- helpers ----------
        private async Task<bool> ApplyBanner(EventFormViewModel vm, IFormFile? banner)
        {
            if (banner == null || banner.Length == 0) return true;
            if (!banner.ContentType.StartsWith("image/")) { ModelState.AddModelError("", "The banner must be an image file."); return false; }
            var url = await _api.UploadFileAsync("events/upload-image", banner, "imageFile");
            if (url == null) { ModelState.AddModelError("", "The banner could not be uploaded."); return false; }
            vm.ImageUrl = url;
            return true;
        }

        private static object Payload(EventFormViewModel v) =>
            new { eventName = v.Name, description = v.Description, eventDate = v.EventDate, location = v.Location, imageUrl = v.ImageUrl };
    }
}