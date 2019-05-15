using System;

namespace Cogito.Autofac.Asp
{

    /// <summary>
    /// Simple object that invokes an action on finalize.
    /// </summary>
    class DisposableAction : IDisposable
    {

        readonly Action action;
        bool disposed;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="action"></param>
        public DisposableAction(Action action)
        {
            this.action = action;
        }

        public void Dispose()
        {
            if (disposed == false)
            {
                action?.Invoke();
                GC.SuppressFinalize(this);
                disposed = true;
            }
        }

        ~DisposableAction()
        {
            Dispose();
        }

    }

}
