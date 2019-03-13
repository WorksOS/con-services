using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.FIlters;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The request representation used to request export data.
  /// </summary>
  public class CompactionExportRequest 
  {
    /// <summary>
    /// A project unique identifier.
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Default)]
    public Guid ProjectUid { get; set; }

    /// <summary>
    /// The filter instance to be use in the request.
    /// </summary>
    /// <remarks>
    /// The value may be null.
    /// </remarks>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult Filter { get; set; }

    /// <summary>
    /// The name of the exported data file.
    /// </summary>
    /// <remarks>
    /// The value should contain only ASCII characters.
    /// </remarks>
    [JsonProperty(PropertyName = "fileName", Required = Required.Always)]
    [ValidFilename(256)]
    public string FileName { get; set; }


    /// <summary>
    /// Default private constructor.
    /// </summary>
    public CompactionExportRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="filter"></param>
    /// <param name="fileName"></param>
    public CompactionExportRequest(
      Guid projectUid,
      FilterResult filter,
      string fileName
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      FileName = fileName;
    }
    
    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (ProjectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid project UID."));
      }

      Filter?.Validate();
    }
  }
}
