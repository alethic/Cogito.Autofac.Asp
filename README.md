# Cogito.Autofac.Asp
Provides the capability to resolve Autofac instances from Classic ASP

This project allows code from classic ASP (VBScript) to resolve instances to objects within the Autofac request lifetime scope.

This is done by initializing the `Cogito.Autofac.Asp.ComponentContext` object within ASP. This object then provides two methods: `Resolve` and `ResolveApplication`. The former will resolve from the lifetime scope for the current request. The latter from the default application container. Both accept the .NET type name to resolve as a string. Obviously, no generics.

The mechanism behind this is a little risky. The managed pipeline in IIS must be used. And the `HttpApplication` must provide a `IContainerProviderAccessor` implementation, as required by the Autofac.Web project. An ASP.Net `HttpModule` is registered. Upon init, it places an `ObjRef` to a proxy object within the default `AppDomain`'s data structure. This is done using the Common Language Runtime Execution Engine library. This `ObjRef` provides remoting from the default process `AppDomain` to the `AppDomain` in which the ASP.Net site is actually hosted. Second, at the beginning of each request, this module places a similar serialized and compressed `ObjRef` into the COM+ `Request` object's `ServerVariables`. This is accessible from a thread entering the COM component originating from classic ASP.

Each of these `ObjRef`s are then deserialized when attempting to resolve an Autofac component, in order to communicate across `AppDomain` boundaries. The associated proxy object then obtains the actual Autofac component context, and attempts to resolve the asked-for type. The type is then resolved to a pointer to the COM `IUnknown` instance, resulting in the creation of a COM Callable Wrapper (CCW). This `IntPtr` is returned across the remoting barrier. It is then converted into a CLR COM proxy whichi results in the creation of a Runtime Callable Wrapper (RCW). Yes, this is a RCW calling a CCW in another `AppDomain`. The RCW is then returned from the COM component to ASP, at which time .NET marshals it across as an actual reference to a COM object.

So, now you have a reference to a COM object existing in a separate AppDomain from the original entry point. This AppDomain is where the actual ASP.Net site was hosted. RCWs and CCWs are process wide, so they don't really know the difference.

The `Cogito.Autofac.Asp` project needs to be made available both to the AppDomain that hosts ASP.Net, as well as the default AppDomain of the IIS process. This probably means most people would GAC it. There are other solutions to that, however. I use IIS Hosted Web Core with registry free activation (see `Cogito.IIS.HostedCore` and `Cogito.COM.MSBuild`).

Usage is relatively simple. Edit your `global.asa` file to include `<object runat="server" scope="Application" id="ComponentContext" progid="Cogito.Autofac.Asp.ComponentContext"></object>`, which makes an application-wide instance available. You can then invoke `Resolve` or `ResolveApplication` on it:

`
Dim obj
Set obj = ComponentContext.ResolveApplication("Cogito.Autofac.Asp.Sample.Objects.ResolvableObject, Cogito.Autofac.Asp.Sample.Objects")
Dim val
val = obj.Text
`

Voila!
