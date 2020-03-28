using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common.Models;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGrids;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Volumes;

namespace VSS.TRex.Tests.TestFixtures
{
  /// <summary>
  /// Factory class that constructs specialist sub grid retrievers according to the grid data type being requested
  /// </summary>
  public class SubGridRetrieverFactory : ISubGridRetrieverFactory
  {
    public ISubGridRetriever Instance(ISiteModel siteModel,
      GridDataType gridDataType,
      IStorageProxy storageProxy,
      ICombinedFilter filter,
      ICellPassAttributeFilterProcessingAnnex filterAnnex,
      bool hasOverrideSpatialCellRestriction,
      BoundingIntegerExtent2D overrideSpatialCellRestriction,
      int maxNumberOfPassesToReturn,
      AreaControlSet areaControlSet,
      IFilteredValuePopulationControl populationControl,
      ISubGridTreeBitMask pdExistenceMap,
      ITRexSpatialMemoryCacheContext subGridCacheContext,
      IOverrideParameters overrides,
      ILiftParameters liftParams)
    {
      if (gridDataType == GridDataType.ProgressiveVolumes)
      {
        return new ProgressiveVolumesSubGridRetriever(siteModel,
          gridDataType,
          storageProxy,
          filter,
          filterAnnex,
          hasOverrideSpatialCellRestriction,
          overrideSpatialCellRestriction,
          subGridCacheContext != null,
          maxNumberOfPassesToReturn,
          areaControlSet,
          populationControl,
          pdExistenceMap,
          overrides,
          liftParams);
      }
      else
      {
        return new SubGridRetriever(siteModel,
          gridDataType,
          storageProxy,
          filter,
          filterAnnex,
          hasOverrideSpatialCellRestriction,
          overrideSpatialCellRestriction,
          subGridCacheContext != null,
          maxNumberOfPassesToReturn,
          areaControlSet,
          populationControl,
          pdExistenceMap,
          overrides,
          liftParams);
      }
    }
  }
}
