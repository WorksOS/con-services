using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.FileAccess.Enums;
using VSS.Common.Abstractions.FileAccess.Interfaces;

namespace VSS.Common.Abstractions.FileAccess
{
  public class DefaultVirtualFileSystemFactory : IVirtualFileSystemFactory
  {
    private readonly ILogger<DefaultVirtualFileSystemFactory> log;
    private readonly Dictionary<FileSystemEntries, IVirtualFileSystemRegistryEntry> fileSystemRegistry = new Dictionary<FileSystemEntries, IVirtualFileSystemRegistryEntry>();

    public DefaultVirtualFileSystemFactory(ILoggerFactory loggerFactory)
    {
      log = loggerFactory.CreateLogger<DefaultVirtualFileSystemFactory>();
    }

    public IEnumerable<FileSystemEntries> GetAvailableTags()
    {
      return fileSystemRegistry.Keys;
    }

    public Task<IVirtualFileSystem> GetFileSystem(FileSystemEntries tag)
    {
      if (fileSystemRegistry.ContainsKey(tag))
      {
        log.LogInformation($"Getting File System with Tag {tag} with type {fileSystemRegistry[tag].GetType().Name}");
        return fileSystemRegistry[tag].Create();
      }

      log.LogWarning($"Failed to find a File System with Tag {tag}");
      return Task.FromResult<IVirtualFileSystem>(null);
    }

    public Task<bool> RegisterFileSystem(IVirtualFileSystemRegistryEntry entry)
    {
      var tag = entry.Tag;
      if (fileSystemRegistry.ContainsKey(tag))
      {
        var existingType = fileSystemRegistry[tag].GetType().Name;
        log.LogWarning($"Registering File System with Tag {tag} to type {entry.GetType().Name}, replacing existing registration of type {existingType}");
      }
      else
      {
        log.LogInformation($"Registering File System with Tag {tag} to type {entry.GetType().Name}");
      }

      fileSystemRegistry[tag] = entry;

      return Task.FromResult(true);
    }
  }
}
