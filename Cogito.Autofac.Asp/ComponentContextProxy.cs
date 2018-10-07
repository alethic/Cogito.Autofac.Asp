using System;
using System.Runtime.InteropServices;

using Autofac;

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
        /// Resolves the type with the specified name from the container.
        /// </summary>
        /// <param name="serviceTypeName"></param>
        /// <returns></returns>
        public IntPtr Resolve(string serviceTypeName)
        {
            var serviceType = Type.GetType(serviceTypeName);
            if (serviceType == null)
                throw new TypeLoadException($"Unable to locate '{serviceTypeName}'.");

            var scope = context();
            if (scope == null)
                throw new InvalidOperationException("Could not resolve Autofac component context.");

            // resolve through component context
            var service = scope.Resolve(serviceType);
            if (service == null)
                return IntPtr.Zero;

            // pointer to IUnknown on CCW
            return Marshal.GetIUnknownForObject(service);
        }

        /// <summary>
        /// Resolves the type with the specified name from the container.
        /// </summary>
        /// <param name="serviceTypeName"></param>
        /// <returns></returns>
        public IntPtr ResolveOptional(string serviceTypeName)
        {
            var serviceType = Type.GetType(serviceTypeName);
            if (serviceType == null)
                throw new TypeLoadException($"Unable to locate '{serviceTypeName}'.");

            var scope = context();
            if (scope == null)
                throw new InvalidOperationException("Could not resolve Autofac component context.");

            // resolve through component context
            var service = scope.ResolveOptional(serviceType);
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
