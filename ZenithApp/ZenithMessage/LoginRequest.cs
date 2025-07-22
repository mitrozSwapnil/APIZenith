using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenithApp.ZenithMessage
{
    public class LoginRequest:BaseRequest
    {
       
        public string? email { get; set; }
        public string? mobileNo { get; set; }
        public string? password { get; set; }
        //public string? otp { get; set; }
        public string? loginType { get; set; }
    }
}
