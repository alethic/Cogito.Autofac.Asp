using System;
using System.Runtime.InteropServices;
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

        const string ContextProxyPtrItemKey = "COMCTXPROXYPTR";

        static readonly object rootSyncRoot = new object();
        static bool init = true;

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

            return null;
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

            return null;
        }

        /// <summary>
        /// Invoked when the application shuts down.
        /// </summary>
        public static void ApplicationShutdown()
        {
            if (init)
            {
                lock (rootSyncRoot)
                {
                    if (init)
                    {
                        var appDomainKey = $"{ComponentContext.AppDomainItemPrefix}{HostingEnvironment.ApplicationID}";
                        new mscoree.CorRuntimeHost().GetDefaultDomain(out var adv);
                        if (adv is AppDomain ad && ad.IsDefaultAppDomain() && ad.GetData(appDomainKey) is IntPtr ptr)
                        {
                            Marshal.Release(ptr);
                            ad.SetData(appDomainKey, null);
                        }

                        init = false;
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the instance.
        /// </summary>
        /// <param name="context"></param>
        public void Init(HttpApplication context)
        {
            lock (rootSyncRoot)
            {
                // only enable if Autofac.Web is configured
                if (context is IContainerProviderAccessor accessor)
                {
                    // connect the app domain proxy to the root container
                    var container = GetAutofacApplicationContext(context);
                    var rootProxy = new ComponentContextProxy(() => container);

                    // store reference to this application in the default app domain
                    var appDomainKey = $"{ComponentContext.AppDomainItemPrefix}{HostingEnvironment.ApplicationID}";
                    new mscoree.CorRuntimeHost().GetDefaultDomain(out var adv);
                    if (adv is AppDomain ad && ad.IsDefaultAppDomain() && ad.GetData(appDomainKey) == null)
                        ad.SetData(appDomainKey, Marshal.GetIUnknownForObject(rootProxy));
                }
            }

            // register request events for context
            context.AddOnBeginRequestAsync(BeginOnBeginRequestAsync, EndOnBeginRequestAsync);
            context.AddOnEndRequestAsync(BeginOnEndRequestAsync, EndOnEndRequestAsync);
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
                var proxy = new ComponentContextProxy(() => GetAutofacRequestContext(context.ApplicationInstance));

                // add serialized object ref into a header, reachable by classic ASP
                var intPtr = Marshal.GetIUnknownForObject(proxy);
                context.Request.Headers.Add(ComponentContext.HeadersProxyItemKey, intPtr.ToInt64().ToString("X"));
                context.Items.Add(ContextProxyPtrItemKey, intPtr);
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

            // release stored pointer
            if (context.Items[ContextProxyPtrItemKey] is IntPtr ptr)
            {
                Marshal.Release(ptr);
                context.Items.Remove(ContextProxyPtrItemKey);
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
