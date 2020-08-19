using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling
{
  public class ConfigResult : ContractExecutionResult, IMasterDataModel
  {
    /// <summary>
    /// Provides current TRex configuration. Currently a string == "OK"
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public string Configuration { get; private set; }


    public ConfigResult(string config)
    {
      Configuration = config;
    }

    public List<string> GetIdentifiers() => new List<string>();

  }
}
