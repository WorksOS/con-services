using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.FIlters;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Models.Models
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
    public string FilespaceId { get; private set; }

    /// <summary>
    /// The full path of the file.
    /// </summary>
    [MaxLength(MAX_PATH)]
    [JsonProperty(PropertyName = "path", Required = Required.Always)]
    public string Path { get; private set; }

    /// <summary>
    /// The name of the file.
    /// </summary>
    [ValidFilename(MAX_FILE_NAME)]
    [MaxLength(MAX_FILE_NAME)]
    [JsonProperty(PropertyName = "fileName", Required = Required.Always)]
    public string FileName { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private FileDescriptor()
    { }

    /// <summary>
    /// Create instance of FileDescriptor
    /// </summary>
    public static FileDescriptor CreateFileDescriptor(string filespaceId, string path, string fileName)
    {
      return new FileDescriptor
      {
        FilespaceId = filespaceId,
        Path = path,
        FileName = fileName
      };
    }

    /// <summary>
    /// Create instance of FileDescriptor
    /// Where the path is /{customerUid}/{projectUid}
    /// </summary>
    public static FileDescriptor CreateFileDescriptor
    (
      string filespaceId,
      string customerUid,
      string projectUid,
      string fileName
    )
    {
      return CreateFileDescriptor(filespaceId, $"/{customerUid}/{projectUid}", fileName);
    }

    public static FileDescriptor EmptyFileDescriptor { get; } = new FileDescriptor
    {
      FilespaceId = string.Empty,
      Path = string.Empty,
      FileName = string.Empty
    };

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (string.IsNullOrEmpty(FilespaceId) || string.IsNullOrEmpty(Path) ||
          string.IsNullOrEmpty(FileName))
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
      return $"{FileName}: {FilespaceId}, {Path}";
    }

    public void Validate(IServiceExceptionHandler serviceExceptionHandler)
    {
      if (string.IsNullOrEmpty(FilespaceId) || string.IsNullOrEmpty(Path) ||
          string.IsNullOrEmpty(FileName))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest,
          ContractExecutionStatesEnum.ValidationError,
          "Filespace Id, filespace name, path and file name are all required");
      }
    }

    private const int MAX_FILE_NAME = 1024;
    private const int MAX_PATH = 2048;

    #region IEquatable

    public bool Equals(FileDescriptor other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return string.Equals(FilespaceId, other.FilespaceId) && string.Equals(Path, other.Path) &&
             string.Equals(FileName, other.FileName);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((FileDescriptor) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = FilespaceId != null ? FilespaceId.GetHashCode() : 0;
        hashCode = (hashCode * 397) ^ (Path != null ? Path.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (FileName != null ? FileName.GetHashCode() : 0);
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
