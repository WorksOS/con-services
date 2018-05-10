using System.Net;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Models.ResultHandling
{
  /// <summary>
  /// Deatils of why a job has failed
  /// </summary>
  public class FailureDetails
  {
    /// <summary>
    /// Failure code
    /// </summary>
    [JsonProperty(PropertyName = "code", Required = Required.Always)]
    public HttpStatusCode Code { get; set; }
    /// <summary>
    /// Failure result
    /// </summary>
    [JsonProperty(PropertyName = "result", Required = Required.Always)]
    public ContractExecutionResult Result { get; set; }
  }
}
