using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;
using RaceDay.MVC.ViewModels;

namespace RaceDay.MVC.Controllers
{
    [SessionAuthorize("Participant")]
    public class ParticipantController : Controller
    {
        private readonly ApiService _api;
        public ParticipantController(ApiService api) { _api = api; }

        public async Task<IActionResult> MyEnrolments() =>
            View(await _api.GetAsync<List<EnrolmentModel>>("enrolments/my") ?? new());

        public async Task<IActionResult> MyResults()
        {
            var results = await _api.GetAsync<List<ResultModel>>("results/my") ?? new();
            var rows = new List<ResultRow>();

            foreach (var r in results)
            {
                var ev = r.Enrolment?.Category?.Event;
                if (ev == null) continue;

                var all = await _api.GetAsync<List<ResultModel>>($"results/event/{ev.EventID}") ?? new();

                rows.Add(new ResultRow
                {
                    EventName = ev.EventName, 
                    EventDate = ev.EventDate,
                    Category = r.Enrolment?.Category?.CategoryName ?? "Unknown",
                    FinishTime = r.FinishTime ?? "",
                    Position = r.Position,
                    TotalFinishers = all.Count
                });
            }

            return View(rows.OrderByDescending(x => x.EventDate).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _api.GetAsync<UserModel>("users/profile");
            if (user == null) { TempData["Error"] = "Your profile could not be loaded."; return RedirectToAction("Index", "Events"); }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(string fullName, string? phoneNumber, IFormFile? picture)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                TempData["Error"] = "Full name is required.";
                return RedirectToAction(nameof(Profile));
            }
            var r = await _api.PutAsync("users/profile", new { fullName, phoneNumber });
            if (!r.IsSuccessStatusCode) { TempData["Error"] = await ApiService.ErrorAsync(r); return RedirectToAction(nameof(Profile)); }

            if (picture is { Length: > 0 })
            {
                if (!picture.ContentType.StartsWith("image/")) { TempData["Error"] = "The profile picture must be an image."; return RedirectToAction(nameof(Profile)); }
                var url = await _api.UploadFileAsync("users/profile-picture", picture, "imageFile");
                if (url == null) { TempData["Error"] = "The picture could not be uploaded."; return RedirectToAction(nameof(Profile)); }
            }
            HttpContext.Session.SetString("UserName", fullName);
            TempData["Success"] = "Profile updated.";
            return RedirectToAction(nameof(Profile));
        }
    }
}