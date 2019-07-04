using System;
using Microsoft.AspNetCore.Hosting;
using VSS.TRex.Gateway.Common.Converters;
using VSS.WebApi.Common;

namespace VSS.TRex.ConnectedSite.Gateway.WebApi
{
  public class Program
  {
    // This static array ensures that all required assemblies are included into the artifacts by the linker
    private static void EnsureAssemblyDependenciesAreLoaded()
    {
      // This static array ensures that all required assemblies are included into the artifacts by the linker
      Type[] AssemblyDependencies =
      {
        typeof(VSS.TRex.TAGFiles.Executors.SubmitTAGFileExecutor),
        typeof(VSS.TRex.TAGFiles.GridFabric.NodeFilters.TAGProcessorRoleBasedNodeFilter),
        typeof (VSS.TRex.TAGFiles.GridFabric.ComputeFuncs.SubmitTAGFileComputeFunc)
      };

      foreach (var asmType in AssemblyDependencies)
      {
        if (asmType.Assembly == null)
        {
          Console.WriteLine($"Assembly for type {asmType} has not been loaded.");
        }
      }
    }

    public static void Main(string[] args)
    {
      EnsureAssemblyDependenciesAreLoaded();

      var webHost = BuildWebHost(args);

      webHost.Run();
    }

    public static IWebHost BuildWebHost(string[] args)
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
