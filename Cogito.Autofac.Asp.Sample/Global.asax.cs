using System.Web;

using Autofac;
using Autofac.Integration.Owin;
using Autofac.Integration.Web;

namespace Cogito.Autofac.Asp.Sample
{

    public class Global : HttpApplication
    {

        class OwinContainerProvider : IContainerProvider
        {

            public ILifetimeScope ApplicationContainer => Startup.Container;

            public ILifetimeScope RequestLifetime => HttpContext.Current?.GetOwinContext()?.GetAutofacLifetimeScope();

            public void EndRequestLifetime() { }

        }

        readonly IContainerProvider containerProvider = new OwinContainerProvider();

        //IContainerProvider IContainerProviderAccessor.ContainerProvider => containerProvider;

    }

}
