﻿using System.IO;
using System.Net;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Log4Net.Extensions;
using VSS.Productivity3D.Now3D;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Now3D
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var host = new WebHostBuilder().BuildHostWithReflectionException(builder =>
      {
        return builder.UseKestrel()
          .UseLibuv(opts => { opts.ThreadCount = 32; })
          .BuildKestrelWebHost(Startup.LoggerRepoName)
          .UseStartup<Startup>()
          .Build();
      });

       host.Run();
    }
  }
}
