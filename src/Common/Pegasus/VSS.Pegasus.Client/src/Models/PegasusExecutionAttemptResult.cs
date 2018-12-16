using Newtonsoft.Json;

namespace VSS.Pegasus.Client.Models
{
  public class PegasusExecutionAttemptResult
  {
    [JsonProperty(PropertyName = "execution_attempt", Required = Required.Default)]
    public PegasusExecutionAttempt ExecutionAttempt { get; set; }
  }
}
