using System.Threading.Tasks;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common.Models;
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
    Task<(ServerRequestResult requestResult, IClientLeafSubGrid clientGrid)> RequestSubGridInternal(
      SubGridCellAddress subGridAddress,
      bool prodDataRequested,
      bool surveyedSurfaceDataRequested);

    /// <summary>
    /// Constructor that accepts the common parameters around a set of sub grids the requester will be asked to process
    /// and initializes the requester state ready to start processing individual sub grid requests.
    /// </summary>
    void Initialize(ISiteModel siteModel,
      GridDataType gridDataType,
      IStorageProxy storageProxy,
      ICombinedFilter filter,
      bool hasOverrideSpatialCellRestriction,
      BoundingIntegerExtent2D overrideSpatialCellRestriction,
      int maxNumberOfPassesToReturn,
      AreaControlSet areaControlSet,
      IFilteredValuePopulationControl populationControl,
      ISubGridTreeBitMask PDExistenceMap,
      ITRexSpatialMemoryCache subGridCache,
      ITRexSpatialMemoryCacheContext subGridCacheContext,
      ISurveyedSurfaces filteredSurveyedSurfaces,
      ISurfaceElevationPatchRequest surfaceElevationPatchRequest,
      ISurfaceElevationPatchArgument surfaceElevationPatchArgument,
      IOverrideParameters overrides,
      ILiftParameters liftParams
    );
  }
}
