using System;
using CCSS.WorksOS.Healthz;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace IntegrationTests.Pulse.WebAPI
{
  public class StartupTests
  {
    [Fact]
    public void GenericWebHostThrowsOnBuild()
    {
      var exception = Record.Exception(() =>
      {
        var hostBuilder = new HostBuilder()
          .ConfigureWebHost(builder =>
          {
            builder.UseStartup<Startup>();
            builder.Build();
          });
      });

      exception.Should().BeOfType(typeof(NotSupportedException));
      exception.Message.Should().Be("Building this implementation of IWebHostBuilder is not supported.", exception.Message);
    }
  }
}
