using System.Linq;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.FIlters;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// TAG file domain object. Model represents TAG file submitted to Raptor.
  /// </summary>
  public class LineworkFileRequest : ProjectID
  {
    /// <summary>
    /// The name of the DXF linework file.
    /// </summary>
    /// <remarks>
    /// Shall contain only ASCII characters.
    /// </remarks>
    [JsonProperty(Required = Required.Always)]
    [ValidFilename(256)]
    public string FileName { get; set; }

    /// <summary>
    /// The content of the DXF linework file as an array of bytes.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public byte[] Data { get; set; }

    /// <summary>
    /// Validates required request properties.
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      if (Data == null || !Data.Any())
      {
          throw new ServiceException(HttpStatusCode.BadRequest,
                  new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                          "Data cannot be null"));
      }
    }
  }
}
