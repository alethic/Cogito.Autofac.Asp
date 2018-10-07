using System;
using System.Runtime.InteropServices;

using Autofac;
using Autofac.Features.OwnedInstances;

using Cogito.Autofac.Asp.Proxies;

namespace Cogito.Autofac.Asp
{

    /// <summary>
    /// Provides an interface to the Autofac component context.
    /// </summary>
    class ComponentContextProxy : MarshalByRefObject
    {

        readonly Func<IComponentContext> context;
        readonly bool transient;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="transient"></param>
        public ComponentContextProxy(Func<IComponentContext> context, bool transient)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            this.transient = transient;
        }

        /// <summary>
        /// Retrieve a service from container.
        /// </summary>
        /// <param name="serviceTypeName"></param>
        /// <returns></returns>
        public IntPtr Resolve(string serviceTypeName)
        {
            if (serviceTypeName == null)
                throw new ArgumentNullException(nameof(serviceTypeName));

            return ResolveFunc(serviceTypeName, (ctx, serviceType) =>
                ctx.Resolve(serviceType));
        }

        /// <summary>
        /// Retrieve an optional service from container.
        /// </summary>
        /// <param name="serviceTypeName"></param>
        /// <returns></returns>
        public IntPtr ResolveOptional(string serviceTypeName)
        {
            if (serviceTypeName == null)
                throw new ArgumentNullException(nameof(serviceTypeName));

            return ResolveFunc(serviceTypeName, (ctx, serviceType) =>
                ctx.ResolveOptional(serviceType));
        }

        /// <summary>
        /// Retrieve a named service from container.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="serviceTypeName"></param>
        /// <returns></returns>
        public IntPtr ResolveNamed(string serviceName, string serviceTypeName)
        {
            if (serviceName == null)
                throw new ArgumentNullException(nameof(serviceName));
            if (serviceTypeName == null)
                throw new ArgumentNullException(nameof(serviceTypeName));

            return ResolveFunc(serviceTypeName, (ctx, serviceType) =>
                ctx.ResolveNamed(serviceName, serviceType));
        }

        /// <summary>
        /// Retrieve an owned service from the request context.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="serviceTypeName"></param>
        /// <returns></returns>
        public IntPtr ResolveOwned(string serviceTypeName)
        {
            if (serviceTypeName == null)
                throw new ArgumentNullException(nameof(serviceTypeName));

            return ResolveFunc(serviceTypeName, (ctx, serviceType) =>
            {
                var value = ctx.Resolve(typeof(Owned<>).MakeGenericType(serviceType));
                if (value == null)
                    return null;

                // wrap in COM compatible proxy
                return new OwnedProxy((OwnedProxyInstance)Activator.CreateInstance(typeof(OwnedProxyInstance<>).MakeGenericType(serviceType), value));
            });
        }

        /// <summary>
        /// Internal method to resolve the service with the specified type using the specified resolution method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serviceTypeName"></param>
        /// <param name="resolve"></param>
        /// <returns></returns>
        IntPtr ResolveFunc(string serviceTypeName, Func<IComponentContext, Type, object> resolve)
        {
            var serviceType = Type.GetType(serviceTypeName);
            if (serviceType == null)
                throw new TypeLoadException($"Unable to locate '{serviceTypeName}'.");

            var scope = context();
            if (scope == null)
                throw new InvalidOperationException("Could not resolve Autofac component context.");

            var service = resolve(scope, serviceType);
            if (service == null)
                return IntPtr.Zero;

            // pointer to IUnknown on CCW
            return Marshal.GetIUnknownForObject(service);
        }

        public override object InitializeLifetimeService()
        {
            return transient ? base.InitializeLifetimeService() : null;
        }

    }

}
