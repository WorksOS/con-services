using System;
using Newtonsoft.Json;

namespace VSS.Pegasus.Client.Models
{
  public class PegasusExecution
  {
    [JsonProperty(PropertyName = "id", Required = Required.Default)]
    public Guid Id { get; set; }
    [JsonProperty(PropertyName = "procedure_id", Required = Required.Default)]
    public Guid ProcedureId { get; set; }
    [JsonProperty(PropertyName = "parameters", Required = Required.Default)]
    public PegasusExecutionParameters Parameters { get; set; }
    [JsonProperty(PropertyName = "status", Required = Required.Default)]
    public string Status { get; set; }
    [JsonProperty(PropertyName = "execution_status", Required = Required.Default)]
    public string ExecutionStatus { get; set; }
    [JsonProperty(PropertyName = "latest_attempt", Required = Required.Default)]
    public PegasusExecutionAttempt LatestAttempt { get; set; }


  }
}
