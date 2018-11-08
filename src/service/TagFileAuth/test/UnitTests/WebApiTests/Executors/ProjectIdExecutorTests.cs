using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

namespace WebApiTests.Executors
{
  [TestClass]
  public class ProjectIdExecutorTests : ExecutorBaseTests
  {

    [TestMethod]
    public async Task CanCallProjectIDExecutorNoValidInput()
    {
      GetProjectIdRequest ProjectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(-1, 91, 181, 0, DateTime.MinValue, "");
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<ProjectIdExecutor>(loggerFactory.CreateLogger<ProjectIdExecutorTests>(), configStore,
        assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionRepository);
      var result = await executor.ProcessAsync(ProjectIdRequest) as GetProjectIdResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(-1, result.projectId, "executor returned incorrect legacy ProjectId");
    }

  }
}