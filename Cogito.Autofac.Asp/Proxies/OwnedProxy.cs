using System;
using System.Runtime.InteropServices;

namespace Cogito.Autofac.Asp.Proxies
{

    /// <summary>
    /// Provides a COM compatible interface to the Autofac Owned instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ComVisible(true)]
    [Guid("9FF5DCD7-F2D4-4B5B-90E2-075C9E37D18E")]
    public class OwnedProxy :
        IDisposable
    {

        readonly OwnedProxyInstance instance;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="owned"></param>
        internal OwnedProxy(OwnedProxyInstance instance)
        {
            this.instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        /// <summary>
        /// Gets the owned value.
        /// </summary>
        public object Value => instance.Value;

        /// <summary>
        /// Disposes of the owned instance.
        /// </summary>
        public void Dispose()
        {
            instance.Dispose();
        }

    }

}
