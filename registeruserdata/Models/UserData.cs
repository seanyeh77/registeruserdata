using System.ComponentModel.DataAnnotations;

namespace registeruserdata.Models
{
    public class UserData
    {
        [Required(ErrorMessage = "不可輸入為空白")]
        [DataType(DataType.Text)]
        [RegularExpression("\\d\\d\\d\\d\\d\\d", ErrorMessage = "請輸入六位數字")]
        public string ID { get; set; }
        [Required(ErrorMessage = "不可輸入為空白")]
        [DataType(DataType.Text)]
        public string ChineseName { get; set; }
        [Required(ErrorMessage = "不可輸入為空白")]
        [DataType(DataType.Text)]
        public string EnglishName { get; set; }
        public int? grade { get; set; }
        [Required(ErrorMessage = "不可輸入為空白")]
        public string gender { get; set; }
        [Required(ErrorMessage = "不可輸入為空白")]
        public string position { get; set; }
        [Required(ErrorMessage = "不可輸入為空白")]
        [EmailAddressAttribute]
        public string email { get; set; }
        [Required(ErrorMessage = "不可輸入為空白")]
        public string view { get; set; }
        [Required(ErrorMessage = "請上傳檔案")]
        [DataType(DataType.Upload)]
        [MaxFileSize(1 * 1024 * 1024, ErrorMessage = "檔案大小不能超過1MB")]
        [AllowedExtensions(new string[] { ".jpg", ".png", ".jpeg"}, ErrorMessage = "檔案類型只能為jpg,png,jpeg")]
        public List<IFormFile> Image { get; set; }
    }
    public enum Gender
    {
        男,
        女,
        其它,
    }
    public enum Position
    {
        學生,
        老師,
        其他,
    }
}
public class AllowedExtensionsAttribute : ValidationAttribute
{
    private readonly string[] _extensions;
    public AllowedExtensionsAttribute(string[] extensions)
    {
        _extensions = extensions;
    }

    protected override ValidationResult IsValid(
    object value, ValidationContext validationContext)
    {
        var files = value as List<IFormFile>;
        if (files != null)
        {
            foreach(var file in files) {
                var extension = Path.GetExtension(file.FileName);
                if (!_extensions.Contains(extension.ToLower()))
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }
        }

        return ValidationResult.Success;
    }

    public string GetErrorMessage()
    {
        return $"只支援bmp、png、jpeg、gif類型的檔案";
    }
}
public class MaxFileSizeAttribute : ValidationAttribute
{
    private readonly int _maxFileSize;
    public MaxFileSizeAttribute(int maxFileSize)
    {
        _maxFileSize = maxFileSize;
    }

    protected override ValidationResult IsValid(
    object value, ValidationContext validationContext)
    {
        var file = value as IFormFile;
        if (file != null)
        {
            if (file.Length > _maxFileSize)
            {
                return new ValidationResult(GetErrorMessage());
            }
        }

        return ValidationResult.Success;
    }

    public string GetErrorMessage()
    {
        return $"Maximum allowed file size is {_maxFileSize} bytes.";
    }
}