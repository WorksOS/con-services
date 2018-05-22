using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum;

namespace WebApiTests.Executors
{
  [TestClass]
  public class ProjectBoundaryAtDateExecutorTests : ExecutorBaseTests
  {

    [TestMethod]
    public async Task CanCallProjectBoundaryAtDateExecutorNoValidInput()
    {
      GetProjectBoundaryAtDateRequest ProjectBoundaryAtDateRequest = GetProjectBoundaryAtDateRequest.CreateGetProjectBoundaryAtDateRequest(-1, DateTime.UtcNow);
      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      var executor = RequestExecutorContainer.Build<ProjectBoundaryAtDateExecutor>(loggerFactory.CreateLogger<ProjectBoundaryAtDateExecutorTests>(), configStore,
        assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionRepository);
      var result = await executor.ProcessAsync(ProjectBoundaryAtDateRequest) as GetProjectBoundaryAtDateResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(ContractExecutionStatesEnum.ExecutedSuccessfully, result.Code, "executor "); // todo does executed successfully mean it executed but may return nothing?

      Assert.IsNotNull(result.projectBoundary, "executor returned incorrect projectBoundary");
      Assert.IsNull(result.projectBoundary.FencePoints, "executor returned incorrect projectBoundary count");
    }

  }
}
