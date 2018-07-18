using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.CoordinateSystems.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Servers.Compute;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.TAGFiles.Classes;

namespace VSS.TRex.Server.MutableData
{
  class Program
  {
    private static readonly AutoResetEvent WaitHandle = new AutoResetEvent(false);

    public static IConfiguration Configuration { get; set; }

    private static void DependencyInjection()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory()))
        .Add(x => x.AddSingleton<ITFAProxy>(new TFAProxy(Configuration)))
        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels.SiteModels()))
        .Add(x => x.AddSingleton<ICoordinateConversion>(new CoordinateConversion()))
        .Add(x => x.AddSingleton(Configuration))
        .Complete();
    }

    static void Main(string[] args)
    {
      // Load settings for Mutabledata
      Configuration = new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddEnvironmentVariables() // can also come from environment variables which will override json file
          .Build();

      DependencyInjection();

      if (Configuration.GetSection("EnableTFAService").Value == null)
        Console.WriteLine("*** Warning! **** Check for missing configuration values. e.g EnableTFAService");

      Console.WriteLine("**** Configuration Settings ****");
      foreach (var env in Configuration.GetChildren())
      {
        Console.WriteLine($"{env.Key}:{env.Value}");
      }


      var server = new TagProcComputeServer();
      Console.WriteLine("Press ctrl+c to exit");
      Console.CancelKeyPress += (s, a) =>
      {
        Console.WriteLine("Exiting");
        WaitHandle.Set();
      };

      WaitHandle.WaitOne();
    }
  }
}
