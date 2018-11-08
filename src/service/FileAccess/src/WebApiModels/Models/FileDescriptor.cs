using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.FileAccess.WebAPI.Models.Models
{
  /// <summary>
  /// Description to identify a file by its location in TCC.
  /// </summary>
  public class FileDescriptor : IValidatable
  {
    private const int MAX_FILE_NAME = 1024;
    private const int MAX_PATH = 2048;

    /// <summary>
    /// The id of the filespace in TCC where the file is located.
    /// </summary>
    [JsonProperty(PropertyName = "filespaceId", Required = Required.Always)]
    [Required]
    public string FilespaceId { get; private set; }

    /// <summary>
    /// The full path of the file.
    /// </summary>
    [MaxLength(MAX_PATH)]
    [JsonProperty(PropertyName = "path", Required = Required.Always)]
    [Required]
    public string Path { get; private set; }

    /// <summary>
    /// The name of the file.
    /// </summary>
    [MaxLength(MAX_FILE_NAME)]
    [JsonProperty(PropertyName = "fileName", Required = Required.Always)]
    [Required]
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
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (string.IsNullOrEmpty(FilespaceId) ||
          string.IsNullOrEmpty(Path) ||
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
      throw new NotImplementedException();
    }
  }
}
