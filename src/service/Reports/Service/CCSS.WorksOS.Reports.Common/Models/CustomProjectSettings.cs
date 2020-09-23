using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class CustomProjectSettings : ContractExecutionResult
  {
    public ProjectSettings settings { get; set; }
  }
  public class ProjectSettings
  {
    [JsonProperty(PropertyName = "useMachineTargetPassCount", Required = Required.Default)]
    public bool? useMachineTargetPassCount { get; set; } = new bool?(true);

    [JsonProperty(PropertyName = "customTargetPassCountMinimum", Required = Required.Default)]
    public int? customTargetPassCountMinimum { get; set; }

    [JsonProperty(PropertyName = "customTargetPassCountMaximum", Required = Required.Default)]
    public int? customTargetPassCountMaximum { get; set; }

    [JsonProperty(PropertyName = "useMachineTargetTemperature", Required = Required.Default)]
    public bool? useMachineTargetTemperature { get; set; } = new bool?(true);

    [JsonProperty(PropertyName = "customTargetTemperatureMinimum", Required = Required.Default)]
    public double? customTargetTemperatureMinimum { get; set; }

    [JsonProperty(PropertyName = "customTargetTemperatureMaximum", Required = Required.Default)]
    public double? customTargetTemperatureMaximum { get; set; }

    [JsonProperty(PropertyName = "useMachineTargetCmv", Required = Required.Default)]
    public bool? useMachineTargetCmv { get; set; } = new bool?(true);

    [JsonProperty(PropertyName = "customTargetCmv", Required = Required.Default)]
    public double? customTargetCmv { get; set; }

    [JsonProperty(PropertyName = "useMachineTargetMdp", Required = Required.Default)]
    public bool? useMachineTargetMdp { get; set; } = new bool?(true);

    [JsonProperty(PropertyName = "customTargetMdp", Required = Required.Default)]
    public double? customTargetMdp { get; set; }

    [JsonProperty(PropertyName = "useDefaultTargetRangeCmvPercent", Required = Required.Default)]
    public bool? useDefaultTargetRangeCmvPercent { get; set; } = new bool?(true);

    [JsonProperty(PropertyName = "customTargetCmvPercentMinimum", Required = Required.Default)]
    public double? customTargetCmvPercentMinimum { get; set; }

    [JsonProperty(PropertyName = "customTargetCmvPercentMaximum", Required = Required.Default)]
    public double? customTargetCmvPercentMaximum { get; set; }

    [JsonProperty(PropertyName = "useDefaultTargetRangeMdpPercent", Required = Required.Default)]
    public bool? useDefaultTargetRangeMdpPercent { get; set; } = new bool?(true);

    [JsonProperty(PropertyName = "customTargetMdpPercentMinimum", Required = Required.Default)]
    public double? customTargetMdpPercentMinimum { get; set; }

    [JsonProperty(PropertyName = "customTargetMdpPercentMaximum", Required = Required.Default)]
    public double? customTargetMdpPercentMaximum { get; set; }

    [JsonProperty(PropertyName = "useDefaultTargetRangeSpeed", Required = Required.Default)]
    public bool? useDefaultTargetRangeSpeed { get; set; } = new bool?(true);

    [JsonProperty(PropertyName = "customTargetSpeedMinimum", Required = Required.Default)]
    public double? customTargetSpeedMinimum { get; set; }

    [JsonProperty(PropertyName = "customTargetSpeedMaximum", Required = Required.Default)]
    public double? customTargetSpeedMaximum { get; set; }

    [JsonProperty(PropertyName = "useDefaultCutFillTolerances", Required = Required.Default)]
    public bool? useDefaultCutFillTolerances { get; set; } = new bool?(true);

    [JsonProperty(PropertyName = "customCutFillTolerances", Required = Required.Default)]
    public List<double> customCutFillTolerances { get; set; }

    [JsonProperty(PropertyName = "useDefaultVolumeShrinkageBulking", Required = Required.Default)]
    public bool? useDefaultVolumeShrinkageBulking { get; set; } = new bool?(true);

    [JsonProperty(PropertyName = "customShrinkagePercent", Required = Required.Default)]
    public double? customShrinkagePercent { get; set; }

    [JsonProperty(PropertyName = "customBulkingPercent", Required = Required.Default)]
    public double? customBulkingPercent { get; set; }

    [JsonProperty(PropertyName = "useDefaultPassCountTargets", Required = Required.Default)]
    public bool? useDefaultPassCountTargets { get; set; } = new bool?(true);

    [JsonProperty(PropertyName = "customPassCountTargets", Required = Required.Default)]
    public List<int> customPassCountTargets { get; set; }

    [JsonProperty(PropertyName = "useDefaultCMVTargets", Required = Required.Default)]
    public bool? useDefaultCMVTargets { get; set; } = new bool?(true);

    [JsonProperty(PropertyName = "customCMVTargets", Required = Required.Default)]
    public List<int> customCMVTargets { get; set; }

    [JsonProperty(PropertyName = "useDefaultTemperatureTargets", Required = Required.Default)]
    public bool? useDefaultTemperatureTargets { get; set; } = new bool?(true);

    [JsonProperty(PropertyName = "customTemperatureTargets", Required = Required.Default)]
    public List<double> customTemperatureTargets { get; set; }
  }
}
