using System;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Filters.Interfaces
{
  public interface IFilteredValuePopulationControl
  {
    bool WantsTargetCCVValues { get; set; }
    bool WantsTargetPassCountValues { get; set; }
    bool WantsTargetLiftThicknessValues { get; set; }
    bool WantsEventDesignNameValues { get; set; }
    bool WantsEventVibrationStateValues { get; set; }
    bool WantsEventAutoVibrationStateValues { get; set; }
    bool WantsEventICFlagsValues { get; set; }
    bool WantsEventMachineGearValues { get; set; }
    bool WantsEventMachineCompactionRMVJumpThreshold { get; set; }
    bool WantsEventMachineAutomaticsValues { get; set; }
    bool WantsEventMapResetValues { get; set; }
    bool WantsEventMinElevMappingValues { get; set; }
    bool WantsEventInAvoidZoneStateValues { get; set; }
    bool WantsEventGPSAccuracyValues { get; set; }
    bool WantsEventPositioningTechValues { get; set; }
    bool WantsTempWarningLevelMinValues { get; set; }
    bool WantsTempWarningLevelMaxValues { get; set; }
    bool WantsTargetMDPValues { get; set; }
    bool WantsLayerIDValues { get; set; }
    bool WantsTargetCCAValues { get; set; }

    /// <summary>
    /// Determines if any of the population flags are set
    /// </summary>
    /// <returns></returns>
    bool AnySet();

    /// <summary>
    /// Sets all event population flags to false
    /// </summary>
    void Clear();

    /// <summary>
    /// Sets all event population flags to true
    /// </summary>
    void Fill();

    /// <summary>
    /// Converts the set of event population flags into a bit-flagged integer
    /// </summary>
    /// <returns></returns>
    int GetFlags();

    /// <summary>
    /// Converts a bit-flagged integer into the set of event population flags
    /// </summary>
    /// <param name="flags"></param>
    void SetFromFlags(uint flags);

    /// <summary>
    /// Prepares the set of event population control flags depending on the requested data type, filter, client grid
    /// and lift build related settings
    /// </summary>
    /// <param name="profileTypeRequired"></param>
    /// <param name="passFilter"></param>
    /// <param name="clientGrid"></param>
    void PreparePopulationControl(GridDataType profileTypeRequired,
      // todo const LiftBuildSettings: TICLiftBuildSettings;
      ICellPassAttributeFilter passFilter,
      IClientLeafSubGrid clientGrid);

    /// <summary>
    /// Prepares the set of event population control flags depending on the requested data type and filter
    /// </summary>
    /// <param name="profileTypeRequired"></param>
    /// <param name="passFilter"></param>
    void PreparePopulationControl(GridDataType profileTypeRequired,
      // todo const LiftBuildSettings: TICLiftBuildSettings;
      ICellPassAttributeFilter passFilter);
  }
}
