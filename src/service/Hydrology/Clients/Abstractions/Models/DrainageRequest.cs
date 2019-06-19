using Newtonsoft.Json;

namespace VSS.Hydrology.WebApi.Abstractions.Models
{
  public class DrainageRequest
  {
    [JsonProperty(PropertyName = "DesignFileName", Required = Required.Default)]
    public string DesignFileName { get; private set; }

    private DrainageRequest(string designFileName)
    {
      DesignFileName = designFileName;
    }

    public void Validate()
    {
    }
  }
}
