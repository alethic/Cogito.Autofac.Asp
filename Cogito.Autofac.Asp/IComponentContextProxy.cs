using System;
using System.Runtime.InteropServices;

namespace Cogito.Autofac.Asp
{

    [ComVisible(true)]
    [ComImport]
    [Guid("19716793-61B5-4088-9291-52EDDFE0958E")]
    public interface IComponentContextProxy
    {

        object Resolve(string serviceTypeName);

        object ResolveNamed(string serviceName, string serviceTypeName);

        object ResolveOptional(string serviceTypeName);

        object ResolveOwned(string serviceTypeName);

    }

}