using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Repositories;
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
      var FilterUid = request.FilterUids.SingleOrDefault(x => x.Role == combinationRole)?.FilterUid;
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

      // Is there a dashboard filter?
      var dashboardFilter = ExtractFilterFromRequest(request, filters, FilterCombinationRole.DashboardFilter);
      if (dashboardFilter != null)
      {
        // Combine the dashboard filter into the combined filter
        combinedFilter = CombineDashboardFilterIntoMasterFilter(masterFilter, dashboardFilter);
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
        .Where(x => request.FilterUids.Any(f => string.Equals(f.FilterUid, x.FilterUid, StringComparison.OrdinalIgnoreCase)))
        .ToList();

      if (filters.Count != request.FilterUids.Count)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 11);
      }

      log.LogDebug($"CombineFilters retrieved {request.FilterUids.Count} requested filters to combine");

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

      // Any time constraint in the dashboard filter overrides the time constraint in the masterfilter.
      combinedFilter.DateRangeType = child.DateRangeType ?? parent.DateRangeType;
      combinedFilter.StartUtc = child?.DateRangeType == DateRangeType.Custom ? child.StartUtc : null;
      combinedFilter.EndUtc = child?.DateRangeType == DateRangeType.Custom ? child.EndUtc : null;

      return combinedFilter;
    }
    /// <summary>
    /// Combines the dashboard filter into the master filter, returning a modified master filter
    /// </summary>
    /// <param name="masterFilter"></param>
    /// <param name="dashboardFilter"></param>
    private static Abstractions.Models.Filter CombineDashboardFilterIntoMasterFilter(Abstractions.Models.Filter masterFilter, Abstractions.Models.Filter dashboardFilter)
    {
      // Create a new filter where all filter aspects are copied preferenially from the dashboard filter, and then the master filter.

      return CombineTwoFilters(masterFilter, dashboardFilter);
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



/*
 Logic from VL UI app:

   * Create payload with list of applied filters and return the filter object  
   * @param mapAppliedFilters 
   * @param selectedDateTime 
   * @param mapAppliedMachineFilters
 
    public getQueryParamsForMapFilters(mapAppliedFilters: MapFilterState, selectedDateTime, mapAppliedMachineFilters, persistDate = true) : any {
    let mapParams: any = {};
    if (mapAppliedFilters && mapAppliedFilters.savedFilter && mapAppliedFilters.savedFilter.isUpdatedFilterStatus) {
      let selectedDateUTC = angular.copy(this.Productivity3dDateRangeService.getSelectedDateTimeUTC());
      if (selectedDateUTC.selectionType === "custom" || selectedDateUTC.selectionType === "Custom" || selectedDateUTC.selectionType === "ProjectExtents") {
        mapParams = {
          startUtc: selectedDateUTC.startUtc,
          endUtc: selectedDateUTC.endUtc,
          dateRangeType: (selectedDateUTC.selectionType === "custom" || selectedDateUTC.selectionType === "Custom") ? 7 : 6
        };
      } else {
        mapParams = {
          dateRangeType: this.Productvity3dFilter.getDateRangeEnumType(selectedDateUTC.selectionType)
        };
      }
    } else {
      if (selectedDateTime.selectionType === "custom" || selectedDateTime.selectionType === "Custom" || selectedDateTime.selectionType === "ProjectExtents") {
        mapParams = {
          startUtc: selectedDateTime.startUtc,
          endUtc: selectedDateTime.endUtc,
          dateRangeType: (selectedDateTime.selectionType === "custom" || selectedDateTime.selectionType === "Custom") ? 7 : 6
        };
      } else {
        mapParams = {
          dateRangeType: this.Productvity3dFilter.getDateRangeEnumType(selectedDateTime.selectionType)
        };
      }

      if (persistDate) {
        this.Productivity3dDateRangeService.setSelectedDateTimeUTC(selectedDateTime);
      }
    }
    if (mapAppliedFilters) {
      if (mapAppliedFilters.liftNames && mapAppliedFilters.liftNames.criteria) {
        mapParams.layerNumber = mapAppliedFilters.liftNames.selectedCriteria()[0];
      }

      if (mapAppliedFilters.elevation && mapAppliedFilters.elevation.criteria) {
        mapParams.elevationType = mapAppliedFilters.elevation.selectedCriteria().length > 0 ? mapAppliedFilters.elevation.selectedCriteria()[0] : null;
      }

      if (mapAppliedFilters.machineNames && mapAppliedFilters.machineNames.selectedCriteria().length > 0) {
        mapParams.contributingMachines = mapAppliedMachineFilters;
      }

      if (mapAppliedFilters.machineDesign && mapAppliedFilters.machineDesign.criteria) {
        mapParams.onMachineDesignId = mapAppliedFilters.machineDesign.selectedCriteria()[0];
        mapParams.onMachineDesignName = mapAppliedFilters.machineDesign.selectedCriteriaNames()[0];
      }

      if (mapAppliedFilters.vibeState && mapAppliedFilters.vibeState.criteria) {
        if (mapAppliedFilters.vibeState.selectedCriteria().length > 0) {
          mapParams.vibeStateOn = JSON.parse(mapAppliedFilters.vibeState.selectedCriteria()[0]);
        }
      }

      if (mapAppliedFilters.direction && mapAppliedFilters.direction.criteria) {
        if (mapAppliedFilters.direction.selectedCriteria().length > 0) {
          mapParams.forwardDirection = JSON.parse(mapAppliedFilters.direction.selectedCriteria()[0]);
        }
      }

      if (mapAppliedFilters.designBoundary && mapAppliedFilters.designBoundary.criteria) {
        if (mapAppliedFilters.designBoundary.selectedCriteria().length > 0) {
          mapParams.designUid = mapAppliedFilters.designBoundary.selectedCriteria()[0];
        }
      }

      if (mapAppliedFilters.boundaryFilter && mapAppliedFilters.boundaryFilter.criteria) {
        if (mapAppliedFilters.boundaryFilter.selectedCriteria().length > 0) {
          mapParams.polygonUid = mapAppliedFilters.boundaryFilter.selectedCriteria()[0];
        }
      }

      if (mapAppliedFilters.alignmentFilter && mapAppliedFilters.alignmentFilter.criteria) {
        if (mapAppliedFilters.alignmentFilter.selectedCriteria().length > 0) {
          mapParams.alignmentUid = mapAppliedFilters.alignmentFilter.selectedCriteria()[0];
          this.setStartEndStation(mapAppliedFilters, mapParams, "startStation", "defaultStartStation");
          this.setStartEndStation(mapAppliedFilters, mapParams, "endStation", "defaultEndStation");
           mapParams.leftOffset = this.CommonUtilityService.convertToMeters(mapAppliedFilters.alignmentFilter.leftOffset);
           mapParams.rightOffset = this.CommonUtilityService.convertToMeters(mapAppliedFilters.alignmentFilter.rightOffset);
         }
      }

      if (mapAppliedFilters.passcountFilter && mapAppliedFilters.passcountFilter.lowerPasscount && mapAppliedFilters.passcountFilter.upperPasscount) {
        mapParams.passCountRangeMin = mapAppliedFilters.passcountFilter.lowerPasscount;
        mapParams.passCountRangeMax = mapAppliedFilters.passcountFilter.upperPasscount;
      }

      if (mapAppliedFilters.temperatureRangeFilter && mapAppliedFilters.temperatureRangeFilter.tempRangeMinVal >= 0 && mapAppliedFilters.temperatureRangeFilter.tempRangeMaxVal >= 0) {
        mapParams.temperatureRangeMin = this.CommonUtilityService.convertToCelcius(mapAppliedFilters.temperatureRangeFilter.tempRangeMinVal, false);
        mapParams.temperatureRangeMax = this.CommonUtilityService.convertToCelcius(mapAppliedFilters.temperatureRangeFilter.tempRangeMaxVal, false);
      }

      if (mapAppliedFilters.automaticsFilter.selectedCriteria().length > 0) {
        mapParams.automaticsType = JSON.parse(mapAppliedFilters.automaticsFilter.selectedCriteria()[0]);
      }
    }
    return JSON.stringify(mapParams);
  }
  */

/*
   public updateInactiveState(headerFilterState: MapFilterState, widgetFilterState: MapFilterState): any {
    let mergedFilterState = new MapFilterState(), activeGlobalFilterCount = 0;
    if (widgetFilterState.isDateRangeNotSelected) {
        headerFilterState.isInactiveDateRange = false;
        mergedFilterState.dateRangeFilter = angular.copy(headerFilterState.dateRangeFilter);
        activeGlobalFilterCount += 1;
    } else {
        headerFilterState.isInactiveDateRange = true;
        mergedFilterState.dateRangeFilter = angular.copy(widgetFilterState.dateRangeFilter);
    }
    
    if (widgetFilterState.liftNames.selectedCriteria().length > 0) {
        headerFilterState.liftNames.isInactive = true;
        mergedFilterState.liftNames = angular.copy(widgetFilterState.liftNames);
    } else {
        headerFilterState.liftNames.isInactive = false;
        mergedFilterState.liftNames = angular.copy(headerFilterState.liftNames);
        activeGlobalFilterCount += headerFilterState.liftNames.selectedCriteria().length;
    }
    
    if (widgetFilterState.elevation.selectedCriteria().length > 0) {
        headerFilterState.elevation.isInactive = true;
        mergedFilterState.elevation = angular.copy(widgetFilterState.elevation);
    } else {
        headerFilterState.elevation.isInactive = false;
        mergedFilterState.elevation = angular.copy(headerFilterState.elevation);
        activeGlobalFilterCount += headerFilterState.elevation.selectedCriteria().length;
    }
    
    if (widgetFilterState.machineNames.selectedCriteria().length > 0) {
        headerFilterState.machineNames.isInactive = true;
        mergedFilterState.machineNames = angular.copy(widgetFilterState.machineNames);
        mergedFilterState["filteredMachinesData"] = widgetFilterState["filteredMachinesData"];
    } else {
        headerFilterState.machineNames.isInactive = false;
        mergedFilterState.machineNames = angular.copy(headerFilterState.machineNames);
        mergedFilterState["filteredMachinesData"] = headerFilterState["headerFilteredMachinesData"];
        activeGlobalFilterCount += headerFilterState.machineNames.selectedCriteria().length;
    }
    
    if (widgetFilterState.machineDesign.selectedCriteria().length > 0) {
        headerFilterState.machineDesign.isInactive = true;
        mergedFilterState.machineDesign = angular.copy(widgetFilterState.machineDesign);
    } else {
        headerFilterState.machineDesign.isInactive = false;
        mergedFilterState.machineDesign = angular.copy(headerFilterState.machineDesign);
        activeGlobalFilterCount += headerFilterState.machineDesign.selectedCriteria().length;
    }
    
    if (widgetFilterState.vibeState.selectedCriteria().length > 0) {
        headerFilterState.vibeState.isInactive = true;
        mergedFilterState.vibeState = angular.copy(widgetFilterState.vibeState);
    } else {
        headerFilterState.vibeState.isInactive = false;
        mergedFilterState.vibeState = angular.copy(headerFilterState.vibeState);
        activeGlobalFilterCount += headerFilterState.vibeState.selectedCriteria().length;
    }
    
    if (widgetFilterState.direction.selectedCriteria().length > 0) {
        headerFilterState.direction.isInactive = true;
        mergedFilterState.direction = angular.copy(widgetFilterState.direction);
    } else {
        headerFilterState.direction.isInactive = false;
        mergedFilterState.direction = angular.copy(headerFilterState.direction);
        activeGlobalFilterCount += headerFilterState.direction.selectedCriteria().length;
    }
    
    if (widgetFilterState.designBoundary.selectedCriteria().length > 0) {
        headerFilterState.designBoundary.isInactive = true;
        mergedFilterState.designBoundary = angular.copy(widgetFilterState.designBoundary);
    } else {
        headerFilterState.designBoundary.isInactive = false;
        mergedFilterState.designBoundary = angular.copy(headerFilterState.designBoundary);
        activeGlobalFilterCount += headerFilterState.designBoundary.selectedCriteria().length;
    }
    
    if (widgetFilterState.boundaryFilter.selectedCriteria().length > 0) {
        headerFilterState.boundaryFilter.isInactive = true;
        mergedFilterState.boundaryFilter = angular.copy(widgetFilterState.boundaryFilter);
    } else {
        headerFilterState.boundaryFilter.isInactive = false;
        mergedFilterState.boundaryFilter = angular.copy(headerFilterState.boundaryFilter);
        activeGlobalFilterCount += headerFilterState.boundaryFilter.selectedCriteria().length;
    }
    
    if (widgetFilterState.alignmentFilter.selectedCriteria().length > 0) {
        headerFilterState.alignmentFilter.isInactive = true;
        mergedFilterState.alignmentFilter = angular.copy(widgetFilterState.alignmentFilter);
    } else {
        headerFilterState.alignmentFilter.isInactive = false;
        mergedFilterState.alignmentFilter = angular.copy(headerFilterState.alignmentFilter);
        activeGlobalFilterCount += headerFilterState.alignmentFilter.selectedCriteria().length;
    }
    
    if (widgetFilterState.temperatureRangeFilter.tempRangeMinVal >= 0 && widgetFilterState.temperatureRangeFilter.tempRangeMaxVal >= 0) {
        headerFilterState.temperatureRangeFilter.isInactive = true;
        mergedFilterState.temperatureRangeFilter = angular.copy(widgetFilterState.temperatureRangeFilter);
    } else {
        headerFilterState.temperatureRangeFilter.isInactive = false;
        mergedFilterState.temperatureRangeFilter = angular.copy(headerFilterState.temperatureRangeFilter);
        if (headerFilterState.temperatureRangeFilter.tempRangeMinVal >= 0 && headerFilterState.temperatureRangeFilter.tempRangeMaxVal >= 0) {
            activeGlobalFilterCount += 1;
        }
    }
    
    if (widgetFilterState.passcountFilter.lowerPasscount && widgetFilterState.passcountFilter.upperPasscount) {
        headerFilterState.passcountFilter.isInactive = true;
        mergedFilterState.passcountFilter = angular.copy(widgetFilterState.passcountFilter);
    } else {
        headerFilterState.passcountFilter.isInactive = false;
        mergedFilterState.passcountFilter = angular.copy(headerFilterState.passcountFilter);
        if (headerFilterState.passcountFilter.lowerPasscount && headerFilterState.passcountFilter.upperPasscount) {
            activeGlobalFilterCount += 1;
        }
    }
    
    if (widgetFilterState.automaticsFilter.selectedCriteria().length > 0) {
        headerFilterState.automaticsFilter.isInactive = true;
        mergedFilterState.automaticsFilter = angular.copy(widgetFilterState.automaticsFilter);
    } else {
        headerFilterState.automaticsFilter.isInactive = false;
        mergedFilterState.automaticsFilter = angular.copy(headerFilterState.automaticsFilter);
        activeGlobalFilterCount += headerFilterState.automaticsFilter.selectedCriteria().length;
    }
    
    return {"mergedFilter": mergedFilterState, "activeGlobalFilterCount": activeGlobalFilterCount};
  }
 */
