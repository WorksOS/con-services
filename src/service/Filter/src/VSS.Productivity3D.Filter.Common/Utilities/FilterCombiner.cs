using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Internal;
using VSS.Productivity3D.Filter.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Filter.Common.Models;

namespace VSS.Productivity3D.Filter.Common.Utilities
{
  /// <summary>
  /// Combines a set of filters expressed as filter Uids and combination roles into a single filter
  /// </summary>
  public static class FilterCombiner
  {
    /// <summary>
    /// Extracts a filter of a particular role from the request to use in filter combining operations
    /// </summary>
    /// <param name="request"></param>
    /// <param name="filters"></param>
    /// <param name="combinationRole"></param>
    /// <returns></returns>
    private static Abstractions.Models.Filter ExtractFilterFromRequest(FilterRequest request, List<MasterData.Repositories.DBModels.Filter> filters, FilterCombinationRole combinationRole)
    {
      var FilterUid = request.HierarchicFilterUids.SingleOrDefault(x => x.Role == combinationRole)?.FilterUid;
      if (string.IsNullOrEmpty(FilterUid))
        return null;

      var filterFromDB = filters.SingleOrDefault(x => string.Equals(x.FilterUid, FilterUid, StringComparison.OrdinalIgnoreCase));
      if (filterFromDB == null)
        return null;

      return JsonConvert.DeserializeObject<Abstractions.Models.Filter>(filterFromDB.FilterJson);
    }

    /// <summary>
    /// Combines the set of filters per the request into a single combined filter according to the roloe of each filter and
    /// the buisness rules relevant to how it contributes to the combined filter.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="filters"></param>
    /// <returns></returns>
    private static Abstractions.Models.Filter CombineFilters(FilterRequest request, List<MasterData.Repositories.DBModels.Filter> filters)
    {
      Abstractions.Models.Filter combinedFilter = null;

      var masterFilter = ExtractFilterFromRequest(request, filters, FilterCombinationRole.MasterFilter);
      if (masterFilter == null)
        return null;

      // Is there a widget filter?
      var widgetFilter = ExtractFilterFromRequest(request, filters, FilterCombinationRole.WidgetFilter);
      if (widgetFilter != null)
      {
        // Combine the widget filter into the combined filter
        combinedFilter = CombineWidgetFilterIntoMasterFilter(masterFilter, widgetFilter);
      }

      // Is there a volumes filter?
      var volumesFilter = ExtractFilterFromRequest(request, filters, FilterCombinationRole.VolumesFilter);
      if (volumesFilter != null)
      {
        // Combine the volumes filter into the combined filter
        combinedFilter = CombineFilterAndVolumeFilter(combinedFilter, volumesFilter);
      }

      return combinedFilter;
    }

    /// <summary>
    /// Takes a set of filters expressed as filter UIDs and combination roles and produces a JSON encoded filter
    /// representing the combined filters
    /// </summary>
    /// <param name="request"></param>
    /// <param name="repository"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static async Task<string> Combine(FilterRequestFull request, IFilterRepository repository, IServiceExceptionHandler serviceExceptionHandler, ILogger log)
    {
      var filters =
        (await repository.GetFiltersForProjectUser(request.CustomerUid, request.ProjectUid, request.UserId, true).ConfigureAwait(false))
        .Where(x => request.HierarchicFilterUids.Any(f => string.Equals(f.FilterUid, x.FilterUid, StringComparison.OrdinalIgnoreCase)))
        .ToList();

      if (filters.Count != request.HierarchicFilterUids.Count)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 11);
      }

      log.LogDebug($"CombineFilters retrieved {request.HierarchicFilterUids.Count} requested filters to combine");

      // Perform the operations to combine the selected filters
      return JsonConvert.SerializeObject(CombineFilters(request, filters));
    }

    private static Abstractions.Models.Filter CombineTwoFilters(Abstractions.Models.Filter parent, Abstractions.Models.Filter child)
    {

      var combinedFilter = new Abstractions.Models.Filter(
        elevationType: child.ElevationType ?? parent.ElevationType,
        layerNumber: child.LayerNumber ?? parent.LayerNumber,
        contributingMachines: child.ContributingMachines ?? parent.ContributingMachines,
        onMachineDesignId: child.OnMachineDesignId ?? parent.OnMachineDesignId,
        onMachineDesignName: child.OnMachineDesignName ?? parent.OnMachineDesignName,
        vibeStateOn: child.VibeStateOn ?? parent.VibeStateOn,
        forwardDirection: child.ForwardDirection ?? parent.ForwardDirection,
        designUid: child.DesignUid ?? parent.DesignUid,
        designFileName:child.DesignFileName ?? parent.DesignFileName,
        polygonUid: child.PolygonUid ?? parent.PolygonUid,
        passCountRangeMin: child.PassCountRangeMin ?? parent.PassCountRangeMin,
        passCountRangeMax: child.PassCountRangeMax ?? parent.PassCountRangeMax,
        temperatureRangeMin: child.TemperatureRangeMin ?? parent.TemperatureRangeMin,
        temperatureRangeMax: child.TemperatureRangeMax ?? parent.TemperatureRangeMax,
        automaticsType: child.AutomaticsType ?? parent.AutomaticsType,
        alignmentUid: child.AlignmentUid ?? parent.AlignmentUid,
        startStation: child.StartStation ?? parent.StartStation,
        endStation: child.EndStation ?? parent.EndStation,
        leftOffset: child.LeftOffset ?? parent.LeftOffset,
        rightOffset: child.RightOffset ?? parent.RightOffset,

        // Todo: These members are never override in the client code, so will be hard coded to null here until otherwise indicated
        polygonLL: null,
        polygonName: null,
        alignmentFileName: null,
        asAtDate: null,
        polygonType: null
        );

      // Any time constraint in the widget filter overrides the time constraint in the masterfilter.
      combinedFilter.DateRangeType = child.DateRangeType ?? parent.DateRangeType;
      combinedFilter.StartUtc = child?.DateRangeType == DateRangeType.Custom ? child.StartUtc : null;
      combinedFilter.EndUtc = child?.DateRangeType == DateRangeType.Custom ? child.EndUtc : null;

      return combinedFilter;
    }
    /// <summary>
    /// Combines the widget filter into the master filter, returning a modified master filter
    /// </summary>
    /// <param name="masterFilter"></param>
    /// <param name="widgetFilter"></param>
    private static Abstractions.Models.Filter CombineWidgetFilterIntoMasterFilter(Abstractions.Models.Filter masterFilter, Abstractions.Models.Filter widgetFilter)
    {
      // Create a new filter where all filter aspects are copied preferenially from the widget filter, and then the master filter.

      return CombineTwoFilters(masterFilter, widgetFilter);
    }

    /// <summary>
    /// Combine summary volume related elements from a 'volume' filter into another filter to produce a filter that
    /// is an extension of 'filter' with summary volume related aspects overridden from the 'volume' filter
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="volumeFilter"></param>
    /// <returns></returns>
    private static Abstractions.Models.Filter CombineFilterAndVolumeFilter(Abstractions.Models.Filter filter, Abstractions.Models.Filter volumeFilter)
    {
      // Currently, the volume role filter is intended to provide date range and volume design reference information, so these are the only
      // elements extracted from the volume filter and used to override matching elements in the combined filter

      var combinedFilter = new Abstractions.Models.Filter(
       elevationType: filter.ElevationType,
        layerNumber: filter.LayerNumber,
        contributingMachines:filter.ContributingMachines,
        onMachineDesignId: filter.OnMachineDesignId,
        onMachineDesignName: filter.OnMachineDesignName,
        vibeStateOn: filter.VibeStateOn,
        forwardDirection: filter.ForwardDirection,
        polygonUid: filter.PolygonUid,
        passCountRangeMin: filter.PassCountRangeMin,
        passCountRangeMax: filter.PassCountRangeMax,
        temperatureRangeMin: filter.TemperatureRangeMin,
        temperatureRangeMax: filter.TemperatureRangeMax,
        automaticsType: filter.AutomaticsType,
        alignmentUid: filter.AlignmentUid,
        startStation: filter.StartStation,
        endStation: filter.EndStation,
        leftOffset: filter.LeftOffset,
        rightOffset: filter.RightOffset,

        // Volume specific overrides:
        designUid: volumeFilter.DesignUid ?? filter.DesignUid,
        designFileName: volumeFilter.DesignFileName ?? filter.DesignFileName,

        // Todo: These members are never override in the client code, so will be hard coded to null here until otherwise indicated
        polygonLL: null,
        polygonName: null,
        alignmentFileName: null,
        asAtDate: null,
        polygonType: null
      );

      combinedFilter.DateRangeType = volumeFilter.DateRangeType ?? filter.DateRangeType;
      combinedFilter.StartUtc = volumeFilter?.DateRangeType == DateRangeType.Custom ? volumeFilter.StartUtc : null;
      combinedFilter.EndUtc = volumeFilter?.DateRangeType == DateRangeType.Custom ? volumeFilter.EndUtc : null;

      return combinedFilter;
    }
  }
}
