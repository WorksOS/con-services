using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.FileAccess;
using VSS.Common.Abstractions.FileAccess.Enums;
using VSS.Common.Abstractions.FileAccess.Interfaces;

namespace VSS.TCCFileAccess.VirtualFileSystem
{
  public class TccVirtualFileSystemRegistryEntry : BaseVirtualFileSystemRegistryEntry
  {
    public TccVirtualFileSystemRegistryEntry(FileSystemEntries entryTag, IConfigurationStore configuration, ILoggerFactory loggerFactory, IServiceProvider serviceProvider) 
      : base(entryTag, configuration, loggerFactory, serviceProvider)
    {
    }

    public override Task<IVirtualFileSystem> Create()
    {
      var result = ActivatorUtilities.CreateInstance<TccVirtualFileSystem>(serviceProvider);
      return Task.FromResult<IVirtualFileSystem>(result);
    }
  }
}
