using System.Net;
using System.Threading.Tasks;
using FileAccess.IntegrationTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.Productivity3D.FileAccess.WebAPI.Controllers;
using VSS.Productivity3D.FileAccess.WebAPI.Models.Models;
using VSS.TCCFileAccess;
using Xunit;

namespace FileAccess.IntegrationTests.Controllers
{
  public class FileAccessControllerTests : TestBase
    {
      private readonly ServiceProvider serviceProvider;

      public FileAccessControllerTests()
      {
        serviceProvider = new ServiceCollection()
                           .AddSingleton(Log)
                           .AddSingleton<IConfigurationStore, GenericConfiguration>()
                           .AddSingleton<IFileRepository, MockFileRepository>()
                           .BuildServiceProvider();
      }

      [Fact]
      public async Task ShouldDownloadFile()
      {
        var controller = new FileAccessController(Log, new MockFileRepository(Log))
                         {
                           ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = serviceProvider } }
                         };

        var requestModel = FileDescriptor.CreateFileDescriptor("5u8472cda0-9f59-41c9-a5e2-e19f922f91d8", "/77561/1158", "Large Sites Road - Trimble Road.ttm");

        var response = await controller.GetFile(requestModel) as OkObjectResult;

        Assert.NotNull(response);
        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        Assert.IsType<FileStreamResult>(response.Value);
      }

      [Fact]
      public async Task ShouldReturnFailedDownloadResponse()
      {
        var controller = new FileAccessController(Log, new MockFileRepository(Log))
                         {
                           ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = serviceProvider } }
                         };

        var requestModel = FileDescriptor.CreateFileDescriptor("u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01", "/77561/1158", "IDontExist.ttm");

        var response = await controller.GetFile(requestModel) as NoContentResult;

        Assert.NotNull(response);
        Assert.Equal((int)HttpStatusCode.NoContent, response.StatusCode);
      }
    }
}
