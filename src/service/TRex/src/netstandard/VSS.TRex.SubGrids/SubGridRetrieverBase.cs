using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Cells;
using VSS.TRex.Common;
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
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Types.Types;

namespace VSS.TRex.SubGrids
{
  public abstract class SubGridRetrieverBase
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridRetrieverBase>();

    // Local state populated by the retriever constructor
    protected bool _canUseGlobalLatestCells;
    protected readonly ICombinedFilter _filter;
    protected readonly ICellPassAttributeFilterProcessingAnnex _filterAnnex;

    protected readonly ISiteModel _siteModel;
    protected readonly IStorageProxy _storageProxy;

    protected readonly GridDataType _gridDataType;
    protected ISubGridCellLatestPassDataWrapper _globalLatestCells;

    protected readonly IFilteredValuePopulationControl _populationControl;

    protected readonly AreaControlSet _areaControlSet;

    protected readonly bool _hasOverrideSpatialCellRestriction;
    protected readonly BoundingIntegerExtent2D _overrideSpatialCellRestriction;
    protected readonly bool _prepareGridForCacheStorageIfNoSieving;
    protected readonly int _maxNumberOfPassesToReturn;

    // Local state populated for the purpose of access from various local methods
    protected IClientLeafSubGrid _clientGrid;
    protected ClientLeafSubGrid _clientGridAsLeaf;

    protected IProfilerBuilder<ProfileCell> _profiler;

    protected readonly FilteredValueAssignmentContext _assignmentContext;

    protected readonly ISubGridTreeBitMask _pdExistenceMap;

    protected readonly IOverrideParameters _overrides;
    protected readonly ILiftParameters _liftParams;

    protected readonly SubGridTreeBitmapSubGridBits _aggregatedCellScanMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

    protected bool _useLastPassGrid; // Assume we can't use last pass data
    protected ISubGrid _subGrid;
    protected IServerLeafSubGrid _subGridAsLeaf;

    protected bool _sieveFilterInUse;
    protected SubGridTreeBitmapSubGridBits _sieveBitmask;

    protected ProfileCell _cellProfile;

    protected ISubGridSegmentIterator _segmentIterator;
    protected SubGridSegmentCellPassIterator_NonStatic _cellPassIterator;

    protected SubGridRetrieverBase(ISiteModel siteModel, 
      GridDataType gridDataType,
      ICombinedFilter filter,
      ICellPassAttributeFilterProcessingAnnex filterAnnex,
      bool hasOverrideSpatialCellRestriction,
      BoundingIntegerExtent2D overrideSpatialCellRestriction,
      bool prepareGridForCacheStorageIfNoSieving,
      int maxNumberOfPassesToReturn,
      IStorageProxy storageProxy,
      AreaControlSet areaControlSet,
      IFilteredValuePopulationControl populationControl,
      ISubGridTreeBitMask pDExistenceMap,
      IOverrideParameters overrides,
      ILiftParameters liftParams)
    {
      _segmentIterator = null;
      _cellPassIterator = null;

      _siteModel = siteModel;
      _gridDataType = gridDataType;
      _filter = filter;
      _filterAnnex = filterAnnex;
      _hasOverrideSpatialCellRestriction = hasOverrideSpatialCellRestriction;
      _overrideSpatialCellRestriction = overrideSpatialCellRestriction;
      _prepareGridForCacheStorageIfNoSieving = prepareGridForCacheStorageIfNoSieving;
      _maxNumberOfPassesToReturn = maxNumberOfPassesToReturn;
      _storageProxy = storageProxy;
      _populationControl = populationControl;
      _areaControlSet = areaControlSet;
      _pdExistenceMap = pDExistenceMap;
      _overrides = overrides;
      _liftParams = liftParams;

      // Create and configure the assignment context which is used to contain a filtered pass and
      // its attendant machine events and target values prior to assignment to the client sub grid.
      _assignmentContext = new FilteredValueAssignmentContext { Overrides = overrides, LiftParams = liftParams };

      _canUseGlobalLatestCells = _filter.AttributeFilter.LastRecordedCellPassSatisfiesFilter;
    }

    public abstract void RetrieveSubGridStripe(byte stripeIndex);

    /// <summary>
    /// Performs extraction of specific attributes from a GlobalLatestCells structure depending on the type of
    /// grid being retrieved
    /// </summary>
    /// <param name="cellPass"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    protected void AssignRequiredFilteredPassAttributesFromGlobalLatestCells(ref CellPass cellPass, int x, int y)
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
    /// <param name="stripeIndex"></param>
    /// <param name="j"></param>
    /// <returns></returns>
    protected bool IsFilteredValueFromLatestCellPass(int stripeIndex, int j)
    {
      switch (_gridDataType)
      {
        case GridDataType.CCV: return _globalLatestCells.CCVValuesAreFromLastPass.BitSet(stripeIndex, j);
        case GridDataType.RMV: return _globalLatestCells.RMVValuesAreFromLastPass.BitSet(stripeIndex, j);
        case GridDataType.Frequency: return _globalLatestCells.FrequencyValuesAreFromLastPass.BitSet(stripeIndex, j);
        case GridDataType.Amplitude: return _globalLatestCells.AmplitudeValuesAreFromLastPass.BitSet(stripeIndex, j);
        case GridDataType.Temperature: return _globalLatestCells.TemperatureValuesAreFromLastPass.BitSet(stripeIndex, j);
        case GridDataType.GPSMode: return _globalLatestCells.GPSModeValuesAreFromLatestCellPass.BitSet(stripeIndex, j);
        case GridDataType.MDP: return _globalLatestCells.MDPValuesAreFromLastPass.BitSet(stripeIndex, j);
        case GridDataType.CCA: return _globalLatestCells.CCAValuesAreFromLastPass.BitSet(stripeIndex, j);
        case GridDataType.TemperatureDetail: return _globalLatestCells.TemperatureValuesAreFromLastPass.BitSet(stripeIndex, j);
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
    protected bool LatestCellPassAttributeIsNull(int stripeIndex, int j)
    {
      switch (_gridDataType)
      {
        case GridDataType.CCV: return _globalLatestCells.ReadCCV(stripeIndex, j) == CellPassConsts.NullCCV;
        case GridDataType.RMV: return _globalLatestCells.ReadRMV(stripeIndex, j) == CellPassConsts.NullRMV;
        case GridDataType.Frequency: return _globalLatestCells.ReadFrequency(stripeIndex, j) == CellPassConsts.NullFrequency;
        case GridDataType.Amplitude: return _globalLatestCells.ReadAmplitude(stripeIndex, j) == CellPassConsts.NullAmplitude;
        case GridDataType.GPSMode: return _globalLatestCells.ReadGPSMode(stripeIndex, j) == GPSMode.NoGPS;
        case GridDataType.MDP: return _globalLatestCells.ReadMDP(stripeIndex, j) == CellPassConsts.NullMDP;
        case GridDataType.CCA: return _globalLatestCells.ReadCCA(stripeIndex, j) == CellPassConsts.NullCCA;
        case GridDataType.Temperature: return _globalLatestCells.ReadTemperature(stripeIndex, j) == CellPassConsts.NullMaterialTemperatureValue;
        case GridDataType.TemperatureDetail: return _globalLatestCells.ReadTemperature(stripeIndex, j) == CellPassConsts.NullMaterialTemperatureValue;
        case GridDataType.CCVPercentChange: return _globalLatestCells.ReadCCV(stripeIndex, j) == CellPassConsts.NullCCV;
        case GridDataType.CCVPercentChangeIgnoredTopNullValue: return _globalLatestCells.ReadCCV(stripeIndex, j) == CellPassConsts.NullCCV;
      }

      return false;
    }

    /// <summary>
    /// PruneSubGridRetrievalHere determines if there is no point continuing the
    /// process of retrieving the sub grid due to the impossibility of returning any
    /// valid values for any cells in the sub grid due to a combination of filter
    /// settings and flags set in the sub grid that denote the types of data that
    /// are, or are not, contained in the sub grid.
    /// </summary>
    /// <returns></returns>
    protected bool PruneSubGridRetrievalHere()
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

    protected virtual void SetupForCellPassStackExamination()
    {
      if (!_commonCellPassStackExaminationDone)
      {
        _populationControl.PreparePopulationControl(_gridDataType, _liftParams, _filter.AttributeFilter, _clientGrid.EventPopulationFlags);

        _filter.AttributeFilter.RequestedGridDataType = _gridDataType;

        // Create and configure the segment iterator to be used
        _segmentIterator = new SubGridSegmentIterator(_subGridAsLeaf, _subGridAsLeaf.Directory, _storageProxy);

        if (_filter.AttributeFilter.HasMachineFilter)
          _segmentIterator.SetMachineRestriction(_filter.AttributeFilter.GetMachineIDsSet());

        // Create and configure the cell pass iterator to be used

        _cellPassIterator = new SubGridSegmentCellPassIterator_NonStatic(_segmentIterator, _maxNumberOfPassesToReturn);
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

    /// <summary>
    /// Orchestrates the mainline work of analyzing cell and cell pass information to create a client sub grid (heights, CMV, MDP etc)
    /// based on filter and other information established in the class
    /// </summary>
    /// <param name="clientGrid"></param>
    /// <param name="cellOverrideMask"></param>
    /// <returns></returns>
    public virtual ServerRequestResult RetrieveSubGrid(IClientLeafSubGrid clientGrid,
      SubGridTreeBitmapSubGridBits cellOverrideMask)
    {
      if (!Utilities.DerivedGridDataTypesAreCompatible(_gridDataType, clientGrid.GridDataType))
      {
        throw new TRexSubGridProcessingException($"Grid data type of client leaf sub grid [{clientGrid.GridDataType}] is not compatible with the grid data type of retriever [{_gridDataType}]");
      }

      var result = ServerRequestResult.UnknownError;

      //  SIGLogMessage.PublishNoODS(Nil, Format('In RetrieveSubGrid: Active pass filters = %s, Active cell filters = %s', [PassFilter.ActiveFiltersText, CellFilter.ActiveFiltersText]));

      // Set up class local state for other methods to access
      _clientGrid = clientGrid;
      _clientGridAsLeaf = clientGrid as ClientLeafSubGrid;

      _canUseGlobalLatestCells &=
        !(_gridDataType == GridDataType.CCV ||
          _gridDataType == GridDataType.CCVPercent) &&
        _liftParams.CCVSummaryTypes != CCVSummaryTypes.None &&
        !(_gridDataType == GridDataType.MDP ||
          _gridDataType == GridDataType.MDPPercent) &&
        _liftParams.MDPSummaryTypes != MDPSummaryTypes.None &&
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

        //TODO: should referenceDesignWrapper be null here or be passed through from args?
        _profiler.Configure(ProfileStyle.CellPasses, _siteModel, _pdExistenceMap, _gridDataType, new FilterSet(_filter),
          null, _populationControl, new CellPassFastEventLookerUpper(_siteModel), VolumeComputationType.None, _overrides, _liftParams);

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
          return result;
        }

        if (!(_subGrid is IServerLeafSubGrid))
        {
          Log.LogError($"_SubGrid {_subGrid.Moniker()} is not a server grid leaf node");
          return result;
        }

        // SIGLogMessage.PublishNoODS(Nil, Format('Getting sub grid leaf at %dx%d', [clientGrid.OriginX, clientGrid.OriginY]));

        _subGridAsLeaf = (IServerLeafSubGrid)_subGrid;
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
          _cellPassIterator.MaxNumberOfPassesToReturn = _maxNumberOfPassesToReturn;
        }

        // Determine if a sieve filter is required for the sub grid where the sieve matches
        // the X and Y pixel world size (used for WMS tile computation)
        _subGrid.CalculateWorldOrigin(out var subGridWorldOriginX, out var subGridWorldOriginY);

        _sieveFilterInUse = _areaControlSet.UseIntegerAlgorithm
          ? GridRotationUtilities.ComputeSieveBitmaskInteger(subGridWorldOriginX, subGridWorldOriginY, _subGrid.Moniker(), _areaControlSet, _siteModel.CellSize, out _sieveBitmask)
          : GridRotationUtilities.ComputeSieveBitmaskFloat(subGridWorldOriginX, subGridWorldOriginY, _areaControlSet, _siteModel.CellSize, _assignmentContext, out _sieveBitmask);

        if (!_sieveFilterInUse)
        {
          // Reset pixel size parameters to indicate no skip stepping is being performed
          _areaControlSet.PixelXWorldSize = 0;
          _areaControlSet.PixelYWorldSize = 0;
        }

        //if (Debug_ExtremeLogSwitchC) Log.LogDebug($"Performing stripe iteration at {clientGrid.OriginX}x{clientGrid.OriginY}");

        // Before iterating over stripes of this sub grid, compute a scan map detailing to the best of our current
        // knowledge, which cells need to be visited so that only cells the filter wants and which are actually
        // present in the data set are requested
        _aggregatedCellScanMap.SetAndOf(_clientGridAsLeaf.FilterMap, _globalLatestCells.PassDataExistenceMap);
        if (_sieveFilterInUse)
          _aggregatedCellScanMap.AndWith(_sieveBitmask); // ... and which are required by any sieve mask
        if (_sieveFilterInUse || !_prepareGridForCacheStorageIfNoSieving)
          _aggregatedCellScanMap.AndWith(_clientGridAsLeaf.ProdDataMap); // ... and which are in the required production data map

        // Iterate over the stripes in the sub grid processing each one in turn.
        for (byte i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
        {
          RetrieveSubGridStripe(i);
        }

        //if Debug_ExtremeLogSwitchC then Log.LogDebug($"Stripe iteration complete at {clientGrid.OriginX}x{clientGrid.OriginY}");

        result = ServerRequestResult.NoError;
      }
      catch (Exception e)
      {
        Log.LogError(e, $"Exception occured in {nameof(RetrieveSubGrid)}");
        throw;
      }

      return result;
    }
  }
}
