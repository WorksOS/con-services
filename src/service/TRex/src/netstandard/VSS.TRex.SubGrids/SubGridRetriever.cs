using System;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Types;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Filters.Models;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.Types.Types;
using Range = VSS.TRex.Common.Utilities.Range;

namespace VSS.TRex.SubGrids
{
  /// <summary>
  /// Contains and orchestrates the business logic for processing sub grids...
  /// </summary>
  public class SubGridRetriever : SubGridRetrieverBase, ISubGridRetriever
  {
    // private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridRetriever>();

    // Local state populated for the purpose of access from various local methods
    private bool _haveFilteredPass;
    private FilteredPassData _currentPass;
    private FilteredPassData _tempPass;
    private bool _firstPassMinElev;

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
    public SubGridRetriever(ISiteModel siteModel,
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
    /// Performs primary filtered iteration over cell passes in the cell being processed to determine the cell pass
    /// to be selected.
    /// </summary>
    private void ProcessCellPasses()
    {
      _haveFilteredPass = false;
      _firstPassMinElev = false;

      while (_cellPassIterator.MayHaveMoreFilterableCellPasses() &&
             _cellPassIterator.GetNextCellPass(ref _currentPass.FilteredPass))
      {

        FiltersValuePopulation.PopulateFilteredValues(_siteModel.MachinesTargetValues[_currentPass.FilteredPass.InternalSiteModelMachineIndex], _populationControl, ref _currentPass);

        if (_filter.AttributeFilter.FilterPass(ref _currentPass, _filterAnnex))
        {

          if (_filter.AttributeFilter.HasElevationTypeFilter)
            _assignmentContext.FilteredValue.PassCount = 1;

          // Track cell passes against lowest/highest elevation criteria
          if (_filter.AttributeFilter.HasElevationTypeFilter && _filter.AttributeFilter.ElevationType == ElevationType.Lowest)
          {
            if (!_haveFilteredPass || _currentPass.FilteredPass.Height < _tempPass.FilteredPass.Height)
              _tempPass = _currentPass;
            _haveFilteredPass = true;
          }
          else if (_filter.AttributeFilter.HasElevationTypeFilter && _filter.AttributeFilter.ElevationType == ElevationType.Highest)
          {
            if (!_haveFilteredPass || _currentPass.FilteredPass.Height > _tempPass.FilteredPass.Height)
              _tempPass = _currentPass;
            _haveFilteredPass = true;
          }
          else if (_filter.AttributeFilter.HasElevationTypeFilter && _filter.AttributeFilter.ElevationType == ElevationType.First)
          {
            _assignmentContext.FilteredValue.FilteredPassData = _currentPass;
            _haveFilteredPass = true;
            _assignmentContext.FilteredValue.PassCount = CellPassConsts.NullPassCountValue;
            break;
          }
          else if (_gridDataType == GridDataType.Temperature || _gridDataType == GridDataType.TemperatureDetail)
          {
            var materialTemperature = _currentPass.FilteredPass.MaterialTemperature;
            if (materialTemperature != CellPassConsts.NullMaterialTemperatureValue)
            {
              if (_filter.AttributeFilter.HasTemperatureRangeFilter)
              {
                if (Range.InRange(materialTemperature, _filter.AttributeFilter.MaterialTemperatureMin, _filter.AttributeFilter.MaterialTemperatureMax))
                {
                  _assignmentContext.FilteredValue.FilteredPassData = _currentPass;
                  _haveFilteredPass = true;
                  _assignmentContext.FilteredValue.PassCount = CellPassConsts.NullPassCountValue;
                  break;
                }
              }
              else
              {
                _assignmentContext.FilteredValue.FilteredPassData = _currentPass;
                _haveFilteredPass = true;
                _assignmentContext.FilteredValue.PassCount = CellPassConsts.NullPassCountValue;
                break;
              }
            }
          }
          else
          {
            // check for min elevation mode
            var internalMachineIndex = _currentPass.FilteredPass.InternalSiteModelMachineIndex;
            var machine = _siteModel.Machines[internalMachineIndex];
            var machineIsAnExcavator = machine.MachineType == MachineType.Excavator;
            var mappingMode = _siteModel.MachinesTargetValues[internalMachineIndex].ElevationMappingModeStateEvents.GetValueAtDate(_currentPass.FilteredPass.Time, out _, ElevationMappingMode.LatestElevation);
            var minimumElevationMappingModeAtCellPassTime = mappingMode == ElevationMappingMode.MinimumElevation;
            if (machineIsAnExcavator && minimumElevationMappingModeAtCellPassTime)
            {
              if (!_firstPassMinElev)
              {
                _firstPassMinElev = true;
                _haveFilteredPass = true;
                _tempPass = _currentPass;
              }
              else if (_currentPass.FilteredPass.Height < _tempPass.FilteredPass.Height)
                _tempPass = _currentPass; // take if lowest pass
            }
            else
            {
              // All criteria have been met for acceptance of this pass
              _assignmentContext.FilteredValue.FilteredPassData = _currentPass;
              _haveFilteredPass = true;
              _assignmentContext.FilteredValue.PassCount = CellPassConsts.NullPassCountValue;
              break;
            }
          }
        }
      }
    }

    /// <summary>
    /// Special version of ProcessCellPasses when using a pass count range Filter.
    /// Performs primary filtered iteration over cell passes in the cell being processed to determine the cell pass
    /// to be selected.
    /// </summary>
    private void ProcessCellPassesPassCountRange()
    {
      _haveFilteredPass = false;
      var haveHalfPass = false;
      var passRangeCount = 0;
      int idxPtr;
      var arrayLength = 1000;
      var filteredPassDataArray = new FilteredPassData[arrayLength];
      var filteredPassBoolArray = new bool[arrayLength];
      var passes = 0;
      var validPasses = 0;
      var minElevCheckRequired = false;
      _firstPassMinElev = false;

      while (_cellPassIterator.MayHaveMoreFilterableCellPasses() &&
             _cellPassIterator.GetNextCellPass(ref _currentPass.FilteredPass))
      {
        FiltersValuePopulation.PopulateFilteredValues(_siteModel.MachinesTargetValues[_currentPass.FilteredPass.InternalSiteModelMachineIndex], _populationControl, ref _currentPass);

        if (_filter.AttributeFilter.FilterPass(ref _currentPass, _filterAnnex))
        {
          if (_gridDataType == GridDataType.Temperature || _gridDataType == GridDataType.TemperatureDetail)
          {
            // make sure we have a valid temperature pass
            var materialTemperature = _currentPass.FilteredPass.MaterialTemperature;
            if (materialTemperature != CellPassConsts.NullMaterialTemperatureValue)
            {
              if (_filter.AttributeFilter.HasTemperatureRangeFilter)
                if (Range.InRange(materialTemperature, _filter.AttributeFilter.MaterialTemperatureMin, _filter.AttributeFilter.MaterialTemperatureMax))
                {
                  _assignmentContext.FilteredValue.FilteredPassData = _currentPass;
                  _haveFilteredPass = true;
                  _assignmentContext.FilteredValue.PassCount = CellPassConsts.NullPassCountValue;
                }
            }
            else
            {
              _assignmentContext.FilteredValue.FilteredPassData = _currentPass;
              _haveFilteredPass = true;
              _assignmentContext.FilteredValue.PassCount = CellPassConsts.NullPassCountValue;
            }
          }
          else
          {
            _assignmentContext.FilteredValue.FilteredPassData = _currentPass;
            _haveFilteredPass = true;
            _assignmentContext.FilteredValue.PassCount = CellPassConsts.NullPassCountValue;
          }
        }

        // Add valid pass to list for pass count range filter
        if (_haveFilteredPass)
        {
          filteredPassDataArray[passes] = _currentPass;
          passes++;
          if (passes == arrayLength - 1)
          {
            arrayLength = arrayLength + 500;
            Array.Resize(ref filteredPassDataArray, arrayLength);
            Array.Resize(ref filteredPassBoolArray, arrayLength);
          }
          _haveFilteredPass = false; // reset
        }
      } // end while more passes

      _haveFilteredPass = false; // reset for next test

      // For pass count range filtering we must walk from earliest to latest
      // setup idxPtr to walk forward in time
      if (_cellPassIterator.SegmentIterator.IterationDirection == IterationDirection.Forwards)
        idxPtr = 0;
      else
        idxPtr = passes - 1; // last entry is earliest

      if (passes > 0)
      {
        for (var i = 0; i < passes; i++)
        {
          if (i > 0)
            if (_cellPassIterator.SegmentIterator.IterationDirection == IterationDirection.Forwards)
              idxPtr++;
            else
              idxPtr--;

          _currentPass = filteredPassDataArray[idxPtr];

          if (_currentPass.FilteredPass.HalfPass)
          {
            if (haveHalfPass)
              passRangeCount++; // increase count on second half pass encountered
            else
            {
              haveHalfPass = true;
              continue; // wont be using first half pass
            }

            haveHalfPass = false;
          }
          else
             passRangeCount++; // increase count for a full pass

          if (Range.InRange(passRangeCount, _filter.AttributeFilter.PassCountRangeMin, _filter.AttributeFilter.PassCountRangeMax))
          {
            filteredPassBoolArray[idxPtr] = true; // tagged for minElev check
            if (_filter.AttributeFilter.HasElevationTypeFilter)
              _assignmentContext.FilteredValue.PassCount = 1;
            else
            {
              validPasses++;
              _assignmentContext.FilteredValue.PassCount = validPasses;
            }

            if ((_filter.AttributeFilter.HasElevationMappingModeFilter && _filter.AttributeFilter.ElevationMappingMode == ElevationMappingMode.MinimumElevation)
                || (_filter.AttributeFilter.HasElevationTypeFilter && _filter.AttributeFilter.ElevationType == ElevationType.Lowest))
            {
              if (!_haveFilteredPass || _currentPass.FilteredPass.Height < _tempPass.FilteredPass.Height)
                _tempPass = _currentPass;
              _haveFilteredPass = true;
            }
            else if (_filter.AttributeFilter.HasElevationTypeFilter && _filter.AttributeFilter.ElevationType == ElevationType.Highest)
            {
              if (!_haveFilteredPass || _currentPass.FilteredPass.Height > _tempPass.FilteredPass.Height)
                _tempPass = _currentPass;
              _haveFilteredPass = true;
            }
            else
            {
              // are we only interested in first pass
              if (_filter.AttributeFilter.HasElevationTypeFilter && _filter.AttributeFilter.ElevationType == ElevationType.First)
              {
                _assignmentContext.FilteredValue.FilteredPassData = _currentPass;
                _haveFilteredPass = true;
                _assignmentContext.FilteredValue.PassCount = CellPassConsts.NullPassCountValue;
                break; // we are out of here
              }

              if (!minElevCheckRequired)
                minElevCheckRequired = true; // means we need to do an extra check below for minElevation on chosen last pass
              if (!_haveFilteredPass)
                _haveFilteredPass = true;
              _tempPass = _currentPass; // good pass. Last one assigned will be latest
            }
          } // end in range

          if (passRangeCount == _filter.AttributeFilter.PassCountRangeMax)
            break; // we are out of here
        } // end pass loop

        if (minElevCheckRequired)
        {
          // If minElevCheckRequired we now have a known list of good passes to determine the lowest pass
          // Rule states we walk in direction of CellPassIterator and return lowest pass while mode is minElev
          // Walk through list that was constructed by CellPassIterator direction to get lowest pass
          _haveFilteredPass = false;
          for (var i = 0; i < passes; i++)
          {
            if (!filteredPassBoolArray[i])
              continue;
            _currentPass = filteredPassDataArray[i];

            // check for min elevation mode
            var internalMachineIndex = _currentPass.FilteredPass.InternalSiteModelMachineIndex;
            var machine = _siteModel.Machines[internalMachineIndex];
            var machineIsAnExcavator = machine.MachineType == MachineType.Excavator;
            var mappingMode = _siteModel.MachinesTargetValues[internalMachineIndex].ElevationMappingModeStateEvents.GetValueAtDate(_currentPass.FilteredPass.Time, out _, ElevationMappingMode.LatestElevation);
            var minimumElevationMappingModeAtCellPassTime = mappingMode == ElevationMappingMode.MinimumElevation;
            if (machineIsAnExcavator && minimumElevationMappingModeAtCellPassTime)
            {
              if (!_firstPassMinElev)
              {
                _firstPassMinElev = true; // take first pass in list as lowest to begin with
                _tempPass = _currentPass;
              }
              else if (!_haveFilteredPass || (_currentPass.FilteredPass.Height < _tempPass.FilteredPass.Height))
                _tempPass = _currentPass;
              if (!_haveFilteredPass)
                _haveFilteredPass = true;
            }
            else
            {
              if (_firstPassMinElev && _haveFilteredPass ) // take last know lowest pass
                break;
              _assignmentContext.FilteredValue.FilteredPassData = _currentPass;
              _haveFilteredPass = true;
              _assignmentContext.FilteredValue.PassCount = CellPassConsts.NullPassCountValue;
              break; // we have the pass we need
            }
          } // end passes
        } // end min check required
      } // end passes > 0
    }

    private void CheckForMinimumElevationRequirements(int stripeIndex, int j)
    {
      // ###US79098### -->
      if (_useLastPassGrid)
      {
        // Determine if there is an elevation mapping mode that may require searching through cell passes. If so, the last pass grid can
        // only be used if the machine that recorded that last pass is not an excavator with an elevation mode set to MinimumHeight.
        // This only applies if there is not an elevation mapping mode filter selecting cells with LatestPass mapping mode
        if (_gridDataType == GridDataType.CutFill || _gridDataType == GridDataType.Height || _gridDataType == GridDataType.HeightAndTime)
        {
          var internalMachineIndex = _globalLatestCells.ReadInternalMachineIndex(stripeIndex, j);
          if (internalMachineIndex != CellPassConsts.NullInternalSiteModelMachineIndex)
          {
            var machine = _siteModel.Machines[internalMachineIndex];

            var machineIsAnExcavator = machine.MachineType == MachineType.Excavator;
            var mappingMode = _siteModel.MachinesTargetValues[internalMachineIndex].ElevationMappingModeStateEvents
              .LastStateValue();

            var minimumElevationMappingModeAtLatestCellPassTime =
              mappingMode == ElevationMappingMode.MinimumElevation;

            if (machineIsAnExcavator && minimumElevationMappingModeAtLatestCellPassTime)
            {
              // It is not possible to use the latest cell pass to answer the query - force the query engine into the cell pass examination work flow
              _useLastPassGrid = false;
              _canUseGlobalLatestCells = false;
            }
          }
        }
      }
      // <-- ###US79098###
    }

    private void AssignedFilteredValueContextToClient(byte stripeIndex, byte j, int topMostLayerCompactionHalfPassCount)
    {
      if (_gridDataType == GridDataType.PassCount || _gridDataType == GridDataType.CellProfile)
        _assignmentContext.FilteredValue.PassCount = topMostLayerCompactionHalfPassCount / 2;

      // If we are displaying a CCV summary view or are displaying a summary of only
      // the top layer in the cell pass stack, then we need to make additional checks to
      // determine if the CCV value filtered from the cell passes is not overridden by
      // the layer in question being superseded. If that is the case, then the CCV value
      // is not assigned to the result set to be passed back to the client as it effectively
      // does not exist given this situation.

      if (_cellProfile == null)
        _clientGrid.AssignFilteredValue(stripeIndex, j, _assignmentContext);
      else
      {
        if (((_gridDataType == GridDataType.CCV || _gridDataType == GridDataType.CCVPercent) &&
             (_liftParams.CCVSummaryTypes == CCVSummaryTypes.None || !_liftParams.CCVSummarizeTopLayerOnly)) ||
            ((_gridDataType == GridDataType.MDP || _gridDataType == GridDataType.MDPPercent) &&
             (_liftParams.MDPSummaryTypes == MDPSummaryTypes.None || !_liftParams.MDPSummarizeTopLayerOnly)) ||
            // ReSharper disable once UseMethodAny.0
            _cellProfile.Layers.Count() > 0 ||
            _gridDataType == GridDataType.CCA || _gridDataType == GridDataType.CCAPercent) // no CCA settings
        {
          _clientGrid.AssignFilteredValue(stripeIndex, j, _assignmentContext);
        }
      }
    }

    private void ChooseSingleFilteredPassForCell(byte stripeIndex, byte j)
    {
      _cellPassIterator.SetCellCoordinatesInSubgrid(stripeIndex, j);

      if (_filter.AttributeFilter.HasElevationRangeFilter)
        _cellPassIterator.SetIteratorElevationRange(_filterAnnex.ElevationRangeBottomElevationForCell, _filterAnnex.ElevationRangeTopElevationForCell);

      _cellPassIterator.Initialise();

      if (_filter.AttributeFilter.HasPassCountRangeFilter)
        ProcessCellPassesPassCountRange();
      else
        ProcessCellPasses();

      if (_haveFilteredPass &&
          (_firstPassMinElev || _filter.AttributeFilter.HasElevationTypeFilter &&
            (_filter.AttributeFilter.ElevationType == ElevationType.Highest || _filter.AttributeFilter.ElevationType == ElevationType.Lowest)))
      {
        _assignmentContext.FilteredValue.FilteredPassData = _tempPass;
        _assignmentContext.FilteredValue.PassCount = -1;
      }
    }

    private void ChooseSingleFilteredPassForCellPromProfiler(byte stripeIndex, byte j, out int topMostLayerCompactionHalfPassCount)
    {
      // if (Debug_ExtremeLogSwitchD) Log.LogDebug{$"SI@{StripeIndex}/{J} at {clientGrid.OriginX}x{clientGrid.OriginY}: Using profiler");
      topMostLayerCompactionHalfPassCount = 0;

      if (_canUseGlobalLatestCells)
      {
        // Optimistically assume that the global latest value is acceptable
        AssignRequiredFilteredPassAttributesFromGlobalLatestCells(ref _assignmentContext.FilteredValue.FilteredPassData.FilteredPass, stripeIndex, j);

        _assignmentContext.FilteredValue.PassCount = -1;

        // Check to see if there is a non-null value for the requested field in the latest value.
        // If there is none, then there is no non-null value in any of the recorded cells passes
        // so the null value may be returned as the filtered value.
        if (_clientGrid.AssignableFilteredValueIsNull(ref _assignmentContext.FilteredValue.FilteredPassData))
        {
          return;
        }

        if (_clientGrid.WantsLiftProcessingResults())
          _haveFilteredPass = IsFilteredValueFromLatestCellPass(stripeIndex, j);

        if (_haveFilteredPass)
          FiltersValuePopulation.PopulateFilteredValues(_siteModel.MachinesTargetValues[_currentPass.FilteredPass.InternalSiteModelMachineIndex],
            _populationControl, ref _assignmentContext.FilteredValue.FilteredPassData);
      }

      if (!_haveFilteredPass)
      {
        _cellPassIterator.SetCellCoordinatesInSubgrid(stripeIndex, j);

        // if (Debug_ExtremeLogSwitchD)  Log.LogDebug{$"SI@{StripeIndex}/{J} at {clientGrid.OriginX}x{clientGrid.OriginY}: Calling BuildLiftsForCell");

        if (_profiler.CellLiftBuilder.Build(_cellProfile, _liftParams, _clientGrid,
          _assignmentContext, // Place a filtered value into this assignment context
          _cellPassIterator, // Iterate over the cells using this cell pass iterator
          true)) // Return an individual filtered value
        {
          // topMostLayerPassCount = _profiler.CellLiftBuilder.FilteredPassCountOfTopMostLayer;
          topMostLayerCompactionHalfPassCount = _profiler.CellLiftBuilder.FilteredHalfCellPassCountOfTopMostLayer;

          // Filtered value selection is combined with lift analysis in this context via
          // the provision of the client grid and the assignment context to the lift analysis engine

          // If we have a temperature filter to be filtered by last pass
          if (_filter.AttributeFilter.HasTemperatureRangeFilter && _filter.AttributeFilter.FilterTemperatureByLastPass)
          {
            var materialTemperature = _cellProfile.Passes.FilteredPassData[_cellProfile.Passes.PassCount - 1].FilteredPass.MaterialTemperature;
            _haveFilteredPass = materialTemperature != CellPassConsts.NullMaterialTemperatureValue &&
                                Range.InRange(materialTemperature, _filter.AttributeFilter.MaterialTemperatureMin, _filter.AttributeFilter.MaterialTemperatureMax);
          }
          else
            _haveFilteredPass = true;
        }

        // if (Debug_ExtremeLogSwitchD) Log.LogDebug{$"SI@{StripeIndex}/{J} at {clientGrid.OriginX}x{clientGrid.OriginY}: Call to BuildLiftsForCell completed");
      }
    }

    private void ChooseSingleFilteredValueFromLastPass(byte stripeIndex, byte j)
    {
      // if (Debug_ExtremeLogSwitchD) Log.LogDebug{$"SI@{StripeIndex}/{J} at {clientGrid.OriginX}x{clientGrid.OriginY}: Using last pass grid");

      AssignRequiredFilteredPassAttributesFromGlobalLatestCells(ref _assignmentContext.FilteredValue.FilteredPassData.FilteredPass, stripeIndex, j);

      _haveFilteredPass = true;
      _assignmentContext.FilteredValue.PassCount = -1;
    }

    /// <summary>
    /// Retrieves cell values for a sub grid stripe at a time.
    /// </summary>
    /// <returns></returns>
    public override void RetrieveSubGridStripe(byte stripeIndex)
    {
      //  int topMostLayerPassCount = 0;
      var topMostLayerCompactionHalfPassCount = 0;

      // if (Debug_ExtremeLogSwitchD) Log.LogDebug($"Beginning stripe iteration {StripeIndex} at {clientGrid.OriginX}x{clientGrid.OriginY}");

      // Iterate over the cells in the sub grid applying the filter and assigning the requested information into the sub grid
      for (byte j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
      {
        // If this cell is not included in the scan mask then prune execution here for the cell
        // For pass attributes that are maintained on a historical last pass basis (meaning their values bubble up through cell passes where the values of
        // those attributes are null), check the global latest pass version of those values. If they are null, then no further work needs to be done
        if (!_aggregatedCellScanMap.BitSet(stripeIndex, j) || LatestCellPassAttributeIsNull(stripeIndex, j))
          continue;

        if (_gridDataType == GridDataType.CellProfile) // all requests using this data type should filter temperature range using last pass only
          _filter.AttributeFilter.FilterTemperatureByLastPass = true;

        _haveFilteredPass = false;

        CheckForMinimumElevationRequirements(stripeIndex, j);

        if (_useLastPassGrid)
        {
          ChooseSingleFilteredValueFromLastPass(stripeIndex, j);
        }
        else
        {
          _filterAnnex.InitializeFilteringForCell(_filter.AttributeFilter, stripeIndex, j);

          if (_profiler != null)
          {
            ChooseSingleFilteredPassForCellPromProfiler(stripeIndex, j, out topMostLayerCompactionHalfPassCount);
          }
          else
          {
            ChooseSingleFilteredPassForCell(stripeIndex, j);
          }
        }

        if (_haveFilteredPass)
        {
          AssignedFilteredValueContextToClient(stripeIndex, j, topMostLayerCompactionHalfPassCount);
        }
      }

      //if (Debug_ExtremeLogSwitchD) Log.LogDebug("Completed stripe iteration {StripeIndex} at {clientGrid.OriginX}x{clientGrid.OriginY}");
    }
  }
}
