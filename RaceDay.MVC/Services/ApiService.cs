using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaceDay.MVC.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContext;
        private readonly string _baseUrl;

        // THE FIX: Make it completely case-insensitive and ignore nulls
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ApiService(HttpClient http, IHttpContextAccessor httpContext, IConfiguration config)
        {
            _http = http;
            _httpContext = httpContext;
            string rawUrl = config["ApiSettings:BaseUrl"] ?? "http://localhost:5253/api/";
            _baseUrl = rawUrl.TrimEnd('/') + "/";
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var token = _httpContext.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _http.SendAsync(request);
        }

        private HttpRequestMessage Build(HttpMethod method, string endpoint, object? data = null)
        {
            var req = new HttpRequestMessage(method, _baseUrl + endpoint.TrimStart('/'));
            if (data != null)
                req.Content = new StringContent(JsonSerializer.Serialize(data, JsonOpts), Encoding.UTF8, "application/json");
            return req;
        }

        public Task<HttpResponseMessage> PostAsync(string endpoint, object data) => SendAsync(Build(HttpMethod.Post, endpoint, data));
        public Task<HttpResponseMessage> PutAsync(string endpoint, object data) => SendAsync(Build(HttpMethod.Put, endpoint, data));
        public Task<HttpResponseMessage> DeleteAsync(string endpoint) => SendAsync(Build(HttpMethod.Delete, endpoint));

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await SendAsync(Build(HttpMethod.Get, endpoint));
                if (!response.IsSuccessStatusCode) return default;
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, JsonOpts);
            }
            catch { return default; }
        }

        public async Task<string?> UploadFileAsync(string endpoint, IFormFile file, string fieldName)
        {
            try
            {
                using var form = new MultipartFormDataContent();
                using var stream = file.OpenReadStream();
                var content = new StreamContent(stream);
                content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                form.Add(content, fieldName, file.FileName);

                var req = new HttpRequestMessage(HttpMethod.Post, _baseUrl + endpoint.TrimStart('/')) { Content = form };
                var response = await SendAsync(req);
                if (!response.IsSuccessStatusCode) return null;

                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                return doc.RootElement.GetProperty("url").GetString();
            }
            catch { return null; }
        }

        public static async Task<string> ErrorAsync(HttpResponseMessage r, string fallback = "Something went wrong. Please try again.")
        {
            if (r.StatusCode == System.Net.HttpStatusCode.Unauthorized) return fallback;
            if (r.StatusCode == System.Net.HttpStatusCode.Forbidden) return "You do not have permission to do that.";
            try
            {
                using var doc = JsonDocument.Parse(await r.Content.ReadAsStringAsync());
                if (doc.RootElement.TryGetProperty("message", out var m) && !string.IsNullOrWhiteSpace(m.GetString()))
                    return m.GetString()!;
            }
            catch { }
            return fallback;
        }
    }
}