using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ZenithApp.ZenithMessage   
{
    public class RequestContext
    {
        public IPrincipal User { get; }

    }
}
