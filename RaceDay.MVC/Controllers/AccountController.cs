using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;
using RaceDay.MVC.ViewModels;

namespace RaceDay.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _api;
        public AccountController(ApiService api) { _api = api; }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                var response = await _api.PostAsync("auth/login", model);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<AuthResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (result?.User != null)
                    {
                        HttpContext.Session.SetString("JwtToken", result.Token);
                        HttpContext.Session.SetString("UserRole", result.User.Role);
                        HttpContext.Session.SetString("UserName", result.User.FullName);
                        HttpContext.Session.SetString("UserEmail", result.User.Email);
                        HttpContext.Session.SetInt32("UserId", result.User.UserID);

                        return result.User.Role == "Organiser"
                            ? RedirectToAction("Dashboard", "Organiser")
                            : RedirectToAction("Index", "Events");
                    }
                }
                ViewBag.Error = "Invalid email or password.";
                return View(model);
            }
            catch
            {
                ViewBag.Error = "We couldn't reach the server. Please try again later.";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                var response = await _api.PostAsync("auth/register", model);
                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Registration successful. Please log in.";
                    return RedirectToAction("Login");
                }
                ViewBag.Error = await ApiService.ErrorAsync(response, "Registration failed. Please check your details.");
                return View(model);
            }
            catch
            {
                ViewBag.Error = "We couldn't reach the server. Please try again later.";
                return View(model);
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}