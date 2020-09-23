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
    (GridDataType GridDataType,
      ICombinedFilter Filter,
      ISurveyedSurfaces FilteredSurveyedSurfaces,
      ISurfaceElevationPatchRequest surfaceElevationPatchRequest,
      ITRexSpatialMemoryCacheContext[] CacheContexts)[] ConstructRequestorIntermediaries(ISiteModel siteModel,
        IFilterSet filters,
        bool includeSurveyedSurfaceInformation,
        GridDataType gridDataType);

    /// <summary>
    /// Constructs the set of requestors, one per filter, required to query the data stacks
    /// </summary>
    ISubGridRequestor[] ConstructRequestors(
      ISubGridsRequestArgument subGridsRequestArgument, 
      ISiteModel siteModel,
      IOverrideParameters overrides,
      ILiftParameters liftParams,
      (GridDataType GridDataType,
        ICombinedFilter Filter,
        ISurveyedSurfaces FilteredSurveyedSurfaces,
        ISurfaceElevationPatchRequest surfaceElevationPatchRequest,
        ITRexSpatialMemoryCacheContext[] CacheContexts)[] intermediaries,
      AreaControlSet areaControlSet,
      ISubGridTreeBitMask prodDataMask);
  }
}
