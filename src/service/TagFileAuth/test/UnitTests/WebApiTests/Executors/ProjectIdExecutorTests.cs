using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;

namespace WebApiTests.Executors
{
  [TestClass]
  public class ProjectIdExecutorTests : ExecutorBaseTests
  {
    [TestMethod]
    public async Task CanCallProjectIDExecutorNoValidInput()
    {
      var ProjectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(-1, 91, 181, DateTime.MinValue);
      var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<ProjectIdExecutor>(loggerFactory.CreateLogger<ProjectIdExecutorTests>(), ConfigStore,
         cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, authorizationProxy.Object);
      var result = await executor.ProcessAsync(ProjectIdRequest) as GetProjectIdResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(-1, result.projectId, "executor returned incorrect legacy ProjectId");
    }

  }
}
