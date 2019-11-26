using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using Moq;
using VSS.DataOcean.Client.ResultHandling;
using VSS.DataOcean.Client.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Serilog.Extensions;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.Notifications;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Cache.MemoryCache;

namespace VSS.DataOcean.Client.UnitTests
{
  public class BaseDataOceanClientCacheTests
  {
    protected IServiceProvider ServiceProvider;
    protected IServiceCollection ServiceCollection;

    public BaseDataOceanClientCacheTests()
    {
      ServiceCollection = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.DataOcean.Client.UnitTests.log")))
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddSingleton<IMemoryCache, MemoryCache>()
        .AddSingleton<IDataCache, InMemoryDataCache>()
        .AddSingleton<IDataOceanClient, DataOceanClient>()
        .AddSingleton(new Mock<IServiceResolution>().Object)
        .AddSingleton<INotificationHubClient, NotificationHubClient>();
      ServiceProvider = ServiceCollection.BuildServiceProvider();
    }

    public BrowseFilesResult SetupExpectedMutipleFileVersionsResult(Guid fileUid, string multiFileName, Guid? expectedFolderResultId, string downloadUrl)
    {
      var updatedAt = DateTime.UtcNow.AddHours(-2);

      var expectedFileResult = new DataOceanFile
      {
        Id = fileUid,
        Name = multiFileName,
        ParentId = expectedFolderResultId,
        Multifile = true,
        RegionPreferences = new List<string> { "us1" },
        Status = "AVAILABLE",
        DataOceanDownload = new DataOceanTransfer { Url = downloadUrl },
        UpdatedAt = updatedAt
      };

      var otherFileResult = new DataOceanFile
      {
        Id = fileUid,
        Name = multiFileName,
        ParentId = expectedFolderResultId,
        Multifile = true,
        RegionPreferences = new List<string> { "us1" },
        Status = "AVAILABLE",
        DataOceanDownload = new DataOceanTransfer { Url = downloadUrl },
        UpdatedAt = updatedAt.AddHours(-5)
      };

      return new BrowseFilesResult { Files = new List<DataOceanFile> { expectedFileResult, otherFileResult } };
    }

    public BrowseFilesResult SetupExpectedSingleFileVersionResult(Guid fileUid, string multiFileName, Guid? expectedFolderResultId, string downloadUrl)
    {
      var updatedAt = DateTime.UtcNow.AddHours(-2);

      var expectedFileResult = new DataOceanFile
      {
        Id = fileUid,
        Name = multiFileName,
        ParentId = expectedFolderResultId,
        Multifile = true,
        RegionPreferences = new List<string> { "us1" },
        Status = "AVAILABLE",
        DataOceanDownload = new DataOceanTransfer { Url = downloadUrl },
        UpdatedAt = updatedAt
      };

      return new BrowseFilesResult { Files = new List<DataOceanFile> { expectedFileResult } };
    }
  }
}
