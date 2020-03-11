using System;
using Microsoft.AspNetCore.Hosting;
using VSS.TRex.Gateway.Common.Converters;
using VSS.WebApi.Common;

namespace VSS.TRex.Gateway.WebApi
{
  public class Program
  {
    private static void EnsureAssemblyDependenciesAreLoaded()
    {
      // This static array ensures that all required assemblies are included into the artifacts by the linker
      Type[] AssemblyDependencies =
      {
        typeof(VSS.TRex.SiteModelChangeMaps.GridFabric.Services.SiteModelChangeProcessorService)
      };

      foreach (var asmType in AssemblyDependencies)
        if (asmType.Assembly == null)
          Console.WriteLine($"Assembly for type {asmType} has not been loaded.");
    }

    public static void Main()
    {
      try
      {
        EnsureAssemblyDependenciesAreLoaded();

        BuildWebHost().Run();
      }
      catch (Exception e)
      {
        Console.WriteLine($"Unhandled exception: {e}");
        Console.WriteLine($"Stack trace: {e.StackTrace}");
      }
    }

    public static IWebHost BuildWebHost()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();

      return new WebHostBuilder().BuildHostWithReflectionException(builder =>
      {
        return builder.UseKestrel()
          .UseLibuv(opts => { opts.ThreadCount = 32; })
          .BuildKestrelWebHost()
          .UseStartup<Startup>()
          .Build();
      });
    }
  }
}
