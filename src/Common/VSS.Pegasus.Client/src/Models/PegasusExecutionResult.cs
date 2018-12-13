using Newtonsoft.Json;

namespace VSS.Pegasus.Client.Models
{
  public class PegasusExecutionResult
  {
    [JsonProperty(PropertyName = "execution_attempt", Required = Required.Default)]
    public PegasusExecutionAttempt ExecutionAttempt { get; set; }
  }
}
