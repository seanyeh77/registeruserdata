using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using registeruserdata.Models;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace registeruserdata.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
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
                    ViewBag.Message= ModelState.ToString();
                    return View();
                }
                List<MemoryStream> MemoryStream_img = new List<MemoryStream>();
                foreach (IFormFile img in userdata.Image)
                {
                    MemoryStream memoryStream = new MemoryStream();
                    var k = GetPicThumbnail(img.OpenReadStream(), 0, 0, 70, memoryStream);
                    if (!k)
                    {
                         ViewBag.Message = "照片壓縮失敗";
                    }
                    MemoryStream_img.Add(memoryStream);
                }
                // The uploaded image corresponds to our business rules => process it
                var formdata = new MultipartFormDataContent();
                formdata.Add(new StringContent(userdata.ID.ToString()), "ID");
                formdata.Add(new StringContent(userdata.ChineseName), "ChineseName");
                formdata.Add(new StringContent(userdata.EnglishName), "EnglishName");
                formdata.Add(new StringContent(userdata.gender), "gender");
                formdata.Add(new StringContent(userdata.grade.ToString()), "grade");
                formdata.Add(new StringContent(userdata.position), "position");
                formdata.Add(new StringContent(userdata.email), "email");
                formdata.Add(new StringContent(userdata.view), "view");
                foreach (MemoryStream img in MemoryStream_img)
                {
                    formdata.Add(new ByteArrayContent(img.ToArray()), "Image", "img");
                }
                HttpResponseMessage response = await client.PostAsync("UserData", formdata);
                if (!response.IsSuccessStatusCode)
                {
                    string err = await response.Content.ReadAsStringAsync();
                    string _message = "";
                    switch (err)
                    {
                        case "facenull":
                            _message = "請換照片再試";
                            break;
                        case "isID":
                            _message = "已經有此ID";
                            break;
                        case "lock":
                            _message = "已經有此ID，而且已被鎖定";
                            break;
                        case "nullimg":
                            _message = "照片傳送失敗";
                            break;
                        default:
                            _message = err;
                            break;
                    }
                    ViewBag.Message = _message;
                    return View();
                }
                ViewBag.Message = "成功新增";
                ModelState.Clear();
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex;
            }
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        private bool GetPicThumbnail(Stream stream, int dHeight, int dWidth, int flag, Stream outstream)
        {
            //可以直接从流里边得到图片,这样就可以不先存储一份了
            System.Drawing.Image iSource = System.Drawing.Image.FromStream(stream);

            //如果为参数为0就保持原图片
            if (dHeight == 0)
            {
                dHeight = iSource.Height;
            }
            if (dWidth == 0)
            {
                dWidth = iSource.Width;
            }


            ImageFormat tFormat = iSource.RawFormat;
            int sW = 0, sH = 0;

            //按比例缩放
            Size tem_size = new Size(iSource.Width, iSource.Height);

            if (tem_size.Width > dHeight || tem_size.Width > dWidth)
            {
                if ((tem_size.Width * dHeight) > (tem_size.Width * dWidth))
                {
                    sW = dWidth;
                    sH = (dWidth * tem_size.Height) / tem_size.Width;
                }
                else
                {
                    sH = dHeight;
                    sW = (tem_size.Width * dHeight) / tem_size.Height;
                }
            }
            else
            {
                sW = tem_size.Width;
                sH = tem_size.Height;
            }

            Bitmap ob = new Bitmap(dWidth, dHeight);
            Graphics g = Graphics.FromImage(ob);

            g.Clear(Color.WhiteSmoke);
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            g.DrawImage(iSource, new Rectangle((dWidth - sW) / 2, (dHeight - sH) / 2, sW, sH), 0, 0, iSource.Width, iSource.Height, GraphicsUnit.Pixel);

            g.Dispose();
            //以下代码为保存图片时，设置压缩质量 
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1];
            qy[0] = flag;//设置压缩的比例1-100 
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;
            try
            {
                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo jpegICIinfo = null;
                for (int x = 0; x < arrayICI.Length; x++)
                {
                    if (arrayICI[x].FormatDescription.Equals("JPEG"))
                    {
                        jpegICIinfo = arrayICI[x];
                        break;
                    }
                }
                if (jpegICIinfo != null)
                {
                    //可以存储在流里边;
                    ob.Save(outstream, jpegICIinfo, ep);

                }
                else
                {
                    ob.Save(outstream, tFormat);
                }


                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                iSource.Dispose();
                ob.Dispose();
            }
        }
    }
}