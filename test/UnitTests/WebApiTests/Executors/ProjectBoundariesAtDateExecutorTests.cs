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
  public class ProjectBoundariesAtDateExecutorTests : ExecutorBaseTests
  {

    [TestMethod]
    public async Task CanCallProjectBoundariesAtDateExecutorNoValidInput()
    {
      GetProjectBoundariesAtDateRequest ProjectBoundariesAtDateRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(-1, DateTime.UtcNow);
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(loggerFactory.CreateLogger<ProjectBoundariesAtDateExecutorTests>(), configStore,
        assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionRepository);
      var result = await executor.ProcessAsync(ProjectBoundariesAtDateRequest) as GetProjectBoundariesAtDateResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect ProjectBoundaries");
      Assert.AreEqual(0, result.projectBoundaries.Length, "executor returned incorrect ProjectBoundaries");
    }
   
  }
}
