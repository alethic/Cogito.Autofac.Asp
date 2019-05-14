using System;
using System.Runtime.InteropServices;

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

        /// <summary>
        /// Resolves a service from the container.
        /// </summary>
        /// <param name="serviceTypeName"></param>
        /// <returns></returns>
        public static object Resolve(string serviceTypeName)
        {
            return ComponentContextUtil.Resolve(serviceTypeName);
        }

        /// <summary>
        /// Resolves a service from the container.
        /// </summary>
        /// <param name="serviceTypeName"></param>
        /// <returns></returns>
        public static object ResolveOptional(string serviceTypeName)
        {
            return ComponentContextUtil.ResolveOptional(serviceTypeName);
        }

        /// <summary>
        /// Resolves a named service from the container.
        /// </summary>
        /// <param name="serviceTypeName"></param>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        public static object ResolveNamed(string serviceName, string serviceTypeName)
        {
            return ComponentContextUtil.ResolveNamed(serviceName, serviceTypeName);
        }

        /// <summary>
        /// Resolves a owned service from the container.
        /// </summary>
        /// <param name="serviceTypeName"></param>
        /// <returns></returns>
        public static object ResolveOwned(string serviceTypeName)
        {
            return ComponentContextUtil.ResolveOwned(serviceTypeName);
        }

        /// <summary>
        /// Resolves an object from the root container.
        /// </summary>
        /// <param name="serviceTypeName"></param>
        /// <returns></returns>
        public static object ResolveApplication(string serviceTypeName)
        {
            return ComponentContextUtil.ResolveApplication(serviceTypeName);
        }

        /// <summary>
        /// Resolves an object from the root container.
        /// </summary>
        /// <param name="serviceTypeName"></param>
        /// <returns></returns>
        public static object ResolveApplicationOptional(string serviceTypeName)
        {
            return ComponentContextUtil.ResolveApplicationOptional(serviceTypeName);
        }

        /// <summary>
        /// Resolves an object from the root container.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="serviceTypeName"></param>
        /// <returns></returns>
        public static object ResolveApplicationNamed(string serviceName, string serviceTypeName)
        {
            return ComponentContextUtil.ResolveApplicationNamed(serviceName, serviceTypeName);
        }

        /// <summary>
        /// Resolves an object from the root container.
        /// </summary>
        /// <param name="serviceTypeName"></param>
        /// <returns></returns>
        public static object ResolveApplicationOwned(string serviceTypeName)
        {
            return ComponentContextUtil.ResolveApplicationOwned(serviceTypeName);
        }

    }

}
