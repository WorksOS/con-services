using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
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
    {
    }

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

    public void Validate(IServiceExceptionHandler serviceExceptionHandler)
    {
      if (string.IsNullOrEmpty(filespaceId) || string.IsNullOrEmpty(path) ||
          string.IsNullOrEmpty(fileName))
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
      return string.Equals(filespaceId, other.filespaceId) && string.Equals(path, other.path) &&
             string.Equals(fileName, other.fileName);
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
