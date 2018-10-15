using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// The base result representation of detailed/summary Temperature request
  /// </summary>
  public class TemperatureBaseResult : ContractExecutionResult
  {
   
    /// <summary>
    /// Are the temperature target values applying to all processed cells constant?
    /// </summary>
    [JsonProperty(PropertyName = "TargetData")]
    public TemperatureTargetData TargetData { get; set; }


  }
}
