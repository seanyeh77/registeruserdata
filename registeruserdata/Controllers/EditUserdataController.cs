using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using registeruserdata.Models;

namespace registeruserdata.Controllers
{
    public class EditUserdataController : Controller
    {
        public static HttpClient client = new HttpClient();
        static List<SelectListItem> items = new List<SelectListItem>();
        private readonly IConfiguration _configuration;
        public EditUserdataController(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new HttpClient() { BaseAddress = new Uri(_configuration.GetSection("webapi").Value) };
        }
        // GET: Home
        public ActionResult EditUserdata()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> EditUserdata(UserDataEdit userdata)
        {
            if (!ModelState.IsValid)
            {
                return View();
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
            HttpResponseMessage response = await client.PutAsync("UserData", formdata);
            string str = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest(await response.Content.ReadAsStringAsync());
            }
            ModelState.Clear();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SaveCapture(string data)
        {
            //Convert Base64 Encoded string to Byte Array.
            byte[] imageBytes = Convert.FromBase64String(data.Split(',')[1]);
            MemoryStream stream = new MemoryStream(imageBytes);
            IFormFile file = new FormFile(stream, 0, imageBytes.Length, "image.jpeg", "image.jpeg");
            var formdata = new MultipartFormDataContent();
            formdata.Add(new StreamContent(file.OpenReadStream()), "image", file.FileName);
            HttpResponseMessage response = await client.PostAsync("UserData/detectimg", formdata);
            if (!response.IsSuccessStatusCode)
            {
                return Json(await response.Content.ReadAsStringAsync());
            }
            UserDataEdit userData = await response.Content.ReadFromJsonAsync<UserDataEdit>();
            return Json(userData);
        }
    }
}
