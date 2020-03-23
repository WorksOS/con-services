using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;

namespace WebApiTests.Executors
{
  [TestClass]
  public class ProjectBoundariesAtDateExecutorTests : ExecutorBaseTests
  {
    [TestMethod]
    public async Task CanCallProjectBoundariesAtDateExecutorNoValidInput()
    {
      var projectBoundariesAtDateRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(-1, DateTime.UtcNow);
      var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(loggerFactory.CreateLogger<ProjectBoundariesAtDateExecutorTests>(), ConfigStore,
         cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object);
      var result = await executor.ProcessAsync(projectBoundariesAtDateRequest) as GetProjectBoundariesAtDateResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect ProjectBoundaries");
      Assert.AreEqual(0, result.projectBoundaries.Length, "executor returned incorrect ProjectBoundaries");
    }
   
  }
}
