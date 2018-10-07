﻿using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters.Binary;

using ASPTypeLibrary;

namespace Cogito.Autofac.Asp
{

    /// <summary>
    /// COM-accessible proxy to the Autofac <see cref="IComponentContext"/>.
    /// </summary>
    [Guid("85CF09B0-725D-4194-87E1-CC27578335E1")]
    [ProgId("Cogito.Autofac.Asp.ComponentContext")]
    public class ComponentContext
    {

        public static readonly string AppDomainItemPrefix = "__COMCTXPROXYREF::";
        public static readonly string HeadersProxyItemKey = "COMCTXPROXYREF";

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ComponentContext()
        {

        }

        /// <summary>
        /// Resolves an object from the container.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.IUnknown)]
        public object Resolve(string typeName)
        {
            var ptr = GetProxy().Resolve(typeName);
            if (ptr == IntPtr.Zero)
                return null;

            // resolve to RCW, which .Net can marshal correctly
            var obj = Marshal.GetObjectForIUnknown(ptr);
            Marshal.Release(ptr);

            return obj;
        }

        /// <summary>
        /// Resolves an object from the root container.
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.IUnknown)]
        public object ResolveApplication(string typeName)
        {
            var ptr = GetApplicationProxy().Resolve(typeName);
            if (ptr == IntPtr.Zero)
                return null;

            // resolve to RCW, which .Net can marshal correctly
            var obj = Marshal.GetObjectForIUnknown(ptr);
            Marshal.Release(ptr);

            return obj;
        }

        /// <summary>
        /// Discovers the proxy by consulting the default AppDomain for the registered application.
        /// </summary>
        /// <returns></returns>
        ComponentContextProxy GetApplicationProxy()
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
                new mscoree.CorRuntimeHostClass().GetDefaultDomain(out var adv);
                if (adv is AppDomain ad2)
                    ad = ad2;
            }

            // find our remote reference
            var objRef = (ObjRef)ad.GetData($"{AppDomainItemPrefix}{applMdPath}");
            if (objRef == null)
                throw new InvalidOperationException("Could not locate ObjRef of remote global LN environment proxy.");

            var aspNetProxy = (ComponentContextProxy)RemotingServices.Unmarshal(objRef);
            return aspNetProxy;
        }

        /// <summary>
        /// Discovers the proxy by examining the request context.
        /// </summary>
        /// <returns></returns>
        ComponentContextProxy GetProxy()
        {
            var request = (IRequest)System.EnterpriseServices.ContextUtil.GetNamedProperty("Request");
            if (request == null)
                throw new InvalidOperationException("Unable to locate request context.");

            // unique ID for the request
            var variables = (IStringList)request.ServerVariables["HTTP_" + HeadersProxyItemKey];
            var objRefEnc = variables?.Count >= 1 ? (string)variables[1] : null;
            if (objRefEnc == null)
                return null;

            // deserialize proxy reference and connect
            using (var stm = new MemoryStream(Convert.FromBase64String(objRefEnc)))
            using (var cmp = new DeflateStream(stm, CompressionMode.Decompress))
            {
                var srs = new BinaryFormatter();
                var aspNetProxy = (ComponentContextProxy)srs.Deserialize(cmp);
                return aspNetProxy;
            }
        }

    }

}