using System;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;

namespace VSS.TRex.Gateway.Common.Helpers
{
  public static class ConvertVolumesHelper
  {
    /// <summary>
    /// Converts values of the VolumesType to ones of the VolumeComputationType.
    /// </summary>
    public static VolumeComputationType ConvertVolumesType(VolumesType volumesType)
    {
      switch (volumesType)
      {
        case VolumesType.None: return VolumeComputationType.None;
        case VolumesType.AboveLevel: return VolumeComputationType.AboveLevel;
        case VolumesType.Between2Levels: return VolumeComputationType.Between2Levels;
        case VolumesType.AboveFilter: return VolumeComputationType.AboveFilter;
        case VolumesType.Between2Filters: return VolumeComputationType.Between2Filters;
        case VolumesType.BetweenFilterAndDesign: return VolumeComputationType.BetweenFilterAndDesign;
        case VolumesType.BetweenDesignAndFilter: return VolumeComputationType.BetweenDesignAndFilter;
        default: throw new ArgumentException($"Unknown VolumesType {Convert.ToInt16(volumesType)}");
      }
    }
  }
}
