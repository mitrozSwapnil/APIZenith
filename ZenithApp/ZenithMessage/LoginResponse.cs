using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenithApp.ZenithMessage
{
    public class LoginResponse:BaseResponse
    {
        public int code { get; set; }
        public LoginData data { get; set; }
    }
    public class LoginData
    {
        public string token { get; set; }
        public String userId { get; set; }
        public string? fullName { get; set; }
        public string? Role { get; set; }
        public string? email { get; set; }
        public string? mobileNo { get; set; }
        public string? Department { get; set; }
        public DateTime? check_In { get; set; }
        public DateTime? check_Out { get; set; }
    }
}
