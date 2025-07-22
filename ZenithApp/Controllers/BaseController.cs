
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using ZenithApp.ZenithMessage;

namespace ZenithApp.Controllers
{
    public abstract class BaseController : ControllerBase, IDisposable
    {
        protected abstract BaseResponse Execute(string action, BaseRequest request);
        protected abstract void Disposing();

        public BaseController()
        {
        }


        public IActionResult ProcessRequest<T>(BaseRequest request,
                                              [CallerMemberName] string action = "") where T : BaseResponse, new()
        {

            // Ask Child controler to initiate execution
            BaseResponse response = this.Execute(action, request);

            // return the response back to client
            return StatusCode((int)response.HttpStatusCode, response);
        }

        private T CreateGenericResponse<T>(Exception ex) where T : BaseResponse, new()
        {
            T t = new T();

            t.Success = false;
            t.Message = "Error Occurred and Logged. We are looking into it";
            t.ResponseCode = 500;       // TODO : Use constants

            return t;
        }

        private IPrincipal GetPrincipal(HttpRequest request)
        {
            string[] roles = null;  /// populate this from token
            BUTPrincipal butPrincipal = new BUTPrincipal(request.HttpContext.User.Identity, roles);

            return butPrincipal;
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    this.Disposing();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BaseController() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }


    public class BUTPrincipal : GenericPrincipal
    {
        public string EmailID { get; set; }
        public string LastLoggedIn { get; set; }

        public BUTPrincipal(IIdentity identity, string[] roles) : base(identity, roles)
        {

            LastLoggedIn = ""; //read from claim
            EmailID = identity.Name;      //read from claim

        }
    }
}
