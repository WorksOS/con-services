using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Cleanup;

namespace VSS.Productivity3D.Filter.Tests
{
  [TestClass]
  public class FilterCleanupTaskTests : ExecutorBaseTests
  {
    [TestMethod]
    public async Task FilterCleanup_NoneDeleted()
    {
      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var toDeleteCount = 0;

      var filterRepo = new Mock<IFilterRepository>();
      filterRepo.Setup(ps => ps.DeleteTransientFilters(It.IsAny<string>())).ReturnsAsync(toDeleteCount);

      var cleanupTask = new FilterCleanupTask(configStore, logger, serviceExceptionHandler, filterRepo.Object);

      var deletedCount = await cleanupTask.FilterCleanup().ConfigureAwait(false);
      Assert.AreEqual(toDeleteCount, deletedCount);
    }

    [TestMethod]
    public async Task FilterCleanup_OneDeleted()
    {
      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var toDeleteCount = 1;

      var filterRepo = new Mock<IFilterRepository>();
      filterRepo.Setup(ps => ps.DeleteTransientFilters(It.IsAny<string>())).ReturnsAsync(toDeleteCount);

      var cleanupTask = new FilterCleanupTask(configStore, logger, serviceExceptionHandler, filterRepo.Object);

      var deletedCount = await cleanupTask.FilterCleanup().ConfigureAwait(false);
      Assert.AreEqual(toDeleteCount, deletedCount);
    }

    [TestMethod]
    public async Task FilterCleanup_DBException()
    {
      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      
      var filterRepo = new Mock<IFilterRepository>();
      var dbException = new Exception("DB error of some kind");
      filterRepo.Setup(ps => ps.DeleteTransientFilters(It.IsAny<string>())).Throws(dbException); 

      var cleanupTask = new FilterCleanupTask(configStore, logger, serviceExceptionHandler, filterRepo.Object);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(async () => await cleanupTask.FilterCleanup().ConfigureAwait(false));
      Assert.AreEqual(HttpStatusCode.InternalServerError, ex.Code, "wrong HttpStatus code");
      Assert.AreEqual(2078, ex.GetResult.Code, "wrong status code");
      StringAssert.Contains(ex.GetResult.Message, "FilterCleanup: Exception occurred: DB error of some kind");
    }
  }
}
