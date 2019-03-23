using System;
using System.Linq;
using System.Runtime.InteropServices;

using Autofac;
using Autofac.Core;
using Autofac.Features.OwnedInstances;

using Cogito.Autofac.Asp.Proxies;

namespace Cogito.Autofac.Asp
{

    /// <summary>
    /// Provides an interface to the Autofac component context.
    /// </summary>
    [Guid("7E6EA822-67B7-4245-9F47-024F5057C293")]
    class ComponentContextProxy : MarshalByRefObject
    {

        readonly Func<IComponentContext> context;
        readonly bool transient;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public ComponentContextProxy(Func<IComponentContext> context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
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
            var scope = context();
            if (scope == null)
                throw new InvalidOperationException("Could not resolve Autofac component context. Ensure Autofac.Web has been configured and that your HttpAppliation instance implements IContainerProviderAccessor.");

            var serviceType = ResolveServiceTypeName(scope, serviceTypeName);
            if (serviceType == null)
                throw new InvalidOperationException($"Could not resolve service type name '{serviceTypeName}'.");

            var service = resolve(scope, serviceType);
            if (service == null)
                return IntPtr.Zero;

            // pointer to IUnknown on CCW
            return Marshal.GetIUnknownForObject(service);
        }

        /// <summary>
        /// Attempts to resolve the service type name.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="serviceTypeName"></param>
        /// <returns></returns>
        Type ResolveServiceTypeName(IComponentContext context, string serviceTypeName)
        {
            var serviceType = Type.GetType(serviceTypeName);
            if (serviceType != null)
                return serviceType;

            // attempt to scan repository for registration that might match based on type name
            serviceType = context.ComponentRegistry.Registrations
                .SelectMany(i => i.Services)
                .OfType<IServiceWithType>()
                .Select(i => i.ServiceType)
                .Where(j => j.FullName == serviceTypeName)
                .FirstOrDefault();
            if (serviceType != null)
                return serviceType;

            return null;
        }

        /// <summary>
        /// Obtains a lifetime service object to control the lifetime policy for this instance.
        /// </summary>
        /// <returns></returns>
        public override object InitializeLifetimeService()
        {
            return null;
        }

    }

}
