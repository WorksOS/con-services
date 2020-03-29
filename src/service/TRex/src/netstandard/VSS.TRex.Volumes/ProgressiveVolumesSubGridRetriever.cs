using System;
using VSS.TRex.Common.Models;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGrids;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Volumes
{
  /*
   * What filter aspects need to be adhered to? Pass count filtering?
   * Does minimum elevation mode need to be respected [Yes]
   *
   * Should always have a time range?
   *
   */
  public class ProgressiveVolumesSubGridRetriever : SubGridRetrieverBase, ISubGridRetriever
  {
    /// <summary>
    /// Constructor for the sub grid retriever helper
    /// </summary>
    /// <param name="siteModel">The project this sub gris is being retrieved from</param>
    /// <param name="gridDataType">The type of client grid data sub grids to be returned by this retriever</param>
    /// <param name="storageProxy">The Ignite storage proxy to be used when requesting data from the persistent store</param>
    /// <param name="filter">The TRex spatial and attribute filtering description for the request</param>
    /// <param name="filterAnnex">An annex of data related to cell by cell filtering where the attributes related to that cell may change from cell to cell</param>
    /// <param name="hasOverrideSpatialCellRestriction">The spatially selected cells are masked by a rectangular restriction boundary</param>
    /// <param name="overrideSpatialCellRestriction"></param>
    /// <param name="prepareGridForCacheStorageIfNoSieving">The cell coordinate bounding box restricting cells involved in the request</param>
    /// <param name="maxNumberOfPassesToReturn">The maximum number of passes in a cell in a sub grid that will be considered when processing the request</param>
    /// <param name="areaControlSet">The skip/step area control set for selection of cells with sub grids for processing. Cells not identified by the control set will return null values.</param>
    /// <param name="populationControl">The delegate responsible for populating events depended on for processing the request.</param>
    /// <param name="pDExistenceMap">The production data existence map for the project the request relates to</param>
    /// <param name="overrides">The set of overriding machine event values to use</param>
    /// <param name="liftParams">The set of layer/lift analysis parameters to use</param>
    public ProgressiveVolumesSubGridRetriever(ISiteModel siteModel,
      GridDataType gridDataType,
      IStorageProxy storageProxy,
      ICombinedFilter filter,
      ICellPassAttributeFilterProcessingAnnex filterAnnex,
      bool hasOverrideSpatialCellRestriction,
      BoundingIntegerExtent2D overrideSpatialCellRestriction,
      bool prepareGridForCacheStorageIfNoSieving,
      int maxNumberOfPassesToReturn,
      AreaControlSet areaControlSet,
      IFilteredValuePopulationControl populationControl,
      ISubGridTreeBitMask pDExistenceMap,
      IOverrideParameters overrides,
      ILiftParameters liftParams)
      : base(siteModel, gridDataType, filter, filterAnnex,
        hasOverrideSpatialCellRestriction, overrideSpatialCellRestriction, prepareGridForCacheStorageIfNoSieving, maxNumberOfPassesToReturn,
        storageProxy, areaControlSet, populationControl, pDExistenceMap, overrides, liftParams)
    {
    }

    /// <summary>
    /// Custom implementation for retrieving stripes of progressive volume cells
    /// </summary>
    /// <param name="stripeIndex"></param>
    public override void RetrieveSubGridStripe(byte stripeIndex)
    {
      // Iterate over the cells in the sub grid applying the filter and assigning the requested information into the sub grid
      for (byte j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
      {
        // If this cell is not included in the scan mask then prune execution here for the cell
        // For pass attributes that are maintained on a historical last pass basis (meaning their values bubble up through cell passes where the values of
        // those attributes are null), check the global latest pass version of those values. If they are null, then no further work needs to be done
        if (!_aggregatedCellScanMap.BitSet(stripeIndex, j) || LatestCellPassAttributeIsNull(stripeIndex, j))
          continue;

        /*
         * For each cell:
         * 1. Use filter to establish time range sampled cell passes can come from
         * 2. Use 'as at' logic to determine current ground for earliest sampled elevation (ie: this may be before the
         * starting time of the sampled range).
         * 3. Construct a vector of time samples for the cell where each sample is the measured elevation at the sample time as
         * defined by the cell pass at or immediately prior to it, or in the case of minimum elevation mode measurements the lowest measured cell
         * pass according to the standard minimum elevation mode selection logic used in the mainline sub grid retriever
         */

        // Probably not required as progressive volumes should never as for CellProfile results
        //if (_gridDataType == GridDataType.CellProfile) // all requests using this data type should filter temperature range using last pass only
        //  _filter.AttributeFilter.FilterTemperatureByLastPass = true;

//        _haveFilteredPass = false;

        throw new NotImplementedException();

//        if (_haveFilteredPass)
//        {
//          AssignedFilteredValueContextToClient(stripeIndex, j, topMostLayerCompactionHalfPassCount);
//        }
      }
    }
  }
}
