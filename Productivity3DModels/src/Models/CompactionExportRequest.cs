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
  public class CompactionExportRequest : ProjectID
  {
    /// <summary>
    /// The filter instance to be use in the request.
    /// </summary>
    /// <remarks>
    /// The value may be null.
    /// </remarks>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult Filter { get; private set; }

    /// <summary>
    /// The name of the exported data file.
    /// </summary>
    /// <remarks>
    /// The value should contain only ASCII characters.
    /// </remarks>
    [JsonProperty(PropertyName = "fileName", Required = Required.Always)]
    [ValidFilename(256)]
    public string FileName { get; private set; }

    /// <summary>
    /// Sets the tolerance to calculate TIN surfaces.
    /// </summary>
    /// <remarks>
    /// The value should be in meters.
    /// </remarks>
    [JsonProperty(PropertyName = "tolerance", Required = Required.Default)]
    public double? Tolerance { get; private set; }

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private CompactionExportRequest()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="filter"></param>
    /// <param name="tolerance"></param>
    /// <param name="fileName"></param>
    public CompactionExportRequest(
      Guid? projectUid,
      FilterResult filter,
      double tolerance,
      string fileName
    )
    {
      ProjectUid = projectUid;
      Filter = filter;
      Tolerance = tolerance;
      FileName = fileName;
    }
    
    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      Filter?.Validate();

      if (FileName == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "File name must be provided"));
      }
    }
  }
}
