using System;

using Autofac.Features.OwnedInstances;

namespace Cogito.Autofac.Asp.Proxies
{

    /// <summary>
    /// Provides a COM compatible interface to the Autofac Owned instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class OwnedProxyInstance<T> :
        OwnedProxyInstance,
        IDisposable
    {

        readonly Owned<T> owned;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="owned"></param>
        public OwnedProxyInstance(Owned<T> owned)
        {
            this.owned = owned ?? throw new ArgumentNullException(nameof(owned));
        }

        /// <summary>
        /// Gets the owned value.
        /// </summary>
        public override object Value => owned.Value;

        /// <summary>
        /// Disposes of the owned instance.
        /// </summary>
        public override void Dispose()
        {
            owned.Dispose();
        }

    }

    /// <summary>
    /// Wraps an owned by provides non-generic access.
    /// </summary>
    abstract class OwnedProxyInstance : IDisposable
    {

        /// <summary>
        /// Gets the owned value.
        /// </summary>
        public abstract object Value { get; }

        /// <summary>
        /// Disposes of the owned instance.
        /// </summary>
        public abstract void Dispose();

    }

}
