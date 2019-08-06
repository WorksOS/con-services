using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.Productivity3D.Models.Enums;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// Settings used for lift or layer analysis. These are used by v1 requests (TBC).
  /// </summary>
  public class LiftSettings
  {
    private const float MIN_THICKNESS = 0.0f;
    private const float MAX_THICKNESS = 100.0f;
    private const float MAX_THICKNESS_ABOVE_TOLERANCE = 1.0f;
    private const float MAX_THICKNESS_BELOW_TOLERANCE = 2.0f;

    /// <summary>
    /// Produce CCV summary based on the information from the top most layer determined from the cell passes
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public bool CCVSummarizeTopLayerOnly { get; private set; }

    /// <summary>
    /// Produce MDP summary based on the information from the top most layer determined from the cell passes
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public bool MDPSummarizeTopLayerOnly { get; private set; }

    /// <summary>
    /// Selects mode how to summarize CCV data across layers.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public SummaryType CCVSummaryType { get; private set; }

    /// <summary>
    /// Selects mode how to summarize MDP data across layers.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public SummaryType MDPSummaryType { get; private set; }

    /// <summary>
    /// The assumed thickness of material under the first pass of a machine over the ground in meters.
    /// </summary>
    [Range(MIN_THICKNESS, MAX_THICKNESS)]
    [JsonProperty(Required = Required.Default)]
    public float FirstPassThickness { get; private set; }

    /// <summary>
    /// The type of layer/lift detection to be used when analyzing the layers the group the cell passes together.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public LiftDetectionType LiftDetectionType { get; private set; }

    /// <summary>
    /// Is the target layer thickness to be interpreted as compacted or uncompacted material
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public LiftThicknessType LiftThicknessType { get; private set; }

    /// <summary>
    /// Specifies  the lift thickness target parameters. This is only used with TargetThicknessSummary display mode and summary report.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public LiftThicknessTarget LiftThicknessTarget { get; private set; }

    /// <summary>
    /// Override the target thickness recorded from the machine with the value of OverridingLiftThickness
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public bool OverrideMachineThickness { get; private set; }

    /// <summary>
    /// The global override value for target lift thickness in meters. Optional.
    /// </summary>
    [Range(MIN_THICKNESS, MAX_THICKNESS)]
    [JsonProperty(Required = Required.Default)]
    public float OverridingLiftThickness { get; private set; }

    /// <summary>
    /// A flag to turn on or off the automatic detection of superceded lifts. 
    /// Ignored when the lift detection type is None or Tagfile. Optional.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public bool IncludeSupersededLifts { get; private set; }

    // Boundaries extending above/below a cell pass constituting the dead band

    /// <summary>
    /// The dead band lower boundary for determining if an existing layer has been removed. If a negative elevation change between two successive cell passes exceeds
    /// this value the layer is determined to be superseded and is removed. This value is used in layer detection methods that involved automatic layer detection.
    /// The value is expressed in meters.
    /// </summary>
    [Range(MIN_THICKNESS, MAX_THICKNESS_BELOW_TOLERANCE)]
    [JsonProperty(Required = Required.Default)]
    public double DeadBandLowerBoundary { get; private set; }

    /// <summary>
    /// The dead band upper boundary for determining if the next cell pass is the first cell pass on a new layer. If a positive elevation change between two successive cell passes 
    /// exceeds this value then a new layer is considered to have started. This value is used in layer detection methods that involved automatic layer detection.
    /// The value is expressed in meters.
    /// </summary>
    [Range(MIN_THICKNESS, MAX_THICKNESS_ABOVE_TOLERANCE)]
    [JsonProperty(Required = Required.Default)]
    public double DeadBandUpperBoundary { get; private set; }

    /// <summary>
    /// Default public constructor.
    /// </summary>
    public LiftSettings()
    {
      LiftDetectionType = LiftDetectionType.None;
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public LiftSettings(
      bool ccvSummarizeTopLayerOnly,
      bool mdpSummarizeTopLayerOnly,
      SummaryType ccvSummaryType,
      SummaryType mdpSummaryType,
      float firstPassThickness,
      LiftDetectionType liftDetectionType,
      LiftThicknessType liftThicknessType,
      LiftThicknessTarget liftThicknessTarget,
      bool overrideMachineThickness,
      float overridingLiftThickness,
      bool includeSupersededLifts,
      double deadBandLowerBoundary,
      double deadBandUpperBoundary
      )
    {
      CCVSummarizeTopLayerOnly = ccvSummarizeTopLayerOnly;
      MDPSummarizeTopLayerOnly = mdpSummarizeTopLayerOnly;
      CCVSummaryType = ccvSummaryType;
      MDPSummaryType = mdpSummaryType;
      FirstPassThickness = firstPassThickness;
      LiftDetectionType = liftDetectionType;
      LiftThicknessType = liftThicknessType;
      LiftThicknessTarget = liftThicknessTarget;
      OverrideMachineThickness = overrideMachineThickness;
      OverridingLiftThickness = overridingLiftThickness;
      IncludeSupersededLifts = includeSupersededLifts;
      DeadBandLowerBoundary = deadBandLowerBoundary;
      DeadBandUpperBoundary = deadBandUpperBoundary;
    }

    /// <summary>
    /// Validates all properties.
    /// </summary>
    public void Validate()
    {
      LiftThicknessTarget?.Validate();
    }
  }
}
