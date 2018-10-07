using Autofac;

using Cogito.Autofac.Asp.Sample.Objects;

using Microsoft.Owin;

using Owin;

[assembly: OwinStartup(typeof(Cogito.Autofac.Asp.Sample.Startup))]

namespace Cogito.Autofac.Asp.Sample
{

    public class Startup
    {

        public static IContainer Container { get; set; }

        static Startup()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ResolvableObject>();
            Container = builder.Build();
        }

        public void Configuration(IAppBuilder app)
        {
            app.UseAutofacMiddleware(Container);
        }

    }

}
