using VSS.TRex.Common;

namespace VSS.TRex.Volumes
{
  public static class VolumesUtilities
  {
    /// <summary>
    /// Selects appropriate data sources for information based on filter or surface involvement in the 'from' and 'to'
    /// contexts in the volume computation.
    /// </summary>
    /// <param name="volumeType"></param>
    /// <param name="fromSelectionType"></param>
    /// <param name="toSelectionType"></param>
    public static void SetProdReportSelectionType(VolumeComputationType volumeType,
      out ProdReportSelectionType fromSelectionType, out ProdReportSelectionType toSelectionType)
    {
      // Set up the volumes calc parameters
      switch (volumeType)
      {
        case VolumeComputationType.Between2Filters:
          fromSelectionType = ProdReportSelectionType.Filter;
          toSelectionType = ProdReportSelectionType.Filter;
          break;

        case VolumeComputationType.BetweenFilterAndDesign:
          fromSelectionType = ProdReportSelectionType.Filter;
          toSelectionType = ProdReportSelectionType.Surface;
          break;

        case VolumeComputationType.BetweenDesignAndFilter:
          fromSelectionType = ProdReportSelectionType.Surface;
          toSelectionType = ProdReportSelectionType.Filter;
          break;

        default:
          fromSelectionType = ProdReportSelectionType.None;
          toSelectionType = ProdReportSelectionType.None;
          break;
      }
    }
  }
}
