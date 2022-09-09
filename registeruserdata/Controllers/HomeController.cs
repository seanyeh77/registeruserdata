using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using registeruserdata.Models;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace registeruserdata.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        //public static HttpClient client = new HttpClient() { BaseAddress = new Uri("https://localhost:7021/") };
        public static HttpClient client = new HttpClient();
        public static List<UserData> result = new List<UserData>();
        public static UserData userdata = new UserData();
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            client = new HttpClient() { BaseAddress = new Uri(_configuration.GetSection("webapi").Value) };
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Index(UserData userdata)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(userdata);
                }
                // The uploaded image corresponds to our business rules => process it
                var formdata = new MultipartFormDataContent();
                formdata.Add(new StringContent(userdata.ID.ToString()), "userdata.ID");
                formdata.Add(new StringContent(userdata.ChineseName), "userdata.ChineseName");
                formdata.Add(new StringContent(userdata.EnglishName), "userdata.EnglishName");
                formdata.Add(new StringContent(userdata.gender), "userdata.gender");
                formdata.Add(new StringContent(userdata.grade.ToString()), "userdata.grade");
                formdata.Add(new StringContent(userdata.position), "userdata.position");
                formdata.Add(new StringContent(userdata.email), "userdata.email");
                formdata.Add(new StringContent(userdata.view), "userdata.view");
                foreach (IFormFile img in userdata.Image)
                {
                    formdata.Add(new StreamContent(img.OpenReadStream()), "userdata.Image", img.FileName);
                }

                HttpResponseMessage response = await client.PostAsync("UserData", formdata);
                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    switch (error)
                    {
                        case "facenull":
                            return BadRequest("請換照片再試");
                        case "isID":
                            return BadRequest("已經有此ID");
                        case "freeze":
                            return BadRequest("已經有此ID，而且已被凍結");
                        default:
                            break;
                    }
                    return BadRequest();
                }
                ModelState.Clear();
            }
            catch
            {
                return View();
            }
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}