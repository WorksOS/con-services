using Microsoft.Extensions.Logging;
using System;
using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Types;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Filters.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Profiling;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.Profiling.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Iterators;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities;

namespace VSS.TRex.SubGrids
{
  /// <summary>
  /// Contains and orchestrates the business logic for processing sub grids...
  /// </summary>
  public class SubGridRetriever
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridRetriever>();

    // Local state populated by the retriever constructor
    private readonly ICombinedFilter _filter;
    private readonly ICellPassAttributeFilterProcessingAnnex _filterAnnex;
    private readonly ISiteModel _siteModel;
    private readonly IStorageProxy _storageProxy;
    private bool _canUseGlobalLatestCells;
    private readonly bool _hasOverrideSpatialCellRestriction;
    private readonly BoundingIntegerExtent2D _overrideSpatialCellRestriction;
    private readonly bool _prepareGridForCacheStorageIfNoSieving;
    private readonly int _maxNumberOfPassesToReturn;
    private readonly AreaControlSet _areaControlSet;

    // Local state populated for the purpose of access from various local methods
    private IClientLeafSubGrid _clientGrid;
    private ClientLeafSubGrid _clientGridAsLeaf;
    private readonly GridDataType _gridDataType;
    private bool _sieveFilterInUse;

    private SubGridTreeBitmapSubGridBits _sieveBitmask;

    private ISubGrid _subGrid;
    private IServerLeafSubGrid _subGridAsLeaf;

    private readonly FilteredValueAssignmentContext _assignmentContext;
    private ISubGridSegmentIterator _segmentIterator;
    private SubGridSegmentCellPassIterator_NonStatic _cellPassIterator;
    private readonly IFilteredValuePopulationControl _populationControl;

    private IProfilerBuilder<ProfileCell> _profiler;
    private ProfileCell _cellProfile;

    private readonly ISubGridTreeBitMask _pdExistenceMap;

    private bool _haveFilteredPass;
    private FilteredPassData _currentPass;
    private FilteredPassData _tempPass;

    private ISubGridCellLatestPassDataWrapper _globalLatestCells;
    private bool _useLastPassGrid; // Assume we can't use last pass data

    private readonly SubGridTreeBitmapSubGridBits _aggregatedCellScanMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

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
      ISubGridTreeBitMask pDExistenceMap)
    {
      _siteModel = siteModel;
      _gridDataType = gridDataType;
      _storageProxy = storageProxy;
      _segmentIterator = null;
      _cellPassIterator = null;
      _filter = filter ?? new CombinedFilter();
      _filterAnnex = filterAnnex;
      _canUseGlobalLatestCells = _filter.AttributeFilter.LastRecordedCellPassSatisfiesFilter;
      _hasOverrideSpatialCellRestriction = hasOverrideSpatialCellRestriction;
      _overrideSpatialCellRestriction = overrideSpatialCellRestriction;
      _prepareGridForCacheStorageIfNoSieving = prepareGridForCacheStorageIfNoSieving;
      _maxNumberOfPassesToReturn = maxNumberOfPassesToReturn;
      _areaControlSet = areaControlSet;
      _populationControl = populationControl;
      _pdExistenceMap = pDExistenceMap;

      // Create and configure the assignment context which is used to contain a filtered pass and
      // its attendant machine events and target values prior to assignment to the client sub grid.
      _assignmentContext = new FilteredValueAssignmentContext();
    }

    /// <summary>
    /// Performs primary filtered iteration over cell passes in the cell being processed to determine the cell pass
    /// to be selected.
    /// </summary>
    private void ProcessCellPasses()
    {
      bool haveHalfPass = false;
      int passRangeCount = 0;
      bool firstFilteredCellPass = true;

      while (_cellPassIterator.MayHaveMoreFilterableCellPasses() &&
             _cellPassIterator.GetNextCellPass(ref _currentPass.FilteredPass))
      {
        FiltersValuePopulation.PopulateFilteredValues(_siteModel.MachinesTargetValues[_currentPass.FilteredPass.InternalSiteModelMachineIndex], _populationControl, ref _currentPass);

        if (_filter.AttributeFilter.FilterPass(ref _currentPass, _filterAnnex))
        {
          // -->###US79098###
          if (firstFilteredCellPass) 
          {
            // if the first filtered pass returned by GetNextCellPass was recorded by an Excavator machine
            // with the minimum elevation mapping mode selected then scan all cell passes until one is encountered
            // which fails that test. The filtered cell pass is then set to be the cell pass with the lowest 
            // measured elevation of that set of cell passes
            var internalMachineIndex = _currentPass.FilteredPass.InternalSiteModelMachineIndex;
            var machine = _siteModel.Machines[internalMachineIndex];
            var machineIsAnExcavator = machine.MachineType == MachineType.Excavator;
            var mappingMode = _siteModel.MachinesTargetValues[internalMachineIndex].ElevationMappingModeStateEvents.GetValueAtDate(_currentPass.FilteredPass.Time, out _, ElevationMappingMode.LatestElevation);
            var minimumElevationMappingModeAtCellPassTime = mappingMode == ElevationMappingMode.MinimumElevation;

            if (machineIsAnExcavator && minimumElevationMappingModeAtCellPassTime)
            {
              // TODO: Assumption validation: Once this workflow is entered the only expected output is the cell pass with the lowest elevation per the selection rules below.

              CellPass _nextCurrentPass = new CellPass();
              CellPass _lowestPass = _currentPass.FilteredPass;

              while (_cellPassIterator.MayHaveMoreFilterableCellPasses() && _cellPassIterator.GetNextCellPass(ref _nextCurrentPass))
              {
                var nextInternalMachineIndex = _nextCurrentPass.InternalSiteModelMachineIndex;
                var nextMachine = _siteModel.Machines[nextInternalMachineIndex];
                var nextMachineIsAnExcavator = nextMachine.MachineType == MachineType.Excavator;
                var nextMappingMode = _siteModel.MachinesTargetValues[internalMachineIndex].ElevationMappingModeStateEvents.GetValueAtDate(_nextCurrentPass.Time, out _, ElevationMappingMode.LatestElevation);
                var nextMinimumElevationMappingModeAtCellPassTime = nextMappingMode == ElevationMappingMode.MinimumElevation;

                if (nextMachineIsAnExcavator && nextMinimumElevationMappingModeAtCellPassTime)
                {
                  // Still an excavator machine, check if this pass is lower than the lowest.
                  if (_nextCurrentPass.Height < _lowestPass.Height)
                    _lowestPass = _nextCurrentPass;
                }
                else
                {
                  // This cell pass was made by a new machine not meeting the filter, or minimum elevation mapping mode.
                  // This terminates search for lowest pass; Return the lowest elevation cell pass encountered in the search.
                  break;
                }
              }

              _currentPass.FilteredPass = _lowestPass;
              FiltersValuePopulation.PopulateFilteredValues(_siteModel.MachinesTargetValues[_currentPass.FilteredPass.InternalSiteModelMachineIndex], _populationControl, ref _currentPass);
              _haveFilteredPass = true;
              _assignmentContext.FilteredValue.PassCount = CellPassConsts.NullPassCountValue;
            }
          }
          // <--###US79098###

          if (_filter.AttributeFilter.HasPassCountRangeFilter)
          {
            if (_currentPass.FilteredPass.HalfPass)
            {
              if (!haveHalfPass)
                ++passRangeCount; // increase count for first half pass
              haveHalfPass = !haveHalfPass;
            }
            else
            {
              ++passRangeCount; // increase count for first full pass
            }

            if (!Range.InRange(passRangeCount, _filter.AttributeFilter.PassCountRangeMin, _filter.AttributeFilter.PassCountRangeMax))
              continue;
          }

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
          else
          {
            // All criteria have been met for acceptance of this pass
            _assignmentContext.FilteredValue.FilteredPassData = _currentPass;
            _haveFilteredPass = true;
            _assignmentContext.FilteredValue.PassCount = CellPassConsts.NullPassCountValue;
            break;
          }

          firstFilteredCellPass = false;
        }
      }
    }

    /// <summary>
    /// Performs extraction of specific attributes from a GlobalLatestCells structure depending on the type of
    /// grid being retrieved
    /// </summary>
    /// <param name="cellPass"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private void AssignRequiredFilteredPassAttributesFromGlobalLatestCells(ref CellPass cellPass, uint x, uint y)
    {
      switch (_gridDataType)
      {
        case GridDataType.Height:
          cellPass.Height = _globalLatestCells.ReadHeight(x, y);
          break;
        case GridDataType.HeightAndTime:
        case GridDataType.CutFill:
          cellPass.Height = _globalLatestCells.ReadHeight(x, y);
          cellPass.Time = _globalLatestCells.ReadTime(x, y);
          break;
        case GridDataType.CCV:
          cellPass.CCV = _globalLatestCells.ReadCCV(x, y);
          break;
        case GridDataType.RMV:
          cellPass.RMV = _globalLatestCells.ReadRMV(x, y);
          break;
        case GridDataType.Frequency:
          cellPass.Frequency = _globalLatestCells.ReadFrequency(x, y);
          break;
        case GridDataType.Amplitude:
          cellPass.Amplitude = _globalLatestCells.ReadAmplitude(x, y);
          break;
        case GridDataType.GPSMode:
          cellPass.gpsMode = _globalLatestCells.ReadGPSMode(x, y);
          break;
        case GridDataType.MDP:
          cellPass.MDP = _globalLatestCells.ReadMDP(x, y);
          break;
        case GridDataType.CCA:
          cellPass.CCA = _globalLatestCells.ReadCCA(x, y);
          break;
        case GridDataType.Temperature:
          cellPass.MaterialTemperature = _globalLatestCells.ReadTemperature(x, y);
          break;
        case GridDataType.TemperatureDetail:
          cellPass.MaterialTemperature = _globalLatestCells.ReadTemperature(x, y);
          break;
        default:
          throw new TRexSubGridProcessingException($"Unsupported grid data type in AssignRequiredFilteredPassAttributesFromGlobalLatestCells: {_gridDataType}");
      }
    }

    /// <summary>
    /// Determine if the given cell has an appropriate values stored in the latest cell pass information for the sub grid
    /// </summary>
    /// <param name="StripeIndex"></param>
    /// <param name="J"></param>
    /// <returns></returns>
    private bool IsFilteredValueFromLatestCellPass(int StripeIndex, int J)
    {
      switch (_gridDataType)
      {
        case GridDataType.CCV: return _globalLatestCells.CCVValuesAreFromLastPass.BitSet(StripeIndex, J);
        case GridDataType.RMV: return _globalLatestCells.RMVValuesAreFromLastPass.BitSet(StripeIndex, J);
        case GridDataType.Frequency: return _globalLatestCells.FrequencyValuesAreFromLastPass.BitSet(StripeIndex, J);
        case GridDataType.Amplitude: return _globalLatestCells.AmplitudeValuesAreFromLastPass.BitSet(StripeIndex, J);
        case GridDataType.Temperature: return _globalLatestCells.TemperatureValuesAreFromLastPass.BitSet(StripeIndex, J);
        case GridDataType.GPSMode: return _globalLatestCells.GPSModeValuesAreFromLatestCellPass.BitSet(StripeIndex, J);
        case GridDataType.MDP: return _globalLatestCells.MDPValuesAreFromLastPass.BitSet(StripeIndex, J);
        case GridDataType.CCA: return _globalLatestCells.CCAValuesAreFromLastPass.BitSet(StripeIndex, J);
        case GridDataType.TemperatureDetail: return _globalLatestCells.TemperatureValuesAreFromLastPass.BitSet(StripeIndex, J);
        case GridDataType.CCVPercentChange:
        case GridDataType.CCVPercentChangeIgnoredTopNullValue:
        case GridDataType.MachineSpeedTarget:
        case GridDataType.PassCount: return false;
        default:
          throw new TRexSubGridProcessingException("Unimplemented data type for sub grid requiring lift processing results");
      }
    }

    /// <summary>
    /// Determines if there is null value for the required grid data type in the latest cell pass information.
    /// If the grid data type is not represented in the latest cell pass information this method returns false.
    /// </summary>
    /// <returns></returns>
    private bool LatestCellPassAttributeIsNull(uint StripeIndex, uint J)
    {
      switch (_gridDataType)
      {
        case GridDataType.CCV: return _globalLatestCells.ReadCCV(StripeIndex, J) == CellPassConsts.NullCCV;
        case GridDataType.RMV: return _globalLatestCells.ReadRMV(StripeIndex, J) == CellPassConsts.NullRMV;
        case GridDataType.Frequency: return _globalLatestCells.ReadFrequency(StripeIndex, J) == CellPassConsts.NullFrequency;
        case GridDataType.Amplitude: return _globalLatestCells.ReadAmplitude(StripeIndex, J) == CellPassConsts.NullAmplitude;
        case GridDataType.GPSMode: return _globalLatestCells.ReadGPSMode(StripeIndex, J) == GPSMode.NoGPS;
        case GridDataType.MDP: return _globalLatestCells.ReadMDP(StripeIndex, J) == CellPassConsts.NullMDP;
        case GridDataType.CCA: return _globalLatestCells.ReadCCA(StripeIndex, J) == CellPassConsts.NullCCA;
        case GridDataType.Temperature: return _globalLatestCells.ReadTemperature(StripeIndex, J) == CellPassConsts.NullMaterialTemperatureValue;
        case GridDataType.TemperatureDetail: return _globalLatestCells.ReadTemperature(StripeIndex, J) == CellPassConsts.NullMaterialTemperatureValue;
        case GridDataType.CCVPercentChange: return _globalLatestCells.ReadCCV(StripeIndex, J) == CellPassConsts.NullCCV;
        case GridDataType.CCVPercentChangeIgnoredTopNullValue: return _globalLatestCells.ReadCCV(StripeIndex, J) == CellPassConsts.NullCCV;
      }

      return false;
    }

    /// <summary>
    /// Retrieves cell values for a sub grid stripe at a time.
    /// </summary>
    /// <returns></returns>
    private void RetrieveSubGridStripe(byte StripeIndex)
    {
      //  int TopMostLayerPassCount = 0;
      int TopMostLayerCompactionHalfPassCount = 0;

      // if (Debug_ExtremeLogSwitchD) Log.LogDebug($"Beginning stripe iteration {StripeIndex} at {clientGrid.OriginX}x{clientGrid.OriginY}");

      // Iterate over the cells in the sub grid applying the filter and assigning the requested information into the sub grid
      for (byte J = 0; J < SubGridTreeConsts.SubGridTreeDimension; J++)
      {
        // If this cell is not included in the scan mask then prune execution here for the cell
        // For pass attributes that are maintained on a historical last pass basis (meaning their values bubble up through cell passes where the values of
        // those attributes are null), check the global latest pass version of those values. If they are null, then no further work needs to be done
        if (!_aggregatedCellScanMap.BitSet(StripeIndex, J) || LatestCellPassAttributeIsNull(StripeIndex, J))
          continue;

        if (_gridDataType == GridDataType.CellProfile) // all requests using this data type should filter temperature range using last pass only
          _filter.AttributeFilter.FilterTemperatureByLastPass = true;

        _haveFilteredPass = false;

        // ###US79098### -->
        if (_useLastPassGrid)
        {
          // Determine if there is an elevation mapping mode that may require searching through cell passes. If so, the last pass grid can
          // only be used if the machine that recorded that last pass is not an excavator with an elevation mode set to MinimumHeight.
          // This only applies if there is not an elevation mapping mode filter selecting cells with LatestPass mapping mode
          if (_gridDataType == GridDataType.CutFill || _gridDataType == GridDataType.Height || _gridDataType == GridDataType.HeightAndTime)
             
          {
            var internalMachineIndex = _globalLatestCells.ReadInternalMachineIndex(StripeIndex, J);
            if (internalMachineIndex != CellPassConsts.NullInternalSiteModelMachineIndex)
            {
              var machine = _siteModel.Machines[internalMachineIndex];

              bool machineIsAnExcavator = machine.MachineType == MachineType.Excavator;
              var mappingMode = _siteModel.MachinesTargetValues[internalMachineIndex].ElevationMappingModeStateEvents
                .LastStateValue();

              bool minimumElevationMappingModeAtLatestCellPassTime =
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

        if (_useLastPassGrid)
        {
          // if (Debug_ExtremeLogSwitchD) Log.LogDebug{$"SI@{StripeIndex}/{J} at {clientGrid.OriginX}x{clientGrid.OriginY}: Using last pass grid");

          AssignRequiredFilteredPassAttributesFromGlobalLatestCells(ref _assignmentContext.FilteredValue.FilteredPassData.FilteredPass, StripeIndex, J);

          // TODO: Review if line below replaced with line above in Ignite POC is good...
          // AssignmentContext.FilteredValue.FilteredPassData.FilteredPass = _GlobalLatestCells[StripeIndex, J];

          _haveFilteredPass = true;
          _assignmentContext.FilteredValue.PassCount = -1;
        }
        else
        {
          // if (Debug_ExtremeLogSwitchD) Log.LogDebug{$"SI@{StripeIndex}/{J} at {clientGrid.OriginX}x{clientGrid.OriginY}: Using profiler");

          _filterAnnex.InitializeFilteringForCell(_filter.AttributeFilter, StripeIndex, J);

          if (_profiler != null)
          {
            // While we have been given a profiler, we may not need to use it to analyze layers in the cell pass stack.
            // The layer analysis in this operation is intended to locate cell passes belonging to superseded layers,
            // in which case they are not considered for providing the requested value. However, if there is no filter
            // in effect, then the global latest information for the sub grid may be consulted first to see if the
            // appropriate values came from the last physically collected cell pass in the cell. Note that the tracking
            // of latest values is also true for time, so that the time recorded in the latest values structure also
            // includes that cell pass time.

            if (_canUseGlobalLatestCells)
            {
              // Optimistically assume that the global latest value is acceptable
              AssignRequiredFilteredPassAttributesFromGlobalLatestCells(ref _assignmentContext.FilteredValue.FilteredPassData.FilteredPass, StripeIndex, J);

              // TODO: Review if line below replaced with line above in Ignite POC is good...
              // AssignmentContext.FilteredValue.FilteredPassData.FilteredPass = _GlobalLatestCells[StripeIndex, J];

              _assignmentContext.FilteredValue.PassCount = -1;

              // Check to see if there is a non-null value for the requested field in the latest value.
              // If there is none, then there is no non-null value in any of the recorded cells passes
              // so the null value may be returned as the filtered value.
              if (_clientGrid.AssignableFilteredValueIsNull(ref _assignmentContext.FilteredValue.FilteredPassData))
                continue;

              if (_clientGrid.WantsLiftProcessingResults())
                _haveFilteredPass = IsFilteredValueFromLatestCellPass(StripeIndex, J);

              if (_haveFilteredPass)
                FiltersValuePopulation.PopulateFilteredValues(_siteModel.MachinesTargetValues[_currentPass.FilteredPass.InternalSiteModelMachineIndex],
                  _populationControl, ref _assignmentContext.FilteredValue.FilteredPassData);
            }

            if (!_haveFilteredPass)
            {
              _cellPassIterator.SetCellCoordinatesInSubgrid(StripeIndex, J);

              // if (Debug_ExtremeLogSwitchD)  Log.LogDebug{$"SI@{StripeIndex}/{J} at {clientGrid.OriginX}x{clientGrid.OriginY}: Calling BuildLiftsForCell");

              if (_profiler.CellLiftBuilder.Build(_cellProfile, _clientGrid,
                _assignmentContext, // Place a filtered value into this assignment context
                _cellPassIterator, // Iterate over the cells using this cell pass iterator
                true)) // Return an individual filtered value
              {
                // TopMostLayerPassCount = _profiler.CellLiftBuilder.FilteredPassCountOfTopMostLayer;
                TopMostLayerCompactionHalfPassCount = _profiler.CellLiftBuilder.FilteredHalfCellPassCountOfTopMostLayer;

                // Filtered value selection is combined with lift analysis in this context via
                // the provision of the client grid and the assignment context to the lift analysis engine

                // If we have a temperature filter to be filtered by last pass
                if (_filter.AttributeFilter.HasTemperatureRangeFilter && _filter.AttributeFilter.FilterTemperatureByLastPass)
                {
                  var _materialTemperature = _cellProfile.Passes.FilteredPassData[_cellProfile.Passes.PassCount - 1].FilteredPass.MaterialTemperature;
                  _haveFilteredPass = _materialTemperature != CellPassConsts.NullMaterialTemperatureValue &&
                                      Range.InRange(_materialTemperature, _filter.AttributeFilter.MaterialTemperatureMin, _filter.AttributeFilter.MaterialTemperatureMax);
                }
                else
                  _haveFilteredPass = true;
              }

              // if (Debug_ExtremeLogSwitchD) Log.LogDebug{$"SI@{StripeIndex}/{J} at {clientGrid.OriginX}x{clientGrid.OriginY}: Call to BuildLiftsForCell completed");
            }
          }
          else
          {
            _cellPassIterator.SetCellCoordinatesInSubgrid(StripeIndex, J);

            if (_filter.AttributeFilter.HasElevationRangeFilter)
              _cellPassIterator.SetIteratorElevationRange(_filterAnnex.ElevationRangeBottomElevationForCell, _filterAnnex.ElevationRangeTopElevationForCell);

            _cellPassIterator.Initialise();
            ProcessCellPasses();

            if (_haveFilteredPass &&
                (_filter.AttributeFilter.HasElevationTypeFilter &&
                 (_filter.AttributeFilter.ElevationType == ElevationType.Highest || _filter.AttributeFilter.ElevationType == ElevationType.Lowest)))
            {
              _assignmentContext.FilteredValue.FilteredPassData = _tempPass;
              _assignmentContext.FilteredValue.PassCount = -1;
            }
          }
        }

        if (_haveFilteredPass)
        {
          if (_gridDataType == GridDataType.PassCount || _gridDataType == GridDataType.CellProfile)
            _assignmentContext.FilteredValue.PassCount = TopMostLayerCompactionHalfPassCount / 2;

          // If we are displaying a CCV summary view or are displaying a summary of only
          // the top layer in the cell pass stack, then we need to make additional checks to
          // determine if the CCV value filtered from the cell passes is not overridden by
          // the layer in question being superseded. If that is the case, then the CCV value
          // is not assigned to the result set to be passed back to the client as it effectively
          // does not exist given this situation.

          if (_cellProfile == null)
            _clientGrid.AssignFilteredValue(StripeIndex, J, _assignmentContext);
          else
          {
            if (((_gridDataType == GridDataType.CCV || _gridDataType == GridDataType.CCVPercent) && (Dummy_LiftBuildSettings.CCVSummaryTypes == 0 || !Dummy_LiftBuildSettings.CCVSummarizeTopLayerOnly)) ||
                ((_gridDataType == GridDataType.MDP || _gridDataType == GridDataType.MDPPercent) && (Dummy_LiftBuildSettings.MDPSummaryTypes == 0 || !Dummy_LiftBuildSettings.MDPSummarizeTopLayerOnly)) ||
                // ReSharper disable once UseMethodAny.0
                _cellProfile.Layers.Count() > 0 ||
                _gridDataType == GridDataType.CCA || _gridDataType == GridDataType.CCAPercent) // no CCA settings
              _clientGrid.AssignFilteredValue(StripeIndex, J, _assignmentContext);
          }
        }
      }

      //if (Debug_ExtremeLogSwitchD) Log.LogDebug("Completed stripe iteration {StripeIndex} at {clientGrid.OriginX}x{clientGrid.OriginY}");
    }

    /// <summary>
    /// PruneSubGridRetrievalHere determines if there is no point continuing the
    /// process of retrieving the sub grid due to the impossibility of returning any
    /// valid values for any cells in the sub grid due to a combination of filter
    /// settings and flags set in the sub grid that denote the types of data that
    /// are, or are not, contained in the sub grid.
    /// </summary>
    /// <returns></returns>
    private bool PruneSubGridRetrievalHere()
    {
      // Check the sub grid global attribute presence flags that are tracked for optional
      // attribute values to see if there is anything at all that needs to be done here
      switch (_gridDataType)
      {
        case GridDataType.CCV: return !_globalLatestCells.HasCCVData();
        case GridDataType.RMV: return !_globalLatestCells.HasRMVData();
        case GridDataType.Frequency: return !_globalLatestCells.HasFrequencyData();
        case GridDataType.Amplitude: return !_globalLatestCells.HasAmplitudeData();
        case GridDataType.GPSMode: return !_globalLatestCells.HasGPSModeData();
        case GridDataType.Temperature: return !_globalLatestCells.HasTemperatureData();
        case GridDataType.MDP: return !_globalLatestCells.HasMDPData();
        case GridDataType.CCA: return !_globalLatestCells.HasCCAData();
        case GridDataType.TemperatureDetail: return !_globalLatestCells.HasTemperatureData();
        default: return false;
      }
    }

    private bool _commonCellPassStackExaminationDone;

    private void SetupForCellPassStackExamination()
    {
      if (!_commonCellPassStackExaminationDone)
      {
        _populationControl.PreparePopulationControl(_gridDataType, /* todo LiftBuildSettings, */ _filter.AttributeFilter, _clientGrid.EventPopulationFlags);

        _filter.AttributeFilter.RequestedGridDataType = _gridDataType;

        // Create and configure the segment iterator to be used
        _segmentIterator = new SubGridSegmentIterator(_subGridAsLeaf, _subGridAsLeaf.Directory, _storageProxy);

        if (_filter.AttributeFilter.HasMachineFilter)
          _segmentIterator.SetMachineRestriction(_filter.AttributeFilter.GetMachineIDsSet());

        // Create and configure the cell pass iterator to be used

        _cellPassIterator = new SubGridSegmentCellPassIterator_NonStatic(_segmentIterator);
        _cellPassIterator.SetTimeRange(_filter.AttributeFilter.HasTimeFilter, _filter.AttributeFilter.StartTime, _filter.AttributeFilter.EndTime);

        _commonCellPassStackExaminationDone = true;
      }

      if (_filter.AttributeFilter.ReturnEarliestFilteredCellPass ||
          (_filter.AttributeFilter.HasElevationTypeFilter && _filter.AttributeFilter.ElevationType == ElevationType.First))
        _segmentIterator.IterationDirection = IterationDirection.Forwards;
      else
        _segmentIterator.IterationDirection = IterationDirection.Backwards;

      _segmentIterator.SubGrid = _subGridAsLeaf;
      _segmentIterator.Directory = _subGridAsLeaf.Directory;
    }

    public ServerRequestResult RetrieveSubGrid(// liftBuildSettings          : TICLiftBuildSettings;
      IClientLeafSubGrid clientGrid,
      SubGridTreeBitmapSubGridBits cellOverrideMask)
    {
      if (!Utilities.DerivedGridDataTypesAreCompatible(_gridDataType, clientGrid.GridDataType))
      {
        throw new TRexSubGridProcessingException($"Grid data type of client leaf sub grid [{clientGrid.GridDataType}] is not compatible with the grid data type of retriever [{_gridDataType}]");
      }

      ServerRequestResult Result = ServerRequestResult.UnknownError;

      //  SIGLogMessage.PublishNoODS(Nil, Format('In RetrieveSubGrid: Active pass filters = %s, Active cell filters = %s', [PassFilter.ActiveFiltersText, CellFilter.ActiveFiltersText]));

      // Set up class local state for other methods to access
      _clientGrid = clientGrid;
      _clientGridAsLeaf = clientGrid as ClientLeafSubGrid;

      _canUseGlobalLatestCells &=
       // todo: Re-add when lift build settings available
       // !(_gridDataType == GridDataType.CCV ||
       //   _gridDataType == GridDataType.CCVPercent) /*&& (LiftBuildSettings.CCVSummaryTypes<>[])*/ &&
       // !(_gridDataType == GridDataType.MDP ||
       //   _gridDataType == GridDataType.MDPPercent) /*&& (LiftBuildSettings.MDPSummaryTypes<>[])*/ &&
        !(_gridDataType == GridDataType.CCA || _gridDataType == GridDataType.CCAPercent) &&
        !(_gridDataType == GridDataType.CellProfile ||
          _gridDataType == GridDataType.PassCount ||
          _gridDataType == GridDataType.CellPasses ||
          _gridDataType == GridDataType.MachineSpeed ||
          _gridDataType == GridDataType.CCVPercentChange ||
          _gridDataType == GridDataType.MachineSpeedTarget ||
          _gridDataType == GridDataType.CCVPercentChangeIgnoredTopNullValue);

      // Support for lazy construction of any required profiling infrastructure
      if (_clientGrid.WantsLiftProcessingResults() && _profiler == null)
      {
        // Some display types require lift processing to be able to select the
        // appropriate cell pass containing the filtered value required.

        _profiler = DIContext.Obtain<IProfilerBuilder<ProfileCell>>();

        _profiler.Configure(ProfileStyle.CellPasses, _siteModel, _pdExistenceMap, _gridDataType, new FilterSet(_filter),
          null,null, _populationControl, new CellPassFastEventLookerUpper(_siteModel));

        _cellProfile = new ProfileCell();

        // Create and configure the assignment context which is used to contain
        // a filtered pass and its attendant machine events and target values
        // prior to assignment to the client sub grid.
        _assignmentContext.CellProfile = _cellProfile;
      }

      try
      {
        // Ensure pass type filter is set correctly
        if (_filter.AttributeFilter.HasPassTypeFilter)
          if ((_filter.AttributeFilter.PassTypeSet & (PassTypeSet.Front | PassTypeSet.Rear)) == PassTypeSet.Front)
            _filter.AttributeFilter.PassTypeSet |= PassTypeSet.Rear; // these two types go together as half passes

        // ... unless we can use the last pass grid to satisfy the query
        if (_canUseGlobalLatestCells &&
            !_filter.AttributeFilter.HasElevationRangeFilter &&
            !_clientGrid.WantsLiftProcessingResults() &&
            !_filter.AttributeFilter.HasElevationMappingModeFilter &&
            !(_filter.AttributeFilter.HasElevationTypeFilter &&
              (_filter.AttributeFilter.ElevationType == ElevationType.Highest ||
               _filter.AttributeFilter.ElevationType == ElevationType.Lowest)) &&
            !(_gridDataType == GridDataType.PassCount || _gridDataType == GridDataType.Temperature ||
              _gridDataType == GridDataType.CellProfile || _gridDataType == GridDataType.CellPasses ||
              _gridDataType == GridDataType.MachineSpeed))
        {
          _useLastPassGrid = true;
        }

        // First get the sub grid we are interested in
        // SIGLogMessage.PublishNoODS(Nil, Format('Begin LocateSubGridContaining at %dx%d', [clientGrid.OriginX, clientGrid.OriginY])); 

        _subGrid = SubGridTrees.Server.Utilities.SubGridUtilities.LocateSubGridContaining(_storageProxy, _siteModel.Grid, clientGrid.OriginX, clientGrid.OriginY, _siteModel.Grid.NumLevels, false, false);

        //  SIGLogMessage.PublishNoODS(Nil, Format('End LocateSubGridContaining at %dx%d', [clientGrid.OriginX, clientGrid.Origin]));

        if (_subGrid == null) // This should never really happen, but we'll be polite about it
        {
          Log.LogWarning(
            $"Sub grid address (CellX={clientGrid.OriginX}, CellY={clientGrid.OriginY}) passed to LocateSubGridContaining() from RetrieveSubGrid() did not match an existing sub grid in the data model. Returning SubGridNotFound as response with a null sub grid reference.");
          return ServerRequestResult.SubGridNotFound;
        }

        // Now process the contents of that sub grid into the sub grid to be returned to the client.

        if (!_subGrid.IsLeafSubGrid())
        {
          Log.LogInformation("Requests of node sub grids in the server sub grid are not yet supported");
          return Result;
        }

        if (!(_subGrid is IServerLeafSubGrid))
        {
          Log.LogError($"_SubGrid {_subGrid.Moniker()} is not a server grid leaf node");
          return Result;
        }

        // SIGLogMessage.PublishNoODS(Nil, Format('Getting sub grid leaf at %dx%d', [clientGrid.OriginX, clientGrid.OriginY]));

        _subGridAsLeaf = (IServerLeafSubGrid) _subGrid;
        _globalLatestCells = _subGridAsLeaf.Directory.GlobalLatestCells;

        if (PruneSubGridRetrievalHere())
          return ServerRequestResult.NoError;

        // Determine the bitmask detailing which cells match the cell selection filter
        if (!SubGridFilterMasks.ConstructSubGridCellFilterMask(_subGridAsLeaf, _siteModel, _filter,
          cellOverrideMask, _hasOverrideSpatialCellRestriction, _overrideSpatialCellRestriction,
          _clientGridAsLeaf.ProdDataMap, _clientGridAsLeaf.FilterMap))
        {
          return ServerRequestResult.FailedToComputeDesignFilterPatch;
        }

        // SIGLogMessage.PublishNoODS(Nil, Format('Setup for stripe iteration at %dx%d', [clientGrid.OriginX, clientGrid.OriginY]));
        
        SetupForCellPassStackExamination();

        // Some display types require lift processing to be able to select the appropriate cell pass containing the filtered value required.
        if (_clientGrid.WantsLiftProcessingResults())
        {
          _segmentIterator.IterationDirection = IterationDirection.Forwards;
          _cellPassIterator.MaxNumberOfPassesToReturn = _maxNumberOfPassesToReturn; //VLPDSvcLocations.VLPDPSNode_MaxCellPassIterationDepth_PassCountDetailAndSummary;
        }

        // TODO Add when cell left build settings supported
        // AssignmentContext.LiftBuildSettings = LiftBuildSettings;

        // Determine if a sieve filter is required for the sub grid where the sieve matches
        // the X and Y pixel world size (used for WMS tile computation)
        _subGrid.CalculateWorldOrigin(out double subGridWorldOriginX, out double subGridWorldOriginY);

        _sieveFilterInUse = _areaControlSet.UseIntegerAlgorithm 
          ? GridRotationUtilities.ComputeSieveBitmaskInteger(subGridWorldOriginX, subGridWorldOriginY, _subGrid.Moniker(), _areaControlSet, _siteModel.CellSize, out _sieveBitmask) 
          : GridRotationUtilities.ComputeSieveBitmaskFloat(subGridWorldOriginX, subGridWorldOriginY, _areaControlSet, _siteModel.CellSize, _assignmentContext, out _sieveBitmask);

        if (!_sieveFilterInUse)
        {
          // Reset pixel size parameters to indicate no skip stepping is being performed
          _areaControlSet.PixelXWorldSize = 0;
          _areaControlSet.PixelYWorldSize = 0;
        }

        //if (VLPDSvcLocations.Debug_ExtremeLogSwitchC) Log.LogDebug($"Performing stripe iteration at {clientGrid.OriginX}x{clientGrid.OriginY}");

        // Before iterating over stripes of this sub grid, compute a scan map detailing to the best of our current
        // knowledge, which cells need to be visited

        // Only ask for cells the filter wants and which are actually present in the data set
        _aggregatedCellScanMap.SetAndOf(_clientGridAsLeaf.FilterMap, _globalLatestCells.PassDataExistenceMap); 
        if (_sieveFilterInUse)
          _aggregatedCellScanMap.AndWith(_sieveBitmask); // ... and which are required by any sieve mask
        if (_sieveFilterInUse || !_prepareGridForCacheStorageIfNoSieving)
          _aggregatedCellScanMap.AndWith(_clientGridAsLeaf.ProdDataMap); // ... and which are in the required production data map

        // Iterate over the stripes in the sub grid processing each one in turn.
        for (byte I = 0; I < SubGridTreeConsts.SubGridTreeDimension; I++)
          RetrieveSubGridStripe(I);

        //if VLPDSvcLocations.Debug_ExtremeLogSwitchC then Log.LogDebug($"Stripe iteration complete at {clientGrid.OriginX}x{clientGrid.OriginY}");

        Result = ServerRequestResult.NoError;
      }
      catch (Exception e)
      {
        Log.LogError(e, $"Exception occured in {nameof(RetrieveSubGrid)}");
        throw;
      }

      return Result;
    }
  }
}
