using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.TRex.Common;

namespace CCSS.Productivity3D.Service.Common
{
  public class FilterUtilities
  {
    private readonly ILogger _log;
    private readonly IFilterServiceProxy _filterServiceProxy;
    private readonly IDataCache _filterCache;
    private readonly DesignUtilities _designUtilities;

    private readonly MemoryCacheEntryOptions _filterCacheOptions = new MemoryCacheEntryOptions
    {
      SlidingExpiration = TimeSpan.FromDays(3)
    };

    public FilterUtilities(ILogger log, IConfigurationStore configStore, IFileImportProxy fileImportProxy, IFilterServiceProxy filterServiceProxy,
      IDataCache filterCache)
    {
      _log = log;
      _filterServiceProxy = filterServiceProxy;
      _filterCache = filterCache;
      _designUtilities = new DesignUtilities(log, configStore, fileImportProxy);
    }

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

    /// <summary>
    /// Creates an instance of the <see cref="FilterResult"/> class and populates it with data from the <see cref="Filter"/> model class.
    /// </summary>
    public async Task<FilterResult> GetCompactionFilter(
      Guid projectUid, string projectTimeZone, string userUid, Guid? filterUid, IHeaderDictionary customHeaders, 
      bool filterMustExist = false)
    {
      var filterKey = filterUid.HasValue ? $"{nameof(FilterResult)} {filterUid.Value}" : string.Empty;
      // Filter models are immutable except for their Name.
      // This service doesn't consider the Name in any of it's operations so we don't mind if our
      // cached object is out of date in this regard.
      var cachedFilter = filterUid.HasValue ? _filterCache.Get<FilterResult>(filterKey) : null;
      if (cachedFilter != null)
      {
        cachedFilter.ApplyDateRange(projectTimeZone, true);

        return cachedFilter;
      }

      var excludedSs = await _designUtilities.GetExcludedSurveyedSurfaceIds(projectUid, userUid, customHeaders);
      var excludedIds = excludedSs?.Select(e => e.Item1).ToList();
      var excludedUids = excludedSs?.Select(e => e.Item2).ToList();
      bool haveExcludedSs = excludedSs != null && excludedSs.Count > 0;

      if (!filterUid.HasValue)
      {
        if (haveExcludedSs)
          return FilterResult.CreateFilter(excludedIds, excludedUids);
        else
        {
          var filterResult = new FilterResult();
          filterResult.anyOfSurveyedSurfacesIncluded = await _designUtilities.AnyIncludedSurveyedSurface(projectUid, userUid, customHeaders);

          return filterResult;
        }
      }

      try
      {
        DesignDescriptor designDescriptor = null;
        DesignDescriptor alignmentDescriptor = null;

        var filterData = await GetFilterDescriptor(projectUid, filterUid.Value, customHeaders);

        if (filterMustExist && filterData == null)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Invalid Filter UID."));
        }

        if (filterData != null)
        {
          _log.LogDebug($"Filter from Filter Svc: {JsonConvert.SerializeObject(filterData)}");
          if (filterData.DesignUid != null && Guid.TryParse(filterData.DesignUid, out Guid designUidGuid))
          {
            designDescriptor = await _designUtilities.GetAndValidateDesignDescriptor(projectUid, designUidGuid, userUid, customHeaders);
          }

          if (filterData.AlignmentUid != null && Guid.TryParse(filterData.AlignmentUid, out Guid alignmentUidGuid))
          {
            alignmentDescriptor = await _designUtilities.GetAndValidateDesignDescriptor(projectUid, alignmentUidGuid, userUid, customHeaders);
          }

          if (filterData.HasData() || haveExcludedSs || designDescriptor != null)
          {
            filterData.ApplyDateRange(projectTimeZone, true);

            var layerMethod = filterData.LayerNumber.HasValue
              ? FilterLayerMethod.TagfileLayerNumber
              : FilterLayerMethod.None;

            bool? returnEarliest = null;
            if (filterData.ElevationType == ElevationType.First)
            {
              returnEarliest = true;
            }

            var raptorFilter = new FilterResult(filterUid, filterData, filterData.PolygonLL, alignmentDescriptor, layerMethod, excludedIds, excludedUids, returnEarliest, designDescriptor);

            _log.LogDebug($"Filter after filter conversion: {JsonConvert.SerializeObject(raptorFilter)}");

            // The filter will be removed from memory and recalculated to ensure we have the latest filter on any relevant changes
            var filterTags = new List<string>()
            {
              filterUid.Value.ToString(),
              projectUid.ToString()
            };

            _filterCache.Set(filterKey, raptorFilter, filterTags, _filterCacheOptions);

            return raptorFilter;
          }
        }
      }
      catch (ServiceException ex)
      {
        _log.LogDebug($"EXCEPTION caught - cannot find filter {ex.Message} {ex.GetContent} {ex.GetResult.Message}");
        throw;
      }
      catch (Exception ex)
      {
        _log.LogDebug("EXCEPTION caught - cannot find filter " + ex.Message);
        throw;
      }

      return haveExcludedSs ? FilterResult.CreateFilter(excludedIds, excludedUids) : null;
    }

    /// <summary>
    /// Gets the <see cref="Microsoft.AspNetCore.Mvc.Filters.FilterDescriptor"/> for a given Filter FileUid (by project).
    /// </summary>
    public async Task<Filter> GetFilterDescriptor(Guid projectUid, Guid filterUid, IHeaderDictionary customHeaders)
    {
      var filterDescriptor = await _filterServiceProxy.GetFilter(projectUid.ToString(), filterUid.ToString(), customHeaders);

      return filterDescriptor == null
        ? null
        : JsonConvert.DeserializeObject<Filter>(filterDescriptor.FilterJson);
    }
  }
}
