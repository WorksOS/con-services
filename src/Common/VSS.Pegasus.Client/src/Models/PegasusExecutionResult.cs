using Newtonsoft.Json;

namespace VSS.Pegasus.Client.Models
{
  public class PegasusExecutionResult
  {
    [JsonProperty(PropertyName = "execution", Required = Required.Default)]
    public PegasusExecution Execution { get; set; }
  }
}
