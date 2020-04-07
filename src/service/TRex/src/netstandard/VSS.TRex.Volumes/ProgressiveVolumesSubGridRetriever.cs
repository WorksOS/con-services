using System;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGrids;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Iterators;
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
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan Interval { get; set; }

    private IClientProgressiveHeightsLeafSubGrid _progressiveClientSubGrid;

    private ISubGridSegmentIterator _reversingSegmentIterator;
    private SubGridSegmentCellPassIterator_NonStatic _reversingCellPassIterator;

    private CellPass _firstCellPass; 
    private CellPass _priorToFirstCellPass; 
    private CellPass _currentCellPass; 

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
      // Clear any time element from the supplied filter. Time constraints ar derived from the startDate and endDate parameters
      filter.AttributeFilter.HasTimeFilter = false;

      // Clear any instruction in the filter to extract the earliest value - this has no meaning in progressive calculations
      filter.AttributeFilter.ReturnEarliestFilteredCellPass = false;

      // Remove any first/last/highest/lowest aspect from the filter - this has no meaning in progressive calculations
      filter.AttributeFilter.HasElevationTypeFilter = false;

      // Remove any machine filtering - the intent here is to examine volume progression over time, machine breakdowns don't make sense at this point
      filter.AttributeFilter.HasMachineFilter = false;
    }

    private bool _commonCellPassStackExaminationDone;

    protected override void SetupForCellPassStackExamination()
    {
      base.SetupForCellPassStackExamination();

      if (!_commonCellPassStackExaminationDone)
      {
        // Modify the cell pass iterator to obey the time range established by the StartDate/EndDate arguments
        _cellPassIterator.SetTimeRange(true, StartDate, EndDate);

        // Construct a reversing segment and cell pass iterator used to locate necessary cell passes earlier than the first cell pass
        // according to the primary cell pass iterator _cellPassIterator. Allow it's time range to be all of history prior to StartDate
        // Note that both the reversing and progressive sub grid operator are provide the same sub grid. This permits both iterators
        // to leverage the inherent cache of segment information within the sub grid.
        _reversingSegmentIterator = new SubGridSegmentIterator(_subGridAsLeaf, _subGridAsLeaf.Directory, _storageProxy)
        {
          IterationDirection = IterationDirection.Backwards
        };

        _reversingCellPassIterator = new SubGridSegmentCellPassIterator_NonStatic(_reversingSegmentIterator, _maxNumberOfPassesToReturn);
        _reversingCellPassIterator.SetTimeRange(true, DateTime.MinValue, StartDate.AddTicks(-100));

        _commonCellPassStackExaminationDone = true;
      }

      _segmentIterator.IterationDirection = IterationDirection.Forwards;

      _reversingSegmentIterator.SubGrid = _subGridAsLeaf;
      _reversingSegmentIterator.Directory = _subGridAsLeaf.Directory;
    }

    /// <summary>
    /// Decorates the base sub grid request logic with additional requirements for progressive requests
    /// </summary>
    /// <param name="clientGrid"></param>
    /// <param name="cellOverrideMask"></param>
    /// <returns></returns>
    public override ServerRequestResult RetrieveSubGrid(IClientLeafSubGrid clientGrid,
      SubGridTreeBitmapSubGridBits cellOverrideMask)
    {
      // Establish the expected number of height layers in the client sub grid
      if (!(clientGrid is IClientProgressiveHeightsLeafSubGrid subGrid))
      {
        throw new ArgumentException($"Supplied client {clientGrid.Moniker()} is not a {nameof(IClientProgressiveHeightsLeafSubGrid)}");
      }

      _progressiveClientSubGrid = subGrid;

      var numHeightLayers = (int)((EndDate.Ticks - StartDate.Ticks) / Interval.Ticks);
      if ((EndDate.Ticks - StartDate.Ticks) % Interval.Ticks == 0)
      {
        numHeightLayers++;
      }

      _progressiveClientSubGrid.NumberOfHeightLayers = numHeightLayers; 

      return base.RetrieveSubGrid(clientGrid, cellOverrideMask);
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
        if (!_aggregatedCellScanMap.BitSet(stripeIndex, j) /* || LatestCellPassAttributeIsNull(stripeIndex, j)*/)
        {
          continue;
        }

        /*
         * For each cell:
         * 1. Establish time range sampled cell passes can come from
         * 2. Use 'as at' logic to determine current ground for earliest sampled elevation (ie: this may be before the
         * starting time of the sampled range).
         * 3. Construct a vector of time samples for the cell where each sample is the measured elevation at the sample time as
         * defined by the cell pass at or immediately prior to it, or in the case of minimum elevation mode measurements the lowest measured cell
         * pass according to the standard minimum elevation mode selection logic used in the mainline sub grid retriever
         *
         * CAVEATS:
         * 1. [Todo] This logic currently does not cater for minimum elevation mode. This should be relatively easy to track on a forwards basis as
         * the cell passes are being traversed
         */

        // Initialise iterator contexts for this cell, both forwards and reversing iterators.
        _cellPassIterator.SetCellCoordinatesInSubGrid(stripeIndex, j);
        _cellPassIterator.Initialise();
        _reversingCellPassIterator.SetCellCoordinatesInSubGrid(stripeIndex, j);
        _reversingCellPassIterator.Initialise();

        if (_filter.AttributeFilter.HasElevationRangeFilter)
        {
          _cellPassIterator.SetIteratorElevationRange(_filterAnnex.ElevationRangeBottomElevationForCell, _filterAnnex.ElevationRangeTopElevationForCell);
          _reversingCellPassIterator.SetIteratorElevationRange(_filterAnnex.ElevationRangeBottomElevationForCell, _filterAnnex.ElevationRangeTopElevationForCell);
        }

        // Ask for the first cell pass prior to the date range using the reversing iterator
        var havePriorToFirstCellPass = _reversingCellPassIterator.GetNextCellPass(ref _priorToFirstCellPass);

        var previousMarchingHeight = Consts.NullHeight;
        if (havePriorToFirstCellPass)
        {
          // There is a prior ground elevation to use for the first height progressing
          _progressiveClientSubGrid.AssignFilteredValue(0, stripeIndex, j, _priorToFirstCellPass.Height);
          previousMarchingHeight = _priorToFirstCellPass.Height;
        }

        if (_progressiveClientSubGrid.NumberOfHeightLayers < 2)
        {
          continue; // Nothing more to do in this cell
        }

        var marchingDate = StartDate;
        var marchingIndex = 0;

        // Advance through cell passes pausing as each date increment is passed to set the appropriate height value into the sub grid
        while (_cellPassIterator.MayHaveMoreFilterableCellPasses() && _cellPassIterator.GetNextCellPass(ref _currentCellPass))
        {
          if (_currentCellPass.Time < marchingDate)
          {
            previousMarchingHeight = _currentCellPass.Height;
            continue;
          }

          // Record this cell pass height at the marching date if it is at the exact time of <marchingDate>, otherwise use
          // previousMarchingHeight as this will be the correct height for this marching date
          _progressiveClientSubGrid.AssignFilteredValue(marchingIndex, stripeIndex, j,
            _currentCellPass.Time == marchingDate ? _currentCellPass.Height : previousMarchingHeight);

          previousMarchingHeight = _currentCellPass.Height;
          marchingIndex++;
          marchingDate += Interval;

          if (marchingIndex >= _progressiveClientSubGrid.NumberOfHeightLayers)
          {
            break; // There is no more work to do
          }
        }

        // Fill in any progression dates beyond the last cell pass extracted
        for (var i = marchingIndex; i < _progressiveClientSubGrid.NumberOfHeightLayers; i++)
        {
          _progressiveClientSubGrid.AssignFilteredValue(i, stripeIndex, j, previousMarchingHeight);
        }
      }
    }
  }
}
