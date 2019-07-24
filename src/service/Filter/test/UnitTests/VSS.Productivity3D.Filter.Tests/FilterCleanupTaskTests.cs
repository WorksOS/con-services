using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Cleanup;
using Xunit;

namespace VSS.Productivity3D.Filter.Tests
{
  public class FilterCleanupTaskTests : IClassFixture<ExecutorBaseTests>
  {
    private readonly ExecutorBaseTests _classFixture;
    private IServiceProvider serviceProvider => _classFixture.serviceProvider;
    private IServiceExceptionHandler serviceExceptionHandler => _classFixture.serviceExceptionHandler;

    public FilterCleanupTaskTests(ExecutorBaseTests classFixture)
    {
      _classFixture = classFixture;
    }

    [Fact]
    public async Task FilterCleanup_NoneDeleted()
    {
      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var toDeleteCount = 0;

      var filterRepo = new Mock<IFilterRepository>();
      filterRepo.Setup(ps => ps.DeleteTransientFilters(It.IsAny<string>())).ReturnsAsync(toDeleteCount);

      var cleanupTask = new FilterCleanupTask(configStore, logger, serviceExceptionHandler, filterRepo.Object);

      var deletedCount = await cleanupTask.FilterCleanup().ConfigureAwait(false);
      Assert.Equal(toDeleteCount, deletedCount);
    }

    [Fact]
    public async Task FilterCleanup_OneDeleted()
    {
      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var toDeleteCount = 1;

      var filterRepo = new Mock<IFilterRepository>();
      filterRepo.Setup(ps => ps.DeleteTransientFilters(It.IsAny<string>())).ReturnsAsync(toDeleteCount);

      var cleanupTask = new FilterCleanupTask(configStore, logger, serviceExceptionHandler, filterRepo.Object);

      var deletedCount = await cleanupTask.FilterCleanup().ConfigureAwait(false);
      Assert.Equal(toDeleteCount, deletedCount);
    }

    [Fact]
    public async Task FilterCleanup_DBException()
    {
      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      
      var filterRepo = new Mock<IFilterRepository>();
      var dbException = new Exception("DB error of some kind");
      filterRepo.Setup(ps => ps.DeleteTransientFilters(It.IsAny<string>())).Throws(dbException); 

      var cleanupTask = new FilterCleanupTask(configStore, logger, serviceExceptionHandler, filterRepo.Object);

      var ex = await Assert.ThrowsAsync<ServiceException>(async () => await cleanupTask.FilterCleanup().ConfigureAwait(false));
      Assert.Equal(HttpStatusCode.InternalServerError, ex.Code);
      Assert.Equal(2078, ex.GetResult.Code);
      Assert.Contains("FilterCleanup: Exception occurred: DB error of some kind", ex.GetResult.Message);
    }
  }
}
