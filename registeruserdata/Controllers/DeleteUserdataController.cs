using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using registeruserdata.Models;

namespace registeruserdata.Controllers
{
    public class DeleteUserdataController : Controller
    {
        public static HttpClient client = new HttpClient();
        private readonly IConfiguration _configuration;
        public DeleteUserdataController(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new HttpClient() { BaseAddress = new Uri(_configuration.GetSection("webapi").Value) };
    }
        public async Task<IActionResult> DeleteUserdata()
        {
            HttpResponseMessage response = await client.GetAsync("UserData/ID");
            string abc = await response.Content.ReadAsStringAsync();
            List<string> IDs = await response.Content.ReadFromJsonAsync<List<string>>();
            List<SelectListItem> items = new List<SelectListItem>();
            if (response.IsSuccessStatusCode)
            {
                foreach (string ID in IDs)
                {
                    items.Add(new SelectListItem()
                    {
                        Text = ID,
                    });
                }
            }
            ViewBag.Items = items;
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> DeleteUserdata(string ID)
        {
            if (!ModelState.IsValid)
            {
                return View(ID);
            }
            HttpResponseMessage response = await client.DeleteAsync("UserData/"+ ID);
            if (!response.IsSuccessStatusCode)
            {
                return Ok(await response.Content.ReadAsStringAsync());
            }
            await DeleteUserdata();
            ModelState.Clear();
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Verify(string data)
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
                string err = await response.Content.ReadAsStringAsync();
                return Json(err);
            }
            UserDataEdit userData = await response.Content.ReadFromJsonAsync<UserDataEdit>();
            if (userData.position == "教練")
            {
                return Json(userData.ChineseName);
            }
            return Json("position");
        }
    }
}
