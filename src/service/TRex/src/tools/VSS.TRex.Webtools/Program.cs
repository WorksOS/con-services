using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog.Extensions.Logging;
using VSS.Serilog.Extensions;

namespace VSS.TRex.Webtools
{
  public class Program
    {
    public static void Main(string[] args)
    {
      try
      {
        BuildWebHost(args).Run();
      }
      catch (Exception e)
      {
        Console.WriteLine($"Unhandled exception: {e}");
        Console.WriteLine($"Stack trace: {e.StackTrace}");
      }
    }

    public static IWebHost BuildWebHost(string[] args)
    {
      return WebHost.CreateDefaultBuilder(args)
                    .ConfigureLogging((hostContext, loggingBuilder) =>
                    {
                      loggingBuilder.AddProvider(
                        p => new SerilogLoggerProvider(
                          SerilogExtensions.Configure()));
                    })
                    .UseStartup<Startup>()
                    .Build();
    }
  }
}
