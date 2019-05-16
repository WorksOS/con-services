using System.Threading.Tasks;
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
}
