using System;

using Microsoft.Owin;

namespace Cogito.Autofac.Asp.Sample.Objects
{

    [RegisterAs(typeof(ResolvableObject))]
    public class ResolvableObject
    {

        readonly IOwinContext owin;

        public ResolvableObject(IOwinContext owin)
        {
            this.owin = owin ?? throw new ArgumentNullException(nameof(owin));
        }

        public string Host => owin.Request.Host.Value;

    }

}