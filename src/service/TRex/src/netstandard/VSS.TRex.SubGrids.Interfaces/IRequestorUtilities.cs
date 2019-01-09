using System;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common.Types;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.GridFabric.Requests;
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
    (ICombinedFilter Filter,
      ISurveyedSurfaces FilteredSurveyedSurfaces,
      Guid[] FilteredSurveyedSurfacesAsArray,
      SurfaceElevationPatchRequest Request,
      ITRexSpatialMemoryCacheContext CacheContext)[] ConstructRequestorIntermediaries(ISiteModel siteModel,
        IFilterSet filters,
        bool includeSurveyedSurfaceInformation,
        GridDataType gridDataType);

    /// <summary>
    /// Constructs the set of requestors, one per filter, required to query the data stacks
    /// </summary>
    /// <returns></returns>
    ISubGridRequestor[] ConstructRequestors(ISiteModel siteModel,
      (ICombinedFilter Filter,
        ISurveyedSurfaces FilteredSurveyedSurfaces,
        Guid[] FilteredSurveyedSurfacesAsArray,
        SurfaceElevationPatchRequest Request,
        ITRexSpatialMemoryCacheContext CacheContext)[] Intermediaries,
      AreaControlSet areaControlSet,
      ISubGridTreeBitMask prodDataMask);
  }
}
