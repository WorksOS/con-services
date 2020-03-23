using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common.Models;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGrids
{
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
      ISubGridTreeBitMask PDExistenceMap,
      ITRexSpatialMemoryCacheContext subGridCacheContext,
      IOverrideParameters overrides,
      ILiftParameters liftParams)
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
        PDExistenceMap,
        overrides,
        liftParams);
    }
  }
}
