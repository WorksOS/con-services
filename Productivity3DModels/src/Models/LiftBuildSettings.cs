using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using VSS.Productivity3D.Models.Enums;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// A collection of parameters and configuration information relating to analysis and determination of material layers.
  /// </summary>
  public class LiftBuildSettings 
  {
    private const short MIN_TARGET_MDP = 0;
    private const short MAX_TARGET_MDP = 10000;
    private const short MIN_TARGET_CCV = 0;
    private const short MAX_TARGET_CCV = 1000;
    private const float MIN_THICKNESS = 0.0f;
    private const float MAX_THICKNESS = 100.0f;
    private const float MAX_THICKNESS_ABOVE_TOLERANCE = 1.0f;
    private const float MAX_THICKNESS_BELOW_TOLERANCE = 2.0f;

    /// <summary>
    /// The range of valid CCV compactions (ie: complete) expressed as a percentage ranges of the target CCV value from the machine, or set as a global override
    /// </summary>
    [JsonProperty(PropertyName = "cCVRange", Required = Required.Default)]
    public CCVRangePercentage CCVRange { get; private set; }

    /// <summary>
    /// Produce CCV summary based on the information from the top most layer determined from the cell passes
    /// </summary>
    [JsonProperty(PropertyName = "cCVSummarizeTopLayerOnly", Required = Required.Default)]
    public bool CCVSummarizeTopLayerOnly { get; private set; }

    /// <summary>
    /// Selects mode how to summarize CCV data across layers.
    /// </summary>
    [JsonProperty(PropertyName = "CCVSummaryType", Required = Required.Default)]
    public CCVSummaryType? CCvSummaryType { get; private set; }


    /// <summary>
    /// The dead band lower boundary for determining if an existing layer has been removed. If a negative elevation change between two successive cell passes exceeds
    /// this value the layer is determined to be superseded and is removed. This value is used in layer detection methods that involved automatic layer detection.
    /// The value is expressed in meters.
    /// </summary>
    [Range(MIN_THICKNESS, MAX_THICKNESS_BELOW_TOLERANCE)]
    [JsonProperty(PropertyName = "deadBandLowerBoundary", Required = Required.Default)]
    public double DeadBandLowerBoundary { get; private set; }

    /// <summary>
    /// The dead band upper boundary for determining if the next cell pass is the first cell pass on a new layer. If a positive elevation change between two successive cell passes 
    /// exceeds this value then a new layer is considered to have started. This value is used in layer detection methods that involved automatic layer detection.
    /// The value is expressed in meters.
    /// </summary>
    [Range(MIN_THICKNESS, MAX_THICKNESS_ABOVE_TOLERANCE)]
    [JsonProperty(PropertyName = "deadBandUpperBoundary", Required = Required.Default)]
    public double DeadBandUpperBoundary { get; private set; }

    /// <summary>
    /// The assumed thickess of material under the first pass of a machine over the ground in meters.
    /// </summary>
    [Range(MIN_THICKNESS, MAX_THICKNESS)]
    [JsonProperty(PropertyName = "firstPassThickness", Required = Required.Default)]
    public float FirstPassThickness { get; private set; }

    /// <summary>
    /// The type of layer/lift detection to be used when analysing the layers the group the cell passes together.
    /// </summary>
    [JsonProperty(PropertyName = "liftDetectionType", Required = Required.Default)]
    public LiftDetectionType LiftDetectionType { get; private set; }

    /// <summary>
    /// Is the target layer thickness to be interpreted as compacted or uncompacted material
    /// </summary>
    [JsonProperty(PropertyName = "liftThicknessType", Required = Required.Default)]
    public LiftThicknessType LiftThicknessType { get; private set; }

    /// <summary>
    /// The range of valid CCV compactions (ie: complete) expressed as a percentage ranges of the target CCV value from the machine, or set as a global override
    /// </summary>
    [JsonProperty(PropertyName = "mDPRange", Required = Required.Default)]
    public MDPRangePercentage MDPRange { get; private set; }

    /// <summary>
    /// Produce MDP summary based on the information from the top most layer determined from the cell passes
    /// </summary>
    [JsonProperty(PropertyName = "mDPSummarizeTopLayerOnly", Required = Required.Default)]
    public bool MDPSummarizeTopLayerOnly { get; private set; }

    /// <summary>
    /// The global override value for target lift thickness in meters. Optional.
    /// </summary>
    [Range(MIN_THICKNESS, MAX_THICKNESS)]
    [JsonProperty(PropertyName = "overridingLiftThickness", Required = Required.Default)]
    public float? OverridingLiftThickness { get; private set; }

    /// <summary>
    /// The global override value for target CCV. Optional.
    /// </summary>
    [Range(MIN_TARGET_CCV, MAX_TARGET_CCV)]
    [JsonProperty(PropertyName = "overridingMachineCCV", Required = Required.Default)]
    public short? OverridingMachineCCV { get; private set; }

    /// <summary>
    /// The global override value for target MDP. Optional.
    /// </summary>
    [Range(MIN_TARGET_MDP, MAX_TARGET_MDP)]
    [JsonProperty(PropertyName = "overridingMachineMDP", Required = Required.Default)]
    public short? OverridingMachineMDP { get; private set; }

    /// <summary>
    /// The global override value for target pass count range. Optional.
    /// </summary>
    [JsonProperty(PropertyName = "overridingTargetPassCountRange", Required = Required.Default)]
    public TargetPassCountRange OverridingTargetPassCountRange { get; private set; }

    /// <summary>
    /// The global override value for temperature warning levels. Optional.
    /// </summary>
    [JsonProperty(PropertyName = "overridingTemperatureWarningLevels", Required = Required.Default)]
    public TemperatureWarningLevels OverridingTemperatureWarningLevels { get; private set; }

    /// <summary>
    /// A flag to turn on or off the automatic detection of superceded lifts. 
    /// Ignored when the lift detection type is None or Tagfile. Optional.
    /// </summary>
    [JsonProperty(PropertyName = "includeSupersededLifts", Required = Required.Default)]
    public bool? IncludeSupersededLifts { get; private set; }

    /// <summary>
    /// Specifies  the lift thickness target parameters. This is only used with TargetThicknessSummary diplay mode and summary report.
    /// </summary>
    /// <value>
    /// The lift thickness target.
    /// </value>
    [JsonProperty(PropertyName = "liftThicknessTarget", Required = Required.Default)]
    public LiftThicknessTarget LiftThicknessTarget { get; private set; }

    /// <summary>
    /// Sets the machine speed target for Speed Summary requests. During this request Raptor does analysis of all cell passes (filtered out) and searches for the
    /// passes with speed above or below target values. If there is at least one cell pass satisfying the condition - this cell is considered bad.
    /// </summary>
    /// <value>
    /// The machine speed target.
    /// </value>
    [JsonProperty(PropertyName = "machineSpeedTarget", Required = Required.Default)]
    public MachineSpeedTarget MachineSpeedTarget { get; private set; }

    /// <summary>
    /// Default private constructor
    /// </summary>
    private LiftBuildSettings()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="cCVRange"></param>
    /// <param name="cCVSummarizeTopLayerOnly"></param>
    /// <param name="deadBandLowerBoundary"></param>
    /// <param name="deadBandUpperBoundary"></param>
    /// <param name="firstPassThickness"></param>
    /// <param name="liftDetectionType"></param>
    /// <param name="liftThicknessType"></param>
    /// <param name="mDPRange"></param>
    /// <param name="mDPSummarizeTopLayerOnly"></param>
    /// <param name="overridingLiftThickness"></param>
    /// <param name="overridingMachineCCV"></param>
    /// <param name="overridingMachineMDP"></param>
    /// <param name="overridingTargetPassCountRange"></param>
    /// <param name="overridingTemperatureWarningLevels"></param>
    /// <param name="includeSupersededLifts"></param>
    /// <param name="liftThicknessTarget"></param>
    /// <param name="machineSpeedTarget"></param>
    public LiftBuildSettings
    (
      CCVRangePercentage cCVRange,
      bool cCVSummarizeTopLayerOnly,
      double deadBandLowerBoundary,
      double deadBandUpperBoundary,
      float firstPassThickness,
      LiftDetectionType liftDetectionType,
      LiftThicknessType liftThicknessType,
      MDPRangePercentage mDPRange,
      bool mDPSummarizeTopLayerOnly,
      float? overridingLiftThickness,
      short? overridingMachineCCV,
      short? overridingMachineMDP,
      TargetPassCountRange overridingTargetPassCountRange,
      TemperatureWarningLevels overridingTemperatureWarningLevels,
      bool? includeSupersededLifts,
      LiftThicknessTarget liftThicknessTarget,
      MachineSpeedTarget machineSpeedTarget
    )
    {
      CCVRange = cCVRange;
      CCVSummarizeTopLayerOnly = cCVSummarizeTopLayerOnly;
      DeadBandLowerBoundary = deadBandLowerBoundary;
      DeadBandUpperBoundary = deadBandUpperBoundary;
      FirstPassThickness = firstPassThickness;
      LiftDetectionType = liftDetectionType;
      LiftThicknessType = liftThicknessType;
      MDPRange = mDPRange;
      MDPSummarizeTopLayerOnly = mDPSummarizeTopLayerOnly;
      OverridingLiftThickness = overridingLiftThickness;
      OverridingMachineCCV = overridingMachineCCV;
      OverridingMachineMDP = overridingMachineMDP;
      OverridingTargetPassCountRange = overridingTargetPassCountRange;
      OverridingTemperatureWarningLevels = overridingTemperatureWarningLevels;
      IncludeSupersededLifts = includeSupersededLifts;
      LiftThicknessTarget = liftThicknessTarget;
      MachineSpeedTarget = machineSpeedTarget;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      CCVRange?.Validate();
      MDPRange?.Validate();
      OverridingTemperatureWarningLevels?.Validate();
      OverridingTargetPassCountRange?.Validate();
      LiftThicknessTarget?.Validate();
      MachineSpeedTarget?.Validate();
    }
  }
}