using System;
using System.Web;
using System.Web.SessionState;

namespace AspNetClassicSessionState.AspNet
{

    public class EnableSessionStateHandler : IHttpAsyncHandler, IRequiresSessionState
    {

        public bool IsReusable => true;

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            return new CompletedAsyncResult(true);
        }

        public void EndProcessRequest(IAsyncResult result)
        {

        }

        public void ProcessRequest(HttpContext context)
        {

        }

    }


}
