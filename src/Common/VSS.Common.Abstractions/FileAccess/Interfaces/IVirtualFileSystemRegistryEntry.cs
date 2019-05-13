using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.FileAccess.Enums;

namespace VSS.Common.Abstractions.FileAccess.Interfaces
{
  public interface IVirtualFileSystemRegistryEntry
  {    
    /// <summary>
    /// A specific tag to identify the repository, related to use case.
    /// </summary>
    FileSystemEntries Tag { get; }

    /// <summary>
    /// Create a new instance of the FileSystem for use.
    /// </summary>
    Task<IVirtualFileSystem> Create();
  }

  public abstract class VirtualFileSystemRegistryEntry : IVirtualFileSystemRegistryEntry
  {
    protected readonly IConfigurationStore configuration;
    protected readonly ILogger log;

    protected VirtualFileSystemRegistryEntry(FileSystemEntries entryTag, IConfigurationStore configuration, ILoggerFactory loggerFactory)
    {
      this.configuration = configuration;
      log = loggerFactory.CreateLogger(GetType());
      Tag = entryTag;
    }

    public abstract Task<IVirtualFileSystem> Create();

    public FileSystemEntries Tag { get; }
  }
}
