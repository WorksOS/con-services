using System.IO;
using Microsoft.AspNetCore.Hosting;

#if NET_4_7
using Microsoft.AspNetCore.Hosting.WindowsServices;
using System.Diagnostics;
#endif


namespace VSS.Productivity3D.TagFileAuth.WebAPI
{
  public class Program
    {
        public static void Main(string[] args)
        {
#if NET_4_7
      //To run the service use https://docs.microsoft.com/en-us/aspnet/core/hosting/windows-service
      var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
      var pathToContentRoot = Path.GetDirectoryName(pathToExe);


      var host = new WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(pathToContentRoot)
        .UseIISIntegration()
        .UseStartup<Startup>()
        .UseApplicationInsights()
        .Build();

      host.RunAsService();
#else

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
#endif
        }
    }
}
