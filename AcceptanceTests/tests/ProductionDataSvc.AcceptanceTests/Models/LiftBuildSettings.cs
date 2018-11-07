namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// A collection of parameters and configuration information relating to analysis and determination of material layers.
  /// This is copied from ...\RaptorServicesCommon\Models\LiftBuildSettings.cs
  /// </summary>
  public class LiftBuildSettings
  {
    /// <summary>
    /// The range of valid CCV compactions (ie: complete) expressed as a percentage ranges of the target CCV value from the machine, or set as a global override
    /// </summary>
    public CCVRangePercentage cCVRange { get; set; }

    /// <summary>
    /// Produce CCV summary based on the information from the top most layer determined from the cell passes
    /// </summary>
    public bool cCVSummarizeTopLayerOnly { get; set; }

    /// <summary>
    /// The dead band lower boundary for determining if an existing layer has been removed. If a negative elevation change between two successive cell passes exceeds
    /// this value the layer is determined to be superceded and is remove. This value is used in layer detection methods that involved automatic layer detection.
    /// The value is expressed in meters.
    /// </summary>
    public double deadBandLowerBoundary { get; set; }

    /// <summary>
    /// The dead band upper boundary for determining if the next cell pass is the first cell pass on a new layer. If a positive elevation change between two successive cell passes 
    /// exceeds this value then a new layer is considered to have started. This value is used in layer detection methods that involved automatic layer detection.
    /// The value is expressed in meters.
    /// </summary>
    public double deadBandUpperBoundary { get; set; }

    /// <summary>
    /// The assumed thickess of material under the first pass of a machine over the ground in meters.
    /// </summary>
    public float firstPassThickness { get; set; }

    /// <summary>
    /// The type of layer/lift detection to be used when analysing the layers the group the cell passes together.
    /// </summary>
    public LiftDetectionType liftDetectionType { get; set; }

    /// <summary>
    /// Is the target layer thickness to be interpreted as compacted or uncompacted material
    /// </summary>
    public LiftThicknessType liftThicknessType { get; set; }

    /// <summary>
    /// The range of valid CCV compactions (ie: complete) expressed as a percentage ranges of the target CCV value from the machine, or set as a global override
    /// </summary>
    public MDPRangePercentage mDPRange { get; set; }

    /// <summary>
    /// Produce MDP summary based on the information from the top most layer determined from the cell passes
    /// </summary>
    public bool mDPSummarizeTopLayerOnly { get; set; }

    /// <summary>
    /// Selects mode how to summarize CCV data across layers.
    /// </summary>
    public CCVSummaryType CCvSummaryType { get; set; }

    /// <summary>
    /// The global override value for target lift thickness in meters. Optional.
    /// </summary>
    public float? overridingLiftThickness { get; set; }

    /// <summary>
    /// The global override value for target CCV. Optional.
    /// </summary>
    public short? overridingMachineCCV { get; set; }

    /// <summary>
    /// The global override value for target MDP. Optional.
    /// </summary>
    public short? overridingMachineMDP { get; set; }

    /// <summary>
    /// The global override value for target pass count. Optional.
    /// </summary>
    public TargetPassCountRange overridingTargetPassCountRange { get; set; }

    /// <summary>
    /// The global override value for temperature warning levels. Optional.
    /// </summary>
    public TemperatureWarningLevels overridingTemperatureWarningLevels { get; set; }

    /// <summary>
    /// A flag to turn on or off the automatic detection of superceded lifts. 
    /// Ignored when the lift detection type is None or Tagfile. Optional.
    /// </summary>
    public bool? includeSupersededLifts { get; set; }

    /// <summary>
    /// Specifies  the lift thickness target parameters. This is only used with TargetThicknessSummary diplay mode and summary report.
    /// </summary>
    /// <value>
    /// The lift thickness target.
    /// </value>
    public LiftThicknessTarget liftThicknessTarget { get; set; }

    /// <summary>
    /// Sets the machine speed target for Speed Summary requests. During this request Raptor does analysis of all cell passes (filtered out) and searches for the
    /// passes with speed above or below target values. If there is at least one cell pass satisfying the condition - this cell is considered bad.
    /// </summary>
    /// <value>
    /// The machine speed target.
    /// </value>
    public MachineSpeedTarget machineSpeedTarget { get; set; }
  }
}
