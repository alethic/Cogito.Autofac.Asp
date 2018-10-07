using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Hosting;

using Autofac;
using Autofac.Integration.Web;

using Cogito.Threading;

using Microsoft.Web.Infrastructure.DynamicModuleHelper;

[assembly: WebActivatorEx.PreApplicationStartMethod(
    typeof(Cogito.Autofac.Asp.ComponentContextModule),
    nameof(Cogito.Autofac.Asp.ComponentContextModule.PreApplicationStart))]

[assembly: WebActivatorEx.ApplicationShutdownMethod(
    typeof(Cogito.Autofac.Asp.ComponentContextModule),
    nameof(Cogito.Autofac.Asp.ComponentContextModule.ApplicationShutdown))]

namespace Cogito.Autofac.Asp
{

    /// <summary>
    /// Initializes the application to share state with ASP Classic Requests through the ASP.Net State Server.
    /// </summary>
    public class ComponentContextModule :
        IHttpModule
    {

        const string ContextProxyItemKey = "COMCTXPROXYREF";

        static ComponentContextProxy rootProxy;
        static ObjRef rootProxyObjRef;

        /// <summary>
        /// Invoked before the application starts.
        /// </summary>
        public static void PreApplicationStart()
        {
            DynamicModuleUtility.RegisterModule(typeof(ComponentContextModule));
        }

        /// <summary>
        /// Resolves the Autofac root context when needed.
        /// </summary>
        /// <returns></returns>
        static IComponentContext GetAutofacApplicationContext(HttpApplication context)
        {
            if (context is IContainerProviderAccessor accessor)
            {
                try
                {
                    HttpContext.Current = context.Context;
                    return accessor.ContainerProvider.ApplicationContainer;
                }
                finally
                {
                    HttpContext.Current = null;
                }
            }

            throw new InvalidOperationException($"Cannot locate Autofac lifetime scope for the application. {nameof(ComponentContextModule)} requires the Autofac.Web package to be configured.");
        }

        /// <summary>
        /// Resolves the Autofac per-request context when needed.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        static IComponentContext GetAutofacRequestContext(HttpApplication context)
        {
            // or check in Autofac.Web integration
            if (context is IContainerProviderAccessor accessor)
            {
                try
                {
                    HttpContext.Current = context.Context;
                    return accessor.ContainerProvider.RequestLifetime;
                }
                finally
                {
                    HttpContext.Current = null;
                }
            }

            throw new InvalidOperationException($"Cannot locate Autofac lifetime scope for the request. {nameof(ComponentContextModule)} requires the Autofac.Web package to be configured.");
        }

        /// <summary>
        /// Invoked when the application shuts down.
        /// </summary>
        public static void ApplicationShutdown()
        {
            if (rootProxy != null)
            {
                // disconnect the app domain proxy
                RemotingServices.Disconnect(rootProxy);
                rootProxyObjRef = null;
                rootProxy = null;
            }

            // clear reference to this application in the default app domain
            new mscoree.CorRuntimeHostClass().GetDefaultDomain(out var adv);
            if (adv is AppDomain ad && ad.IsDefaultAppDomain())
                ad.SetData($"__COMCTXPROXYMODREF_{HostingEnvironment.ApplicationID}", null);
        }

        /// <summary>
        /// Initializes the instance.
        /// </summary>
        /// <param name="context"></param>
        public void Init(HttpApplication context)
        {
            // connect the app domain proxy to the root container
            var container = GetAutofacApplicationContext(context);
            rootProxy = new ComponentContextProxy(() => container, false);
            rootProxyObjRef = RemotingServices.Marshal(rootProxy);

            // store reference to this application in the default app domain
            new mscoree.CorRuntimeHostClass().GetDefaultDomain(out var adv);
            if (adv is AppDomain ad && ad.IsDefaultAppDomain())
                ad.SetData($"{ComponentContext.AppDomainItemPrefix}{HostingEnvironment.ApplicationID}", rootProxyObjRef);

            context.AddOnBeginRequestAsync(BeginOnBeginRequestAsync, EndOnBeginRequestAsync);
            context.AddOnEndRequestAsync(BeginOnEndRequestAsync, EndOnEndRequestAsync);
        }

        /// <summary>
        /// Serializes the ObjRef to a base64 encoded string.
        /// </summary>
        /// <param name="objRef"></param>
        /// <returns></returns>
        string SerializeObjRef(ObjRef objRef)
        {
            using (var stm = new MemoryStream())
            using (var cmp = new DeflateStream(stm, CompressionMode.Compress, true))
            {
                var srs = new BinaryFormatter();
                srs.Serialize(cmp, objRef);
                cmp.Flush();
                cmp.Dispose();
                stm.Position = 0;
                return Convert.ToBase64String(stm.ToArray());
            }
        }

        /// <summary>
        /// Invoked when the request is beginning.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <param name="cb"></param>
        /// <param name="extraData"></param>
        /// <returns></returns>
        IAsyncResult BeginOnBeginRequestAsync(object sender, EventArgs args, AsyncCallback cb, object extraData)
        {
            // ignore spurious calls
            var context = HttpContext.Current;
            if (context == null)
                return new CompletedAsyncResult(null, null);

            // only generate for classic ASP requests
            if (context.Request.CurrentExecutionFilePathExtension == ".asp")
            {
                // generate new proxy reference to current request context
                var proxy = new ComponentContextProxy(() => GetAutofacRequestContext(context.ApplicationInstance), true);

                // add serialized object ref into a header, reachable by classic ASP
                var objRef = RemotingServices.Marshal(proxy, null, typeof(ComponentContextProxy));
                context.Request.Headers.Add(ComponentContext.HeadersProxyItemKey, SerializeObjRef(objRef));
                context.Items.Add(ContextProxyItemKey, proxy);
            }

            return new CompletedAsyncResult(null, null);
        }

        /// <summary>
        /// Invoked when the request is beginning.
        /// </summary>
        /// <param name="ar"></param>
        void EndOnBeginRequestAsync(IAsyncResult ar)
        {

        }

        /// <summary>
        /// Invoked when the request is ending.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="cb"></param>
        /// <param name="extraData"></param>
        /// <returns></returns>
        IAsyncResult BeginOnEndRequestAsync(object sender, EventArgs e, AsyncCallback cb, object extraData)
        {
            // ignore spurious calls
            var context = HttpContext.Current;
            if (context == null)
                return new CompletedAsyncResult(null, null);

            // did we store away a proxy for disconnection?
            if (context.Items.Contains(ContextProxyItemKey))
            {
                var proxy = (ComponentContextProxy)context.Items[ContextProxyItemKey];
                if (proxy != null)
                {
                    RemotingServices.Disconnect(proxy);
                    context.Items.Remove(ContextProxyItemKey);
                }
            }

            return new CompletedAsyncResult(null, null);
        }

        /// <summary>
        /// Invoked when the request is ending.
        /// </summary>
        /// <param name="ar"></param>
        void EndOnEndRequestAsync(IAsyncResult ar)
        {

        }

        /// <summary>
        /// Disposes of the instance.
        /// </summary>
        public void Dispose()
        {

        }

    }

}
