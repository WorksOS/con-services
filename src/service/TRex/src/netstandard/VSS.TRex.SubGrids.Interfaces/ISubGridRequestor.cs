using System;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common.Types;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGrids.Interfaces
{
  public interface ISubGridRequestor
  {
    SubGridTreeBitmapSubGridBits CellOverrideMask { get; set; }

    /// <summary>
    /// Responsible for coordinating the retrieval of production data for a sub grid from a site model and also annotating it with
    /// surveyed surface information for requests involving height data.
    /// </summary>
    /// <param name="subGridAddress"></param>
    /// <param name="prodDataRequested"></param>
    /// <param name="surveyedSurfaceDataRequested"></param>
    /// <param name="clientGrid"></param>
    /// <returns></returns>
    ServerRequestResult RequestSubGridInternal(SubGridCellAddress subGridAddress,
      // LiftBuildSettings: TICLiftBuildSettings;
      bool prodDataRequested,
      bool surveyedSurfaceDataRequested,
      IClientLeafSubGrid clientGrid
    );

    /// <summary>
    /// Constructor that accepts the common parameters around a set of sub grids the requester will be asked to process
    /// and initializes the requester state ready to start processing individual sub grid requests.
    /// </summary>
    void Initialize(ISiteModel sitemodel,
      IStorageProxy storageProxy,
      ICombinedFilter filter,
      bool hasOverrideSpatialCellRestriction,
      BoundingIntegerExtent2D overrideSpatialCellRestriction,
      byte treeLevel,
      int maxNumberOfPassesToReturn,
      AreaControlSet areaControlSet,
      IFilteredValuePopulationControl populationControl,
      ISubGridTreeBitMask PDExistenceMap,
      ITRexSpatialMemoryCache subGridCache,
      ITRexSpatialMemoryCacheContext subGridCacheContext,
      ISurveyedSurfaces filteredSurveyedSurfaces,
      Guid[] filteredSurveyedSurfacesAsArray,
      ISurfaceElevationPatchRequest surfaceElevationPatchRequest,
      ISurfaceElevationPatchArgument surfaceElevationPatchArgument);
  }
}
