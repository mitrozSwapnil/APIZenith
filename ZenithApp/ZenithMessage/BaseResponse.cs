using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ZenithApp.ZenithMessage
{
    public class BaseResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public int ResponseCode { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
    }
}
