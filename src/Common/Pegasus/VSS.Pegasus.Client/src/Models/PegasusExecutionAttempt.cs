using System;
using Newtonsoft.Json;

namespace VSS.Pegasus.Client.Models
{
  public class PegasusExecutionAttempt
  {
    [JsonProperty(PropertyName = "id", Required = Required.Default)]
    public Guid Id { get; set; }
    [JsonProperty(PropertyName = "status", Required = Required.Default)]
    public ExecutionStatus Status { get; set; }
    [JsonProperty(PropertyName = "bound_parameters", Required = Required.Default)]
    public PegasusBoundParameters BoundParameters { get; set; }
  }
}
