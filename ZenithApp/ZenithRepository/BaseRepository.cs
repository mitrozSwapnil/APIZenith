
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using ZenithApp.ZenithMessage;

namespace ZenithApp.ZenithRepository
{
    public abstract class BaseRepository : IDisposable
    {
        protected abstract void Disposing();

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        public IIdentity GetCurrentIdentityUser(BaseRequest Request)
        {
            return Request.RequestContext.User.Identity;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Disposing();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
