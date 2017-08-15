using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.Productivity3D.Common.Filters.Validation;
using VSS.Productivity3D.Common.Interfaces;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// Description to identify a file by its location in TCC.
  /// </summary>
  public class FileDescriptor : IValidatable
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
    {}

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
    /// Create example instance of FileDescriptor to display in Help documentation.
    /// </summary>
    public static FileDescriptor HelpSample => new FileDescriptor
    {
      filespaceId = "u72003136-d859-4be8-86de-c559c841bf10",
      path = "BC Data/Sites/Integration10/Designs",
      fileName = "Cycleway.ttm"
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
    /// <param name="configStore">Where to get environment variables, connection string etc. frome</param>
    /// <param name="log">The Logger for logging</param>
    /// <param name="designId">The id of the design file</param>
    /// <param name="offset">The offset if the file is a reference surface</param>
    /// <returns></returns>
    public TVLPDDesignDescriptor DesignDescriptor(IConfigurationStore configStore, ILogger log, long designId, double offset)
    {
      string filespaceName = configStore.GetValueString("TCCFILESPACENAME");

      if (string.IsNullOrEmpty(filespaceName))
      {
        var errorString = "Your application is missing an environment variable TCCFILESPACENAME";
        log.LogError(errorString);
        throw new InvalidOperationException(errorString);
      }
      return VLPDDecls.__Global.Construct_TVLPDDesignDescriptor(designId, filespaceName, filespaceId, path, fileName, offset);
    }

    private const int MAX_FILE_NAME = 1024;
    private const int MAX_PATH = 2048;
  }
}