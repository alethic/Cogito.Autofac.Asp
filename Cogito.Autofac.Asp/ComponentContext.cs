using System;
using System.Globalization;
using System.Runtime.InteropServices;

using ASPTypeLibrary;

namespace Cogito.Autofac.Asp
{

    /// <summary>
    /// COM-accessible proxy to the Autofac <see cref="IComponentContext"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("85CF09B0-725D-4194-87E1-CC27578335E1")]
    [ProgId("Cogito.Autofac.Asp.ComponentContext")]
    public class ComponentContext
    {

        public static readonly string AppDomainItemPrefix = "__COMCTXPROXYPTR::";
        public static readonly string HeadersProxyItemKey = "COMCTXPROXYPTR";

        /// <summary>
        /// Resolves a service from the container.
        /// </summary>
        /// <param name="serviceTypeName"></param>
        /// <returns></returns>
        public object Resolve(string serviceTypeName)
        {
            if (serviceTypeName == null)
                throw new ArgumentNullException(nameof(serviceTypeName));

            return ResolveFunc(proxy => proxy.Resolve(serviceTypeName));
        }

        /// <summary>
        /// Resolves a service from the container.
        /// </summary>
        /// <param name="serviceTypeName"></param>
        /// <returns></returns>
        public object ResolveOptional(string serviceTypeName)
        {
            if (serviceTypeName == null)
                throw new ArgumentNullException(nameof(serviceTypeName));

            return ResolveFunc(proxy => proxy.ResolveOptional(serviceTypeName));
        }

        /// <summary>
        /// Resolves a named service from the container.
        /// </summary>
        /// <param name="serviceTypeName"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public object ResolveNamed(string serviceName, string serviceTypeName)
        {
            if (serviceName == null)
                throw new ArgumentNullException(nameof(serviceName));
            if (serviceTypeName == null)
                throw new ArgumentNullException(nameof(serviceTypeName));

            return ResolveFunc(proxy => proxy.ResolveNamed(serviceName, serviceTypeName));
        }

        /// <summary>
        /// Resolves a owned service from the container.
        /// </summary>
        /// <param name="serviceTypeName"></param>
        /// <returns></returns>
        public object ResolveOwned(string serviceTypeName)
        {
            if (serviceTypeName == null)
                throw new ArgumentNullException(nameof(serviceTypeName));

            return ResolveFunc(proxy => proxy.ResolveOwned(serviceTypeName));
        }

        /// <summary>
        /// Resolves an object from the root container.
        /// </summary>
        /// <param name="serviceTypeName"></param>
        /// <returns></returns>
        public object ResolveApplication(string serviceTypeName)
        {
            if (serviceTypeName == null)
                throw new ArgumentNullException(nameof(serviceTypeName));

            return ResolveApplicationFunc(proxy => proxy.Resolve(serviceTypeName));
        }

        /// <summary>
        /// Resolves an object from the root container.
        /// </summary>
        /// <param name="serviceTypeName"></param>
        /// <returns></returns>
        public object ResolveApplicationOptional(string serviceTypeName)
        {
            if (serviceTypeName == null)
                throw new ArgumentNullException(nameof(serviceTypeName));

            return ResolveApplicationFunc(proxy => proxy.ResolveOptional(serviceTypeName));
        }

        /// <summary>
        /// Resolves an object from the root container.
        /// </summary>
        /// <param name="serviceTypeName"></param>
        /// <returns></returns>
        public object ResolveApplicationNamed(string serviceName, string serviceTypeName)
        {
            if (serviceTypeName == null)
                throw new ArgumentNullException(nameof(serviceTypeName));

            return ResolveApplicationFunc(proxy => proxy.ResolveNamed(serviceName, serviceTypeName));
        }

        /// <summary>
        /// Resolves an object from the root container.
        /// </summary>
        /// <param name="serviceTypeName"></param>
        /// <returns></returns>
        public object ResolveApplicationOwned(string serviceTypeName)
        {
            if (serviceTypeName == null)
                throw new ArgumentNullException(nameof(serviceTypeName));

            return ResolveApplicationFunc(proxy => proxy.ResolveOwned(serviceTypeName));
        }

        /// <summary>
        /// Invokes the desired resolve method and appropriately wraps the result for COM.
        /// </summary>
        /// <param name="serviceTypeName"></param>
        /// <param name="resolve"></param>
        /// <returns></returns>
        object ResolveWithProxyFunc(IComponentContextProxy proxy, Func<IComponentContextProxy, object> resolve)
        {
            if (proxy == null)
                throw new ArgumentNullException(nameof(proxy));
            if (resolve == null)
                throw new ArgumentNullException(nameof(resolve));

            return resolve(proxy);
        }

        /// <summary>
        /// Invokes the desired resolve method and appropriately wraps the result for COM.
        /// </summary>
        /// <param name="resolve"></param>
        /// <returns></returns>
        object ResolveFunc(Func<IComponentContextProxy, object> resolve)
        {
            var proxy = GetProxy();
            if (proxy == null)
                throw new InvalidOperationException("Unable to resolve Autofac COM request proxy. Ensure the Cogito.Autofac.Asp project is installed within the ASP.Net site, and that Autofac.Web has been configured properly with an HttpApplication instance that implements IContainerProviderAccessor.");

            return ResolveWithProxyFunc(proxy, resolve);
        }

        /// <summary>
        /// Invokes the desired resolve method and appropriately wraps the result for COM.
        /// </summary>
        /// <param name="resolve"></param>
        /// <returns></returns>
        object ResolveApplicationFunc(Func<IComponentContextProxy, object> resolve)
        {
            var proxy = GetApplicationProxy();
            if (proxy == null)
                throw new InvalidOperationException("Unable to resolve Autofac COM application proxy. Ensure the Cogito.Autofac.Asp project is installed within the ASP.Net site, and that Autofac.Web has been configured properly with an HttpApplication instance that implements IContainerProviderAccessor.");

            return ResolveWithProxyFunc(proxy, resolve);
        }

        /// <summary>
        /// Discovers the proxy by consulting the default AppDomain for the registered application.
        /// </summary>
        /// <returns></returns>
        IComponentContextProxy GetApplicationProxy()
        {
            var request = (IRequest)System.EnterpriseServices.ContextUtil.GetNamedProperty("Request");
            if (request == null)
                throw new InvalidOperationException("Unable to locate request context.");

            // unique ID for the request
            var variables = (IStringList)request.ServerVariables["APPL_MD_PATH"];
            var applMdPath = variables?.Count >= 1 ? (string)variables[1] : null;
            if (applMdPath == null)
                throw new InvalidOperationException("Unable to discover IIS application path");

            // find default domain
            var ad = AppDomain.CurrentDomain;
            if (ad.IsDefaultAppDomain() == false)
            {
                new mscoree.CorRuntimeHost().GetDefaultDomain(out var adv);
                if (adv is AppDomain ad2)
                    ad = ad2;
            }

            // find our remote reference
            var intPtr = (IntPtr?)ad.GetData($"{AppDomainItemPrefix}{applMdPath}") ?? IntPtr.Zero;
            if (intPtr == IntPtr.Zero)
                throw new InvalidOperationException("Could not locate IntPtr of remote global LN environment proxy.");

            var proxy = (IComponentContextProxy)Marshal.GetObjectForIUnknown(intPtr);
            return proxy;
        }

        /// <summary>
        /// Discovers the proxy by examining the request context.
        /// </summary>
        /// <returns></returns>
        IComponentContextProxy GetProxy()
        {
            var request = (IRequest)System.EnterpriseServices.ContextUtil.GetNamedProperty("Request");
            if (request == null)
                throw new InvalidOperationException("Unable to locate request context.");

            // unique ID for the request
            var variables = (IStringList)request.ServerVariables["HTTP_" + HeadersProxyItemKey];
            var intPtrEnc = variables?.Count >= 1 ? (string)variables[1] : null;
            if (intPtrEnc == null || string.IsNullOrWhiteSpace(intPtrEnc))
                return null;

            // decode pointer to proxy
            var intPtr = new IntPtr(long.Parse(intPtrEnc, NumberStyles.HexNumber));
            if (intPtr == IntPtr.Zero)
                return null;

            // get reference to proxy
            var proxy = (IComponentContextProxy)Marshal.GetObjectForIUnknown(intPtr);
            return proxy;
        }

    }

}
