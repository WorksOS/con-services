using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Log4Net.Extensions;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.Gateway.WebApi
{
  public class Program
  {
    private static void DependencyInjection()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory()))
        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels.SiteModels()))
        .Complete();
    }

    public static void Main(string[] args)
    {
      DependencyInjection();

      var kestrelConfig = new ConfigurationBuilder()
        .AddJsonFile("kestrelsettings.json", optional: true, reloadOnChange: false)
        .Build();
      var host = new WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseIISIntegration()
        //Logging configured in DIBuilder
        /*
        .ConfigureLogging(builder =>
        {
          Log4NetProvider.RepoName = Startup.LoggerRepoName;
          builder.Services.AddSingleton<ILoggerProvider, Log4NetProvider>();
          builder.SetMinimumLevel(LogLevel.Information);
        })
        */
        .UseStartup<Startup>()
        .Build();

      host.Run();
    }
  }
}
