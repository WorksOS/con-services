using Newtonsoft.Json;

namespace VSS.Pegasus.Client.Models
{
  public class CreateExecutionMessage
  {
    [JsonProperty(PropertyName = "execution", Required = Required.Always)]
    public PegasusExecution Execution { get; set; }
  }
}
