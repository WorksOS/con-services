﻿using System;
using System.Linq;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Caching;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.GridFabric.Arguments;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;
// ReSharper disable IdentifierTypo

namespace VSS.TRex.SubGrids
{
  /// <summary>
  /// Provides support for creation of requestors that encapsulate much of the state related to querying sets of sub grids
  /// </summary>
  public class RequestorUtilities : IRequestorUtilities
  {
    private static readonly bool _enableGeneralSubGridResultCaching = DIContext.Obtain<IConfigurationStore>().GetValueBool("ENABLE_GENERAL_SUBGRID_RESULT_CACHING", Consts.ENABLE_GENERAL_SUBGRID_RESULT_CACHING);

    private ITRexSpatialMemoryCache _subGridCache;

    /// <summary>
    /// The DI injected TRex spatial memory cache for general sub grid results
    /// </summary>
    private ITRexSpatialMemoryCache SubGridCache => _subGridCache ??= DIContext.Obtain<ITRexSpatialMemoryCache>();
     
    /// <summary>
    /// The DI injected factory to create request instances
    /// </summary>
    private readonly Func<ISubGridRequestor> _subGridRequestorFactory = DIContext.Obtain<Func<ISubGridRequestor>>();

    /// <summary>
    /// The DI injected factory to created requests for surveyed surface information
    /// </summary>
    private readonly Func<ITRexSpatialMemoryCache, ITRexSpatialMemoryCacheContext, ISurfaceElevationPatchRequest> _surfaceElevationPatchRequestFactory = 
      DIContext.Obtain<Func<ITRexSpatialMemoryCache, ITRexSpatialMemoryCacheContext, ISurfaceElevationPatchRequest>>();

    /// <summary>
    /// Constructs a set of requester intermediaries that have various aspects of surveyed surfaces, filters and caches pre-calculated
    /// ready to be used to create per-Task request delegates
    /// </summary>
    /// <returns></returns>
    public (GridDataType GridDataType,
      ICombinedFilter Filter,
      ISurveyedSurfaces FilteredSurveyedSurfaces,
      ISurfaceElevationPatchRequest surfaceElevationPatchRequest,
      ISurfaceElevationPatchArgument surfaceElevationPatchArgument,
      ITRexSpatialMemoryCacheContext CacheContext)[] 
      ConstructRequestorIntermediaries(
        ISiteModel siteModel,
        IFilterSet filters,
        bool includeSurveyedSurfaceInformation,
        GridDataType gridDataType)
    {
      (GridDataType GridDataType,
      ICombinedFilter Filter,
      ISurveyedSurfaces FilteredSurveyedSurfaces,
      ISurfaceElevationPatchRequest surfaceElevationPatchRequest,
      ISurfaceElevationPatchArgument surfaceElevationPatchArgument,
      ITRexSpatialMemoryCacheContext CacheContext) GetIntermediary(ICombinedFilter filter)
      {
        // Construct the appropriate list of surveyed surfaces
        // Obtain local reference to surveyed surface list. If it is replaced while processing the
        // list then the local reference will still be valid allowing lock free read access to the list.
        ISurveyedSurfaces filteredSurveyedSurfaces = null;
        var surveyedSurfaceList = siteModel.SurveyedSurfaces;

        if (includeSurveyedSurfaceInformation && surveyedSurfaceList?.Count > 0)
        {
          filteredSurveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();

          // Filter out any surveyed surfaces which don't match current filter (if any) - realistically, this is time filters we're thinking of here
          surveyedSurfaceList.FilterSurveyedSurfaceDetails(filter.AttributeFilter.HasTimeFilter,
            filter.AttributeFilter.StartTime, filter.AttributeFilter.EndTime,
            filter.AttributeFilter.ExcludeSurveyedSurfaces(), filteredSurveyedSurfaces,
            filter.AttributeFilter.SurveyedSurfaceExclusionList);

          // Ensure that the filtered surveyed surfaces are in a known ordered state
          filteredSurveyedSurfaces.SortChronologically(filter.AttributeFilter.ReturnEarliestFilteredCellPass);
        }

        var filteredSurveyedSurfacesAsArray = filteredSurveyedSurfaces?.Count > 0 ? filteredSurveyedSurfaces.Select(s => s.ID).ToArray() : new Guid[0];

        // Get a caching context for the sub grids returned by this requester, but only if the requested grid data type supports it
        ITRexSpatialMemoryCacheContext subGridCacheContext = null;

        if (_enableGeneralSubGridResultCaching && 
            ClientLeafSubGrid.SupportsAssignationFromCachedPreProcessedClientSubGrid[(int)gridDataType])
        {
          subGridCacheContext = SubGridCache?.LocateOrCreateContext(siteModel.ID, SpatialCacheFingerprint.ConstructFingerprint(siteModel.ID, gridDataType, filter, filteredSurveyedSurfacesAsArray));
        }

        // Instantiate a single instance of the argument object for the surface elevation patch requests and populate it with 
        // the common elements for this set of sub grids being requested. We always want to request all surface elevations to 
        // promote cacheability.
        var surfaceElevationPatchArg = new SurfaceElevationPatchArgument
        (
          siteModelID: siteModel.ID,
          oTGCellBottomLeftX: int.MinValue,
          oTGCellBottomLeftY: int.MinValue,
          cellSize: siteModel.CellSize,
          includedSurveyedSurfaces: filteredSurveyedSurfaces, 
          surveyedSurfacePatchType: filter.AttributeFilter.ReturnEarliestFilteredCellPass ? SurveyedSurfacePatchType.EarliestSingleElevation : SurveyedSurfacePatchType.LatestSingleElevation,
          processingMap: new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled)
        );

        return (gridDataType,
          filter,
          filteredSurveyedSurfaces,
          _surfaceElevationPatchRequestFactory(SubGridCache, SubGridCache?.LocateOrCreateContext(siteModel.ID, surfaceElevationPatchArg.CacheFingerprint())),
          surfaceElevationPatchArg,
          subGridCacheContext);
      }

      // Construct the intermediary request state
      return filters.Filters.Select(GetIntermediary).ToArray();
    }

    /// <summary>
    /// Constructs the set of requestors, one per filter, required to query the data stacks
    /// </summary>
    public ISubGridRequestor[] ConstructRequestors(
      ISubGridsRequestArgument subGridsRequestArgument,
      ISiteModel siteModel,
      IOverrideParameters overrides,
      ILiftParameters liftParams,
      (GridDataType GridDataType,
        ICombinedFilter Filter,
        ISurveyedSurfaces FilteredSurveyedSurfaces,
        ISurfaceElevationPatchRequest surfaceElevationPatchRequest,
        ISurfaceElevationPatchArgument surfaceElevationPatchArgument,
        ITRexSpatialMemoryCacheContext CacheContext)[] intermediaries,
      AreaControlSet areaControlSet,
      ISubGridTreeBitMask prodDataMask
      )
    {
      // Construct the resulting requestors
      return intermediaries.Select(x =>
      {
        var requestor = _subGridRequestorFactory();
        requestor.Initialize(
          subGridsRequestArgument,
          siteModel,
          x.GridDataType,
          siteModel.PrimaryStorageProxy,
          x.Filter,
          false, // Override cell restriction
          BoundingIntegerExtent2D.Inverted(),
          int.MaxValue, // MaxCellPasses
          areaControlSet,
          new FilteredValuePopulationControl(),
          prodDataMask,
          SubGridCache,
          x.CacheContext,
          x.FilteredSurveyedSurfaces,
          x.surfaceElevationPatchRequest,
          x.surfaceElevationPatchArgument,
          overrides,
          liftParams);

        return requestor;
      }).ToArray();
    }
  }
}
