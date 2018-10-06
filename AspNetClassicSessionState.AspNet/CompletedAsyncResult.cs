using System;
using System.Threading;

namespace AspNetClassicSessionState.AspNet
{

    /// <summary>
    /// Immediately completed <see cref="IAsyncResult"/>.
    /// </summary>
    public class CompletedAsyncResult : 
        IAsyncResult
    {

        readonly bool result;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="result"></param>
        public CompletedAsyncResult(bool result)
        {
            this.result = result;
        }

        public bool IsCompleted
        {
            get { return true; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { throw new NotImplementedException(); }
        }

        public object AsyncState
        {
            get { return result; }
        }

        public bool CompletedSynchronously
        {
            get { return true; }
        }

    }

}
