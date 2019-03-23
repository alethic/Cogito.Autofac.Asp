using System;

namespace Cogito.Autofac.Asp.Sample.Objects
{

    [RegisterAs(typeof(ApplicationResolvableObject))]
    public class ApplicationResolvableObject
    {

        public string Value => Guid.NewGuid().ToString();

    }

}