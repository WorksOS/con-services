using System;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common.Models;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGrids.Interfaces
{
  public interface IRequestorUtilities
  {
    /// <summary>
    /// Constructs a set of requester intermediaries that have various aspects of surveyed surfaces, filters and caches pre-calculated
    /// ready to be used to create per-Task requestor delegates
    /// </summary>
    /// <returns></returns>
    (GridDataType GridDataType,
      ICombinedFilter Filter,
      ISurveyedSurfaces FilteredSurveyedSurfaces,
      ISurfaceElevationPatchRequest surfaceElevationPatchRequest,
      ISurfaceElevationPatchArgument surfaceElevationPatchArgument,
      ITRexSpatialMemoryCacheContext CacheContext)[] ConstructRequestorIntermediaries(ISiteModel siteModel,
        IFilterSet filters,
        bool includeSurveyedSurfaceInformation,
        GridDataType gridDataType);

    /// <summary>
    /// Constructs the set of requestors, one per filter, required to query the data stacks
    /// </summary>
    /// <returns></returns>
    ISubGridRequestor[] ConstructRequestors(ISiteModel siteModel,
      IOverrideParameters overrides,
      ILiftParameters liftParams,
      (GridDataType GridDataType,
        ICombinedFilter Filter,
        ISurveyedSurfaces FilteredSurveyedSurfaces,
        ISurfaceElevationPatchRequest surfaceElevationPatchRequest,
        ISurfaceElevationPatchArgument surfaceElevationPatchArgument,
        ITRexSpatialMemoryCacheContext CacheContext)[] intermediaries,
      AreaControlSet areaControlSet,
      ISubGridTreeBitMask prodDataMask,
      Action<ISubGridRequestor, ISubGridRetriever> customRequestorInitializer = null);
  }
}
