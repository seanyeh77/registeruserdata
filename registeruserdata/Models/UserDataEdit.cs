using System.ComponentModel.DataAnnotations;

namespace registeruserdata.Models
{
    public class UserDataEdit
    {
        [Required]
        [DataType(DataType.Text)]
        public string ID { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string ChineseName { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string EnglishName { get; set; }
        public int? grade { get; set; }
        [Required]
        public string gender { get; set; }
        [Required]
        public string position { get; set; }
        [Required]
        [EmailAddressAttribute]
        public string email { get; set; }
        [Required]
        public string view { get; set; }
        //public enum Gender
        //{
        //    男,
        //    女,
        //    其它,
        //}
        //public enum Position
        //{
        //    成員,
        //    教練,
        //    其他,
        //}
    }

}
