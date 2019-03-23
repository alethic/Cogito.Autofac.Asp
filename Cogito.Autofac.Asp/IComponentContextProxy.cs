using System;
using System.Runtime.InteropServices;

namespace Cogito.Autofac.Asp
{

    [ComVisible(true)]
    [ComImport]
    [Guid("19716793-61B5-4088-9291-52EDDFE0958E")]
    public interface IComponentContextProxy
    {

        IntPtr Resolve(string serviceTypeName);

        IntPtr ResolveNamed(string serviceName, string serviceTypeName);

        IntPtr ResolveOptional(string serviceTypeName);

        IntPtr ResolveOwned(string serviceTypeName);

    }

}