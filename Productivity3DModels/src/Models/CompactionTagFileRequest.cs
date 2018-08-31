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
  public class CompactionTagFileRequest : ProjectID
  {
    /// <summary>
    /// The name of the TAG file.
    /// </summary>
    /// <remarks>
    /// Shall contain only ASCII characters.
    /// </remarks>
    [JsonProperty(Required = Required.Always)]
    [ValidFilename(maxlength: 256)]
    public string FileName { get; set; }

    /// <summary>
    /// The content of the TAG file as an array of bytes.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public byte[] Data { get; set; }

    /// <summary>
    /// Defines Org ID (either from TCC or Connect) to support project-based subs
    /// </summary>
    public string OrgId { get; set; }


    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (Data == null || !Data.Any())
      {
          throw new ServiceException(HttpStatusCode.BadRequest,
                  new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                          "Data cannot be null"));
      }
    }
  }
}