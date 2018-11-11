using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace MockProjectWebApi
{
  public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseUrls("http://0.0.0.0:5001")
                .Build();

            host.Run();
        }
    }
}
