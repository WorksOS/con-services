using System;
using Microsoft.Extensions.Logging;
using VLPDDecls;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// Description to identify a file by its location in TCC.
  /// </summary>
  public static class FileDescriptorExtensions
  {
    /// <summary>
    /// Creates a Raptor design file descriptor
    /// </summary>
    /// <param name="configStore">Where to get environment variables, connection string etc. from</param>
    /// <param name="log">The Logger for logging</param>
    /// <param name="designId">The id of the design file</param>
    /// <param name="offset">The offset if the file is a reference surface</param>
    /// <returns>The Raptor design file descriptor</returns>
    public static TVLPDDesignDescriptor DesignDescriptor(this FileDescriptor descriptor, IConfigurationStore configStore, ILogger log, long designId,
      double offset)
    {
      string filespaceName = GetFileSpaceName(configStore, log);

      return __Global.Construct_TVLPDDesignDescriptor(designId, filespaceName, descriptor.FilespaceId, descriptor.Path, descriptor.FileName,
        offset);
    }

    /// <summary>
    /// Gets the TCC filespace name. The name is stored in an environment variable.
    /// </summary>
    /// <param name="configStore">Where to get environment variables, connection string etc. from</param>
    /// <param name="log">The Logger for logging</param>
    /// <returns>The TCC's file space name</returns>
    public static string GetFileSpaceName(IConfigurationStore configStore, ILogger log)
    {
      string filespaceName = configStore.GetValueString("TCCFILESPACENAME");

      if (string.IsNullOrEmpty(filespaceName))
      {
        var errorString = "Your application is missing an environment variable TCCFILESPACENAME";
        log.LogError(errorString);
        throw new InvalidOperationException(errorString);
      }

      return filespaceName;
    }

    /// <summary>
    /// Gets the TCC filespaceId for the vldatastore filespace. The ID is stored in an environment variable.
    /// </summary>
    /// <param name="configStore">Where to get environment variables, connection string etc. from</param>
    /// <param name="log">The Logger for logging</param>
    /// <returns>The TCC's file space identifier</returns>
    public static string GetFileSpaceId(IConfigurationStore configStore, ILogger log)
    {
      string fileSpaceIdStr = configStore.GetValueString("TCCFILESPACEID");

      if (string.IsNullOrEmpty(fileSpaceIdStr))
      {
        var errorString = "Your application is missing an environment variable TCCFILESPACEID";
        log.LogError(errorString);
        throw new InvalidOperationException(errorString);
      }

      return fileSpaceIdStr;
    }
  }
}
