using System;
using Newtonsoft.Json;

namespace VSS.Pegasus.Client.Models
{
  public class PegasusExecution
  {
    [JsonProperty(PropertyName = "procedure_id", Required = Required.Default)]
    public Guid ProcedureId { get; set; }
    [JsonProperty(PropertyName = "parameters", Required = Required.Default)]
    public PegasusExecutionParameters Parameters { get; set; }
  }
}
