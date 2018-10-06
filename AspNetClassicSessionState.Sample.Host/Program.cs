using System;
using System.IO;
using System.Reflection;

using Cogito.HostedWebCore;
using Cogito.Web.Configuration;

namespace AspNetClassicSessionState.Sample.Host
{

    public static class Program
    {

        /// <summary>
        /// Normalizes the given physical site path.
        /// </summary>
        /// <param name="physicalPath"></param>
        /// <returns></returns>
        static string NormalizePath(string physicalPath)
        {
            if (physicalPath == null)
                throw new ArgumentNullException(nameof(physicalPath));
            if (string.IsNullOrWhiteSpace(physicalPath))
                throw new ArgumentException(nameof(physicalPath));

            // can contain environmental variables
            physicalPath = Environment.ExpandEnvironmentVariables(physicalPath);

            // path might be relative, make absolute
            if (Path.IsPathRooted(physicalPath) == false)
                physicalPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), physicalPath);

            // has to exist
            if (Directory.Exists(physicalPath) == false)
                Directory.CreateDirectory(physicalPath);

            return new Uri(physicalPath).LocalPath;
        }

        /// <summary>
        /// Loads the resource with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static Stream GetManifestResource(string name) =>
            typeof(Program).Assembly.GetManifestResourceStream("AspNetClassicSessionState.Sample.Host." + name);

        /// <summary>
        /// Gets a temporary directory with the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static string GetTempPath(string name) =>
            NormalizePath($@"%TEMP%\AspNetClassicSessionState\{name}");

        /// <summary>
        /// Main application entry-point.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            new AppHostBuilder()
                .ConfigureWeb(GetManifestResource("Web.config"), h => h
                    .SystemWeb(w => w.Compilation(c => c.TempDirectory(GetTempPath("F")))))
                .ConfigureApp(GetManifestResource("ApplicationHost.config"), h => h
                    .SystemWebServer(s => s
                        .Asp(a => a.Cache(c => c.DiskTemplateCacheDirectory(GetTempPath("C"))))
                        .HttpCompression(c => c.Directory(GetTempPath("X"))))
                    .Site(1, s => s
                        .RemoveBindings()
                        .AddBinding("http", "*:41177:*")
                        .Application("/", a => a
                            .VirtualDirectory("/", v => v.UsePhysicalPath(NormalizePath(@"..\..\..\..\AspNetClassicSessionState.Sample"))))))
                .Build()
                .Run();

#if DEBUG
        System.Console.ReadLine();
#endif
        }

    }

}
