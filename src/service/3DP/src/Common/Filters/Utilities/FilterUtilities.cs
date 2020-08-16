using System;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common;

namespace VSS.Productivity3D.Common.Filters.Utilities
{
  public static class FilterUtilities
  {

    public static (FilterResult baseFilter, FilterResult topFilter) AdjustFilterToFilter(FilterResult baseFilter, FilterResult topFilter)
    {
      //Special case for Raptor filter to filter comparisons.
      //If base filter is earliest and top filter is latest with a DateTime filter then replace
      //base filter with latest with a date filter with the start date at the beginning of time and 
      //the end date at the original start date. This is to avoid losing data between original start date
      //and first event after the start date with data.
      if (baseFilter.HasTimeComponent() && (baseFilter.ReturnEarliest ?? false) &&
          topFilter.HasTimeComponent() && !(topFilter.ReturnEarliest ?? false))
      {
        //Note: filters are cached so we need to make a copy to adjust.
        var newTopFilter = new FilterResult(topFilter);
        newTopFilter.ElevationType = null;

        // Adjust the base filter accordingly
        var newBaseFilter = new FilterResult(baseFilter);
        newBaseFilter.EndUtc = baseFilter.StartUtc;
        newBaseFilter.StartUtc = Consts.MIN_DATETIME_AS_UTC;
        newBaseFilter.ReturnEarliest = false;
        newBaseFilter.ElevationType = null;

        return (baseFilter: newBaseFilter, topFilter: newTopFilter);
      }
      return (baseFilter, topFilter);
    }

    /// <summary>
    /// Ensures there is not a misconfigured topFilter for certain operations that involve design surfaces for tile rendering operations
    /// </summary>
    public static FilterResult ReconcileTopFilterAndVolumeComputationMode(
      FilterResult topFilter,
      DisplayMode mode,
      VolumesType computeVolType)
    {
      // Adjust filter to take into account volume type computations that effect Cut/Fill, Volume and Thickness requests. 
      // If these requests invovle a design through the appropriate volume computation modes, the topFilter has no effect
      // and must be made safe so the underlying engines do not receive conflicting instructions between a specified design
      // and a top filter indication one of the comparative surfaces used by these requests
      if ((mode == DisplayMode.CutFill || mode == DisplayMode.VolumeCoverage || mode == DisplayMode.TargetThicknessSummary)
          &&
          (computeVolType == VolumesType.BetweenDesignAndFilter || computeVolType == VolumesType.BetweenFilterAndDesign))
      {
        // Force topfilter (which is filter2) to be a plain empty filter to remove any default
        // setting such as the LayerType to percolate through into the request.
        return new FilterResult();
      }

      return topFilter;
    }

    /// <summary>
    /// Ensures there is not a misconfigured topFilter for certain operations that involve design surfaces for volume computation operations
    /// </summary>
    public static FilterResult ReconcileTopFilterAndVolumeComputationMode(FilterResult topFilter, VolumesType computeVolType)
    {
      // Adjust filter to take into account volume computations with respect to designs
      // If these requests invovle a design through the appropriate volume computation modes, the topFilter has no effect
      // and must be made safe so the underlying engines do not receive conflicting instructions between a specified design
      // and a top filter indication one of the comparative surfaces used by these requests
      if ((computeVolType == VolumesType.BetweenDesignAndFilter) || (computeVolType == VolumesType.BetweenFilterAndDesign))
      {
        // Force topfilter (which is filter2) to be a plain empty filter to remove any default
        // setting such as the LayerType to percolate through into the request.
        return new FilterResult();
      }

      return topFilter;
    }

    /// <summary>
    /// Ensures there is not a misconfigured filter2 for certain operations that involve design surfaces for tile rendering operations
    /// </summary>
    public static (FilterResult baseFilter, FilterResult topFilter) ReconcileTopFilterAndVolumeComputationMode(
      FilterResult filter1,
      FilterResult filter2,
      DisplayMode mode,
      VolumesType computeVolType)
    {
      // Adjust filter to take into account volume type computations that effect Cut/Fill, Volume and Thickness requests. 
      // If these requests involve a design through the appropriate volume computation modes, either the topFilter or the baseFilter
      // has no effect depending on the style of filter/design and design/filter chosen 
      // and must be made safe so the underlying engines do not receive conflicting instructions between a specified design
      // and a filter used by these requests
      if (mode == DisplayMode.CutFill || mode == DisplayMode.VolumeCoverage || mode == DisplayMode.TargetThicknessSummary)
      {
        if (computeVolType == VolumesType.BetweenDesignAndFilter)
        {
          // Force topfilter to be a plain empty filter to remove any default
          // setting such as the LayerType to percolate through into the request.
          return (baseFilter: filter1, topFilter: new FilterResult());
        }

        if (computeVolType == VolumesType.BetweenFilterAndDesign)
        {
          // Force basefilter to be a plain empty filter to remove any default
          // setting such as the LayerType to percolate through into the request.
          return (baseFilter: filter1, topFilter: new FilterResult());
        }
      }
      return (baseFilter: filter1, topFilter: filter2);
    }

    /// <summary>
    /// Ensures there is not a misconfigured topFilter or baseFilter for certain operations that involve design surfaces for volume computation operations
    /// </summary>
    public static (FilterResult baseFilter, FilterResult topFilter) ReconcileTopFilterAndVolumeComputationMode(
      FilterResult baseFilter,
      FilterResult topFilter,
      VolumesType computeVolType)
    {
      // Adjust filter to take into account volume type computations respect to designs. 
      // If these requests involve a design through the appropriate volume computation modes, either the topFilter or the baseFilter
      // has no effect depending on the style of filter/design and design/filter chosen 
      // and must be made safe so the underlying engines do not receive conflicting instructions between a specified design
      // and a filter used by these requests

      if (computeVolType == VolumesType.BetweenDesignAndFilter)
      {
        // Force topfilter to be a plain empty filter to remove any default
        // setting such as the LayerType to percolate through into the request.
        return (baseFilter: new FilterResult(), topFilter);
      }

      if (computeVolType == VolumesType.BetweenFilterAndDesign)
      {
        // Force basefilter to be a plain empty filter to remove any default
        // setting such as the LayerType to percolate through into the request.
        return (baseFilter, topFilter: new FilterResult());
      }

      return (baseFilter, topFilter);
    }
  }
}
