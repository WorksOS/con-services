using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Reflection;
using VSS.TRex.Cells;
using VSS.TRex.Common.CellPasses;
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
using VSS.TRex.Utilities;

namespace VSS.TRex.SubGrids
{
  /// <summary>
  /// Contains and orchestrates the business logic for processing subgrids...
  /// </summary>
  public class SubGridRetriever
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    // Local state populated by the retriever constructor
    private readonly ICombinedFilter _filter;
    private readonly ISiteModel _siteModel;
    private readonly IStorageProxy _storageProxy;
    private bool _canUseGlobalLatestCells;
    private readonly bool _hasOverrideSpatialCellRestriction;
    private readonly BoundingIntegerExtent2D _overrideSpatialCellRestriction;
    private readonly bool _prepareGridForCacheStorageIfNoSeiving;
    private readonly byte _level;
    private readonly int _maxNumberOfPassesToReturn;
    private AreaControlSet _areaControlSet;

    // Local state populated for the purpose of access from various local methods
    private IClientLeafSubGrid _clientGrid;
    private ClientLeafSubGrid _clientGridAsLeaf;
    private GridDataType _gridDataType = GridDataType.All;
    private bool _seiveFilterInUse;

    private SubGridTreeBitmapSubGridBits _seiveBitmask;

    ISubGrid _subGrid;
    IServerLeafSubGrid _subGridAsLeaf;

    private FilteredValueAssignmentContext _assignmentContext;
    private ISubGridSegmentIterator _segmentIterator;
    private SubGridSegmentCellPassIterator_NonStatic _cellPassIterator;
    private readonly IFilteredValuePopulationControl _populationControl;

    private IProfilerBuilder<ProfileCell> _profiler;
    private ProfileCell _cellProfile;

    private readonly ISubGridTreeBitMask _pdExistenceMap;

    // ProductionEventChanges MachineTargetValues = null;

    private bool _haveFilteredPass;
    private FilteredPassData _currentPass;
    private FilteredPassData _tempPass;

    private ISubGridCellLatestPassDataWrapper _globalLatestCells;
    private bool _useLastPassGrid; // Assume we can't use last pass data


    /// <summary>
    /// Constructor for the subgrid retriever helper
    /// </summary>
    /// <param name="sitemodel"></param>
    /// <param name="storageProxy"></param>
    /// <param name="filter"></param>
    /// <param name="hasOverrideSpatialCellRestriction"></param>
    /// <param name="overrideSpatialCellRestriction"></param>
    /// <param name="prepareGridForCacheStorageIfNoSeiving"></param>
    /// <param name="treeLevel"></param>
    /// <param name="maxNumberOfPassesToReturn"></param>
    /// <param name="areaControlSet"></param>
    /// <param name="populationControl"></param>
    /// <param name="pDExistenceMap"></param>
    public SubGridRetriever(ISiteModel sitemodel,
      IStorageProxy storageProxy,
      ICombinedFilter filter,
      bool hasOverrideSpatialCellRestriction,
      BoundingIntegerExtent2D overrideSpatialCellRestriction,
      bool prepareGridForCacheStorageIfNoSeiving,
      byte treeLevel,
      int maxNumberOfPassesToReturn,
      AreaControlSet areaControlSet,
      IFilteredValuePopulationControl populationControl,
      ISubGridTreeBitMask pDExistenceMap)
    {
      _siteModel = sitemodel;
      _storageProxy = storageProxy;
      _segmentIterator = null;
      _cellPassIterator = null;

      _filter = filter ?? new CombinedFilter();

      _canUseGlobalLatestCells = _filter.AttributeFilter.LastRecordedCellPassSatisfiesFilter;

      _hasOverrideSpatialCellRestriction = hasOverrideSpatialCellRestriction;
      _overrideSpatialCellRestriction = overrideSpatialCellRestriction;

      _prepareGridForCacheStorageIfNoSeiving = prepareGridForCacheStorageIfNoSeiving;

      _level = treeLevel;
      _maxNumberOfPassesToReturn = maxNumberOfPassesToReturn;

      _areaControlSet = areaControlSet;

      _populationControl = populationControl;
      _pdExistenceMap = pDExistenceMap;

      // Create and configure the assignment context which is used to contain
      // a filtered pass and its attendant machine events and target values
      // prior to assignment to the client subgrid.
      _assignmentContext = new FilteredValueAssignmentContext();
    }

    private void ProcessCellPasses()
    {
      bool haveHalfPass = false;
      int passRangeCount = 0;

      while (_cellPassIterator.MayHaveMoreFilterableCellPasses() &&
             _cellPassIterator.GetNextCellPass(ref _currentPass.FilteredPass))
      {
        FiltersValuePopulation.PopulateFilteredValues(
          _siteModel.MachinesTargetValues[_currentPass.FilteredPass.InternalSiteModelMachineIndex],
          _populationControl, ref _currentPass);

        if (_filter.AttributeFilter.FilterPass(ref _currentPass))
        {
          bool takePass;
          if (_filter.AttributeFilter.HasPassCountRangeFilter)
          {

            if (_currentPass.FilteredPass.HalfPass)
            {
              if (!haveHalfPass)
                ++passRangeCount; // increase count for first half pass
              haveHalfPass = !haveHalfPass;
            }
            else
              ++passRangeCount; // increase count for first full pass

            takePass = Range.InRange(passRangeCount, _filter.AttributeFilter.PasscountRangeMin, _filter.AttributeFilter.PasscountRangeMax);
          }
          else
            takePass = true;

          if (takePass)
          {
            if (_filter.AttributeFilter.HasElevationTypeFilter) 
              _assignmentContext.FilteredValue.PassCount = 1;

            if (_filter.AttributeFilter.HasMinElevMappingFilter || (_filter.AttributeFilter.HasElevationTypeFilter &&
                 _filter.AttributeFilter.ElevationType == ElevationType.Lowest))
            {
              if (!_haveFilteredPass || _currentPass.FilteredPass.Height < _tempPass.FilteredPass.Height)
                _tempPass = _currentPass;
              _haveFilteredPass = true;
            }
            else
            {
              if (_filter.AttributeFilter.HasElevationTypeFilter && _filter.AttributeFilter.ElevationType == ElevationType.Highest)
              {
                if (!_haveFilteredPass || _currentPass.FilteredPass.Height > _tempPass.FilteredPass.Height)
                  _tempPass = _currentPass;
                _haveFilteredPass = true;
              }
              else
              {
                _assignmentContext.FilteredValue.FilteredPassData = _currentPass;
                _haveFilteredPass = true;
                _assignmentContext.FilteredValue.PassCount = -1;
                break;
              }
            }
          }
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
    private void AssignRequiredFilteredPassAttributesFromGlobalLatestCells(ref CellPass cellPass, int x, int y)
    {
      switch (_gridDataType)
      {
        case GridDataType.Height:
          cellPass.Height = _globalLatestCells.ReadHeight(x, y);
          break;

        case GridDataType.HeightAndTime:
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
          Debug.Assert(false,
            $"Unsupported grid data type in AssignRequiredFilteredPassAttributesFromGlobalLatestCells: {_gridDataType}");
          break;
      }
    }

    /// <summary>
    /// Retrieves cell values for a subgrid stripe at a time. Currently deprecated in favour of RetriveSubGridCell()
    /// </summary>
    /// <param name="StripeIndex"></param>
    /// <returns></returns>
    public ServerRequestResult RetrieveSubGridStripe(byte StripeIndex)
    {
      int TopMostLayerPassCount = 0;
      int TopMostLayerCompactionHalfPassCount = 0;

      // bool Debug_ExtremeLogSwitchD = VLPDSvcLocations.Debug_ExtremeLogSwitchD;

      // Iterate over the cells in the subgrid applying the filter and assigning the requested information into the subgrid

      //if (Debug_ExtremeLogSwitchD)
      //    Log.LogDebug($"Beginning stripe iteration {StripeIndex} at {CellX}x{CellY}");

      try
      {
        /* TODO Readd when LiftBuildSettings is implemented
         &&
         (!(_GridDataType in [icdtCCV, icdtCCVPercent]) && (LiftBuildSettings.CCVSummaryTypes<>[])) &&
         (!(_GridDataType in [icdtMDP, icdtMDPPercent]) && (LiftBuildSettings.MDPSummaryTypes<>[])) &&
         (!(_GridDataType in [icdtCCA, icdtCCAPercent])) &&
         !(_GridDataType in [icdtCellProfile,
                                    icdtPassCount,
                                    icdtCellPasses,
                                    icdtMachineSpeed,
                                    icdtCCVPercentChange,
                                    icdtMachineSpeedTarget,
                                    icdtCCVPercentChangeIgnoredTopNullValue]); */

        for (byte J = 0; J < SubGridTreeConsts.SubGridTreeDimension; J++)
        {
          // If there is an overriding sieve bitmask (from WMS rendering) then
          // check if this cell is contained in the sieve, otherwise ignore it.
          if (_seiveFilterInUse && !_seiveBitmask.BitSet(StripeIndex, J))
            continue;

          if (_seiveFilterInUse || !_prepareGridForCacheStorageIfNoSeiving)
            if (!_clientGridAsLeaf.ProdDataMap.BitSet(StripeIndex, J)) // This cell does not match the filter mask and should not be processed
              continue;


          if (_gridDataType == GridDataType.CellProfile) // all requests using this data type should filter temperature range using last pass only
            _filter.AttributeFilter.FilterTemperatureByLastPass = true;

          // For pass attributes that are maintained on a historical last pass basis
          // (meaning their values bubble up through cell passes where the values of
          // those attributes are null), check the global latest pass version of
          // those values. If they are null, then no further work needs to be done

          switch (_gridDataType)
          {
            case GridDataType.CCV:
              if (_globalLatestCells.ReadCCV(StripeIndex, J) == CellPassConsts.NullCCV)
                continue;
              break;

            case GridDataType.RMV:
              if (_globalLatestCells.ReadRMV(StripeIndex, J) == CellPassConsts.NullRMV)
                continue;
              break;

            case GridDataType.Frequency:
              if (_globalLatestCells.ReadFrequency(StripeIndex, J) == CellPassConsts.NullFrequency)
                continue;
              break;

            case GridDataType.Amplitude:
              if (_globalLatestCells.ReadAmplitude(StripeIndex, J) == CellPassConsts.NullAmplitude)
                continue;
              break;

            case GridDataType.GPSMode:
              if (_globalLatestCells.ReadGPSMode(StripeIndex, J) == GPSMode.NoGPS)
                continue;
              break;

            case GridDataType.MDP:
              if (_globalLatestCells.ReadMDP(StripeIndex, J) == CellPassConsts.NullMDP)
                continue;
              break;

            case GridDataType.CCA:
              if (_globalLatestCells.ReadCCA(StripeIndex, J) == CellPassConsts.NullCCA)
                continue;
              break;

            case GridDataType.Temperature:
              if (_globalLatestCells.ReadTemperature(StripeIndex, J) == CellPassConsts.NullMaterialTemperatureValue)
                continue;
              break;

            case GridDataType.TemperatureDetail:
              if (_globalLatestCells.ReadTemperature(StripeIndex, J) == CellPassConsts.NullMaterialTemperatureValue)
                continue;
              break;

            case GridDataType.CCVPercentChange:
              if (_globalLatestCells.ReadCCV(StripeIndex, J) == CellPassConsts.NullCCV)
                continue;
              break;

            case GridDataType.CCVPercentChangeIgnoredTopNullValue:
              if (_globalLatestCells.ReadCCV(StripeIndex, J) == CellPassConsts.NullCCV)
                continue;
              break;
          }

          _haveFilteredPass = false;

          if (_useLastPassGrid)
          {           
            // if (Debug_ExtremeLogSwitchD)
            //   Log.LogDebug{$"SI@{StripeIndex}/{J} at {CellX}x{CellY}: Using last pass grid");

            AssignRequiredFilteredPassAttributesFromGlobalLatestCells(ref _assignmentContext.FilteredValue.FilteredPassData.FilteredPass, StripeIndex, J);

            // TODO: Review if line below replaced with line above in Ignite POC is good...
            // AssignmentContext.FilteredValue.FilteredPassData.FilteredPass = _GlobalLatestCells[StripeIndex, J];

            _haveFilteredPass = true;
            _assignmentContext.FilteredValue.PassCount = -1;
          }
          else
          {
            // if (Debug_ExtremeLogSwitchD)
            //    Log.LogDebug{$"SI@{StripeIndex}/{J} at {CellX}x{CellY}: Using profiler");

            _filter.AttributeFilter.InitaliaseFilteringForCell(StripeIndex, J);

            if (_profiler != null) // we don't need this anymore as the logic is implemented in lift builder
            {
              // While we have been given a profiler, we may not need to use it to
              // analyze layers in the cell pass stack. The layer analysis in this
              // operation is intended to locate cell passes belonging to superseded
              // layers, in which case they are not considered for providing the
              // requested value. However, if there is no filter is in effect, then the
              // global latest information for the subgrid may be consulted first
              // to see if the appropriate values came from the last physically collected
              // cell pass in the cell. Note that the tracking of latest values is
              // also true for time, so that the time recorded in the latest values structure
              // also includes that cell pass time.

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
                {
                  // There is no value available for the requested data field in any recorded
                  // cell pass. Thus, there is no cell pass value to assign so abort
                  // consideration of this cell

                  continue;
                }

                bool FilteredValueIsFromLatestCellPass = false;

                if (_clientGrid.WantsLiftProcessingResults())
                {
                  switch (_gridDataType)
                  {
                    case GridDataType.CCV:
                      FilteredValueIsFromLatestCellPass = _globalLatestCells.CCVValuesAreFromLastPass.BitSet(StripeIndex, J);
                      break;
                    case GridDataType.RMV:
                      FilteredValueIsFromLatestCellPass = _globalLatestCells.RMVValuesAreFromLastPass.BitSet(StripeIndex, J);
                      break;
                    case GridDataType.Frequency:
                      FilteredValueIsFromLatestCellPass = _globalLatestCells.FrequencyValuesAreFromLastPass.BitSet(StripeIndex, J);
                      break;
                    case GridDataType.Amplitude:
                      FilteredValueIsFromLatestCellPass = _globalLatestCells.AmplitudeValuesAreFromLastPass.BitSet(StripeIndex, J);
                      break;
                    case GridDataType.Temperature:
                      FilteredValueIsFromLatestCellPass = _globalLatestCells.TemperatureValuesAreFromLastPass.BitSet(StripeIndex, J);
                      break;
                    case GridDataType.GPSMode:
                      FilteredValueIsFromLatestCellPass = _globalLatestCells.GPSModeValuesAreFromLatestCellPass.BitSet(StripeIndex, J);
                      break;
                    case GridDataType.MDP:
                      FilteredValueIsFromLatestCellPass = _globalLatestCells.MDPValuesAreFromLastPass.BitSet(StripeIndex, J);
                      break;
                    case GridDataType.CCA:
                      FilteredValueIsFromLatestCellPass = _globalLatestCells.CCAValuesAreFromLastPass.BitSet(StripeIndex, J);
                      break;
                    case GridDataType.TemperatureDetail:
                      FilteredValueIsFromLatestCellPass = _globalLatestCells.TemperatureValuesAreFromLastPass.BitSet(StripeIndex, J);
                      break;
                    case GridDataType.CCVPercentChange:
                    case GridDataType.CCVPercentChangeIgnoredTopNullValue:
                      break;
                    case GridDataType.MachineSpeedTarget:
                      break;
                    case GridDataType.PassCount:
                      // This cannot be answered here
                      break;
                    default:
                      Debug.Assert(false, "Unimplemented data type for subgrid requiring lift processing results");
                      break;
                  }
                }

                if (FilteredValueIsFromLatestCellPass)
                  _haveFilteredPass = FilteredValueIsFromLatestCellPass;

                if (_haveFilteredPass)
                {
                  FiltersValuePopulation.PopulateFilteredValues(
                    _siteModel.MachinesTargetValues[_currentPass.FilteredPass.InternalSiteModelMachineIndex],
                    _populationControl, ref _assignmentContext.FilteredValue.FilteredPassData);
                }
              }

              if (!_haveFilteredPass)
              {
                _cellPassIterator.SetCellCoordinatesInSubgrid(StripeIndex, J);

                // if (Debug_ExtremeLogSwitchD)
                //  Log.LogDebug{$"SI@{StripeIndex}/{J} at {CellX}x{CellY}: Calling BuildLiftsForCell");

                if (_profiler.CellLiftBuilder.Build(_cellProfile, _clientGrid,
                  _assignmentContext, // Place a filtered value into this assignment context
                  _cellPassIterator,  // Iterate over the cells using this cell pass iterator
                  true)) // Return an individual filtered value
                  // Selection of a filtered value should occur in forwards time order
                {
                  TopMostLayerPassCount = _profiler.CellLiftBuilder.FilteredPassCountOfTopMostLayer;
                  TopMostLayerCompactionHalfPassCount = _profiler.CellLiftBuilder.FilteredHalfCellPassCountOfTopMostLayer;

                  // Filtered value selection is combined with lift analysis in this context via
                  // the provision of the client grid and the assignment context to the
                  // lift analysis engine

                  // if we have a temperature filter to be filtered by last pass
                  if (_filter.AttributeFilter.HasTemperatureRangeFilter && _filter.AttributeFilter.FilterTemperatureByLastPass)
                    {
                      _haveFilteredPass = ( _cellProfile.Passes.FilteredPassData[_cellProfile.Passes.PassCount - 1].FilteredPass.MaterialTemperature != CellPassConsts.NullMaterialTemperatureValue) &&
                             Range.InRange(_cellProfile.Passes.FilteredPassData[_cellProfile.Passes.PassCount - 1].FilteredPass.MaterialTemperature, _filter.AttributeFilter.MaterialTemperatureMin, _filter.AttributeFilter.MaterialTemperatureMax);
                    }
                  else
                    _haveFilteredPass = true;
                }

                // if (Debug_ExtremeLogSwitchD)
                //    Log.LogDebug{$"SI@{StripeIndex}/{J} at {CellX}x{CellY}: Call to BuildLiftsForCell completed");
              }
            }
            else
            {
              _cellPassIterator.SetCellCoordinatesInSubgrid(StripeIndex, J);

              if (_filter.AttributeFilter.HasElevationRangeFilter)
                _cellPassIterator.SetIteratorElevationRange(_filter.AttributeFilter.ElevationRangeBottomElevationForCell,
                  _filter.AttributeFilter.ElevationRangeTopElevationForCell);

              _cellPassIterator.Initialise();

              ProcessCellPasses();

              if (_haveFilteredPass &&
                  (_filter.AttributeFilter.HasMinElevMappingFilter ||
                   (_filter.AttributeFilter.HasElevationTypeFilter &&
                    (_filter.AttributeFilter.ElevationType == ElevationType.Highest ||
                     _filter.AttributeFilter.ElevationType == ElevationType.Lowest))))
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

        return ServerRequestResult.NoError;
      }
      finally
      {
        //if (Debug_ExtremeLogSwitchD)
        //  Log.LogDebug("Completed stripe iteration {StripeIndex} at {CellX}x{CellY}");
      }
    }

    /// <summary>
    /// PruneSubGridRetrievalHere determines if there is no point continuing the
    /// process of retrieving the subgrid due to the impossibility of returning any
    /// valid values for any cells in the subgrid due to a combination of filter
    /// settings and flags set in the subgrid that denote the types of data that
    /// are, or are not, contained in the subgrid.
    /// </summary>
    /// <returns></returns>
    private bool PruneSubGridRetrievalHere()
    {
      // Check the subgrid global attribute presence flags that are tracked for optional
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

    private void SetupForCellPassStackExamination()
    {
      _populationControl.PreparePopulationControl(_gridDataType, /* todo LiftBuildSettings, */ _filter.AttributeFilter, _clientGrid);

      _filter.AttributeFilter.RequestedGridDataType = _gridDataType;

      // Create and configure the segment iterator to be used

      _segmentIterator = new SubGridSegmentIterator(_subGridAsLeaf, _subGridAsLeaf.Directory, _storageProxy);

      if (_filter.AttributeFilter.ReturnEarliestFilteredCellPass ||
          (_filter.AttributeFilter.HasElevationTypeFilter &&
           (_filter.AttributeFilter.ElevationType == ElevationType.First)))
        _segmentIterator.IterationDirection = IterationDirection.Forwards;
      else
        _segmentIterator.IterationDirection = IterationDirection.Backwards;

      _segmentIterator.SubGrid = _subGridAsLeaf;
      _segmentIterator.Directory = _subGridAsLeaf.Directory;

      if (_filter.AttributeFilter.HasMachineFilter)
        _segmentIterator.SetMachineRestriction(_filter.AttributeFilter.MachineIDSet);

      // Create and configure the cell pass iterator to be used

      _cellPassIterator = new SubGridSegmentCellPassIterator_NonStatic(_segmentIterator);
      _cellPassIterator.SetTimeRange(_filter.AttributeFilter.HasTimeFilter,
        _filter.AttributeFilter.StartTime,
        _filter.AttributeFilter.EndTime);
    }

    public ServerRequestResult RetrieveSubGrid(uint CellX, uint CellY,
      // liftBuildSettings          : TICLiftBuildSettings;
      IClientLeafSubGrid clientGrid,
      SubGridTreeBitmapSubGridBits cellOverrideMask,
      IClientHeightLeafSubGrid designElevations)
    {
      ServerRequestResult Result = ServerRequestResult.UnknownError;

      //  SIGLogMessage.PublishNoODS(Nil, Format('In RetrieveSubGrid: Active pass filters = %s, Active cell filters = %s', [PassFilter.ActiveFiltersText, CellFilter.ActiveFiltersText]), slmcDebug);

      // Set up class local state for other methods to access
      _clientGrid = clientGrid;
      _clientGridAsLeaf = clientGrid as ClientLeafSubGrid;
      _gridDataType = clientGrid.GridDataType;

      _canUseGlobalLatestCells &=
        !(_gridDataType == GridDataType.CCV ||
          _gridDataType == GridDataType.CCVPercent) /*&& (LiftBuildSettings.CCVSummaryTypes<>[])*/ &&
        !(_gridDataType == GridDataType.MDP ||
          _gridDataType == GridDataType.MDPPercent) /*&& (LiftBuildSettings.MDPSummaryTypes<>[])*/ &&
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

        _profiler.Configure(_siteModel, _pdExistenceMap, _gridDataType, _filter.AttributeFilter, _filter.SpatialFilter,
            null, null, _populationControl, new CellPassFastEventLookerUpper(_siteModel));

        _cellProfile = new ProfileCell();

        // Create and configure the assignment context which is used to contain
        // a filtered pass and its attendant machine events and target values
        // prior to assignment to the client subgrid.
        _assignmentContext.CellProfile = _cellProfile;
      }

      try
      {
          // Ensure pass type filter is set correctly
          if (_filter.AttributeFilter.HasPassTypeFilter)
            if ((_filter.AttributeFilter.PassTypeSet & (PassTypeSet.Front | PassTypeSet.Rear)) == PassTypeSet.Front)
                _filter.AttributeFilter.PassTypeSet |= PassTypeSet.Rear; // these two types go together as half passes

          // ... unless we if we can use the last pass grid to satisfy the query
          if (_canUseGlobalLatestCells &&
              !_filter.AttributeFilter.HasElevationRangeFilter &&
              !_clientGrid.WantsLiftProcessingResults() &&
              !_filter.AttributeFilter.HasMinElevMappingFilter &&
              !(_filter.AttributeFilter.HasElevationTypeFilter &&
                (_filter.AttributeFilter.ElevationType == ElevationType.Highest ||
                 _filter.AttributeFilter.ElevationType == ElevationType.Lowest)) &&
              !(_gridDataType == GridDataType.PassCount || _gridDataType == GridDataType.Temperature ||
                _gridDataType == GridDataType.CellProfile || _gridDataType == GridDataType.CellPasses ||
                _gridDataType == GridDataType.MachineSpeed))
          {
            _useLastPassGrid = true;
          }

          // First get the subgrid we are interested in
          // SIGLogMessage.PublishNoODS(Nil, Format('Begin LocateSubGridContaining at %dx%d', [CellX, CellY]), slmcDebug); {SKIP}

          // _SubGrid = SiteModel.Grid.LocateSubGridContaining(CellX, CellY, Level);
          _subGrid = SubGridTrees.Server.Utilities.SubGridUtilities.LocateSubGridContaining(_storageProxy, _siteModel.Grid, CellX, CellY, _level, false, false);

          //  SIGLogMessage.PublishNoODS(Nil, Format('End LocateSubGridContaining at %dx%d', [CellX, CellY]), slmcDebug); {SKIP}

          if (_subGrid == null)
          {
            // This should never really happen, but we'll be polite about it
            Log.LogWarning(
              $"Subgrid address (CellX={CellX}, CellY={CellY}) passed to LocateSubGridContaining() from RetrieveSubgrid() did not match an existing subgrid in the data model.' + 'Returning icsrrSubGridNotFound as response with a nil subgrid reference.");
            return ServerRequestResult.SubGridNotFound;
          }

          // Now process the contents of that subgrid into the subgrid to be returned to the client.

          if (!_subGrid.IsLeafSubGrid()) // It's a leaf node
          {
            Log.LogInformation("Requests of node subgrids in the server subgrid are not yet supported");
            return Result;
          }

          if (!(_subGrid is IServerLeafSubGrid))
          {
            Log.LogError($"_SubGrid {_subGrid.Moniker()} is not a server grid leaf node");
            return Result;
          }

          // SIGLogMessage.PublishNoODS(Nil, Format('Getting subgrid leaf at %dx%d', [CellX, CellY]), slmcDebug);

          _subGridAsLeaf = (IServerLeafSubGrid) _subGrid;
          _globalLatestCells = _subGridAsLeaf.Directory.GlobalLatestCells;

          if (PruneSubGridRetrievalHere())
            return ServerRequestResult.NoError;

          //todo: This map calculation seems odd if we are caching subgrids...
          // Determine the bitmask detailing which cells match the cell selection filter
          if (!SubGridFilterMasks.ConstructSubgridCellFilterMask(_subGridAsLeaf, _siteModel, _filter,
            cellOverrideMask, _hasOverrideSpatialCellRestriction, _overrideSpatialCellRestriction,
            _clientGridAsLeaf.ProdDataMap, _clientGridAsLeaf.FilterMap))
          {
            return ServerRequestResult.FailedToComputeDesignFilterPatch;
          }

          // SIGLogMessage.PublishNoODS(Nil, Format('Setup for stripe iteration at %dx%d', [CellX, CellY]), slmcDebug);

          try
          {
            if (!_useLastPassGrid)
              SetupForCellPassStackExamination();

            // Some display types require lift processing to be able to select the
            // appropriate cell pass containing the filtered value required.
            if (_clientGrid.WantsLiftProcessingResults())
            {            
              _segmentIterator.IterationDirection = IterationDirection.Forwards;
              _cellPassIterator.MaxNumberOfPassesToReturn = _maxNumberOfPassesToReturn; //VLPDSvcLocations.VLPDPSNode_MaxCellPassIterationDepth_PassCountDetailAndSummary;
            }

          // TODO Add when cell left build settings supported
          // AssignmentContext.LiftBuildSettings = LiftBuildSettings;

          // Determine if a sieve filter is required for the subgrid where the sieve matches
          // the X and Y pixel world size (used for WMS tile computation)
          _subGrid.CalculateWorldOrigin(out double subGridWorldOriginX, out double subGridWorldOriginY);

            if (_areaControlSet.UseIntegerAlgorithm)
            {
              _seiveFilterInUse = GridRotationUtilities.ComputeSeiveBitmaskInteger(subGridWorldOriginX, subGridWorldOriginY, _subGrid.Moniker(), _areaControlSet, _siteModel.Grid.CellSize, out _seiveBitmask);
            }
            else
            {
              _assignmentContext.InitialiseProbePositions();
              _seiveFilterInUse = GridRotationUtilities.ComputeSeiveBitmaskFloat(subGridWorldOriginX, subGridWorldOriginY, _subGrid.Moniker(), _areaControlSet, _siteModel.Grid.CellSize, _assignmentContext, out _seiveBitmask);
            }

            if (!_seiveFilterInUse)
            {
              // Reset pixel size parameters to indicate no skip stepping is being performed
              _areaControlSet.PixelXWorldSize = 0;
              _areaControlSet.PixelYWorldSize = 0;
            }

            //if (VLPDSvcLocations.Debug_ExtremeLogSwitchC)
            //  Log.LogDebug($"Performing stripe iteration at {CellX}x{CellY}");

            // Iterate over the stripes in the subgrid processing each one in turn.
            for (byte I = 0; I < SubGridTreeConsts.SubGridTreeDimension; I++)
              RetrieveSubGridStripe(I);

            //if VLPDSvcLocations.Debug_ExtremeLogSwitchC then
            //  Log.LogDebug($"Stripe iteration complete at {CellX}x{CellY}");
          }
          finally
          {
            /* TODO - move to owning context of the cell pass looker upper...
          if (CellPassFastEventLookerUpper != null)
          {
                if (VLPDSvcLocations.Debug_LogCellPassLookerUpperFullLookups)
                {
                    InterlockedExchangeAdd64(Debug_TotalCellPassLookerUpperFullLookups, CellPassFastEventLookerUpper.NumFullEventLookups);
                    SIGLogMessage.PublishNoODS(Nil, Format('Cell pass looker-upper invoked %d full event lookups, total = %d', [CellPassFastEventLookerUpper.NumFullEventLookups, Debug_TotalCellPassLookerUpperFullLookups]), slmcDebug);
                }
          }
            */
        }

          Result = ServerRequestResult.NoError;
      }
      catch (Exception e)
      {
        Log.LogError($"Exception {e} occured in RetrieveSubGrid");
        throw;
      }

//  Log.LogInformation("Exiting RetrieveSubGrid");

      return Result;
    }
  }
}
