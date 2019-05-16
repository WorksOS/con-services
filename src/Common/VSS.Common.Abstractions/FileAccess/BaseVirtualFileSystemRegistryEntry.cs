using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.FileAccess.Enums;
using VSS.Common.Abstractions.FileAccess.Interfaces;

namespace VSS.Common.Abstractions.FileAccess
{
  public abstract class BaseVirtualFileSystemRegistryEntry : IVirtualFileSystemRegistryEntry
  {
    protected readonly IConfigurationStore configuration;
    protected readonly IServiceProvider serviceProvider;
    protected readonly ILogger log;

    protected BaseVirtualFileSystemRegistryEntry (FileSystemEntries entryTag, IConfigurationStore configuration, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
    {
      this.configuration = configuration;
      this.serviceProvider = serviceProvider;
      log = loggerFactory.CreateLogger(GetType());
      Tag = entryTag;
    }

    public abstract Task<IVirtualFileSystem> Create();

    public FileSystemEntries Tag { get; }
  }
}
