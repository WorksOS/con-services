using VSS.TRex.Common.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Types.Types;

namespace VSS.TRex.Common.Models
{
  public interface ILiftParameters : IFromToBinary
  {
    bool OverrideMachineThickness { get; set; }
    LiftThicknessType LiftThicknessType { get; set; }
    double OverridingLiftThickness { get; set; }
    CCVSummaryTypes CCVSummaryTypes { get; set; }
    bool CCVSummarizeTopLayerOnly { get; set; }
    float FirstPassThickness { get; set; }
    MDPSummaryTypes MDPSummaryTypes { get; set; }
    bool MDPSummarizeTopLayerOnly { get; set; }
    LiftDetectionType LiftDetectionType { get; set; }
    bool IncludeSuperseded { get; set; }
    double TargetLiftThickness { get; set; }
    double AboveToleranceLiftThickness { get; set; }
    double BelowToleranceLiftThickness { get; set; }
    double DeadBandLowerBoundary { get; set; }
    double DeadBandUpperBoundary { get; set; }
  }
}
