using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using VSS.Productivity3D.AssetMgmt3D;
using Xunit;

namespace AssetMgmt.IntegrationTests.Controllers
{
  public class StartupTests: IClassFixture<WebApplicationFactory<Startup>>
    {
      private readonly WebApplicationFactory<Startup> _factory;

      public StartupTests(WebApplicationFactory<Startup> fixture)
      {
        _factory = fixture;
      }

      [Fact]
      public void GenericWebHostThrowsOnBuild()
      {
        var exception = Assert.Throws<NotSupportedException>(() =>
        {
          var hostBuilder = new HostBuilder()
            .ConfigureWebHost(builder =>
            {
              builder.UseStartup<Startup>();
              builder.Build();
            });
        });

        Assert.Equal("Building this implementation of IWebHostBuilder is not supported.", exception.Message);
      }
    }
}
