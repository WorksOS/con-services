using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Validation;
using VSS.Productivity3D.Common.Interfaces;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// Description to identify a file by its location in TCC.
  /// </summary>
  public class FileDescriptor : IValidatable, IEquatable<FileDescriptor>
  {
    /// <summary>
    /// The id of the filespace in TCC where the file is located.
    /// </summary>
    [JsonProperty(PropertyName = "filespaceId", Required = Required.Always)]
    [Required]
    public string filespaceId { get; private set; }

    /// <summary>
    /// The full path of the file.
    /// </summary>
    [MaxLength(MAX_PATH)]
    [JsonProperty(PropertyName = "path", Required = Required.Always)]
    [Required]
    public string path { get; private set; }

    /// <summary>
    /// The name of the file.
    /// </summary>
    [ValidFilename(MAX_FILE_NAME)]
    [MaxLength(MAX_FILE_NAME)]
    [JsonProperty(PropertyName = "fileName", Required = Required.Always)]
    [Required]
    public string fileName { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private FileDescriptor()
    { }

    /// <summary>
    /// Create instance of FileDescriptor
    /// </summary>
    public static FileDescriptor CreateFileDescriptor
        (
          string filespaceId,
          string path,
          string fileName
        )
    {
      return new FileDescriptor
      {
        filespaceId = filespaceId,
        path = path,
        fileName = fileName
      };
    }

    public static FileDescriptor EmptyFileDescriptor { get; } = new FileDescriptor
    {
      filespaceId = string.Empty,
      path = string.Empty,
      fileName = string.Empty
    };

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (string.IsNullOrEmpty(filespaceId) || string.IsNullOrEmpty(path) ||
          string.IsNullOrEmpty(fileName))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  "Filespace Id, filespace name, path and file name are all required"));
      }

    }

    /// <summary>
    /// A string representation of a class instance.
    /// </summary>
    public override string ToString()
    {
      return $"{fileName}: {filespaceId}, {path}";
    }

    /// <summary>
    /// Creates a Raptor design file descriptor
    /// </summary>
    /// <param name="configStore">Where to get environment variables, connection string etc. from</param>
    /// <param name="log">The Logger for logging</param>
    /// <param name="designId">The id of the design file</param>
    /// <param name="offset">The offset if the file is a reference surface</param>
    /// <returns>The Raptor design file descriptor</returns>
    public TVLPDDesignDescriptor DesignDescriptor(IConfigurationStore configStore, ILogger log, long designId, double offset)
    {
      string filespaceName = GetFileSpaceName(configStore, log);

      return VLPDDecls.__Global.Construct_TVLPDDesignDescriptor(designId, filespaceName, filespaceId, path, fileName, offset);
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

    private const int MAX_FILE_NAME = 1024;
    private const int MAX_PATH = 2048;

    #region IEquatable
    public bool Equals(FileDescriptor other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return string.Equals(filespaceId, other.filespaceId) && string.Equals(path, other.path) && string.Equals(fileName, other.fileName);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((FileDescriptor)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = filespaceId != null ? filespaceId.GetHashCode() : 0;
        hashCode = (hashCode * 397) ^ (path != null ? path.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (fileName != null ? fileName.GetHashCode() : 0);
        return hashCode;
      }
    }

    public static bool operator ==(FileDescriptor left, FileDescriptor right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(FileDescriptor left, FileDescriptor right)
    {
      return !Equals(left, right);
    }
    #endregion
  }
}