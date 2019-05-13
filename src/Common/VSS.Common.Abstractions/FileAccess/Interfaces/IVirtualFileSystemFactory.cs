using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.FileAccess.Enums;

namespace VSS.Common.Abstractions.FileAccess.Interfaces
{
  public interface IVirtualFileSystemFactory
  {
    /// <summary>
    /// Get tags that have file systems associated with them
    /// </summary>
    /// <returns></returns>
    IEnumerable<FileSystemEntries> GetAvailableTags();

    /// <summary>
    /// Get the Virtual File System for a given tag. Can be null.
    /// </summary>
    Task<IVirtualFileSystem> GetFileSystem(FileSystemEntries tag);

    /// <summary>
    /// Register a Virtual File System for a tag, will overwrite existing settings if it already exists.
    /// </summary>
    Task<bool> RegisterFileSystem(IVirtualFileSystemRegistryEntry entry);
  }
}
