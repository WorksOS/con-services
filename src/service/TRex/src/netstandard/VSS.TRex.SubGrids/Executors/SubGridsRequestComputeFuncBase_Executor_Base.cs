using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Utilities;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using SubGridUtilities = VSS.TRex.SubGridTrees.Core.Utilities.SubGridUtilities;
using VSS.TRex.Types;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.GridFabric.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.GridFabric.Arguments;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGrids.Responses;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.Serilog.Extensions;

namespace VSS.TRex.SubGrids.Executors
{
  /// <summary>
  /// The closure/function that implements sub grid request processing on compute nodes
  /// </summary>
  public abstract class SubGridsRequestComputeFuncBase_Executor_Base<TSubGridsRequestArgument, TSubGridRequestsResponse>
    where TSubGridsRequestArgument : SubGridsRequestArgument
    where TSubGridRequestsResponse : SubGridRequestsResponse, new()
  {
    public const string SUB_GRIDS_REQUEST_ADDRESS_BUCKET_SIZE = "SUB_GRIDS_REQUEST_ADDRESS_BUCKET_SIZE";

    private readonly int _addressBucketSize = DIContext.Obtain<IConfigurationStore>().GetValueInt(SUB_GRIDS_REQUEST_ADDRESS_BUCKET_SIZE, 25);

    private readonly IRequestorUtilities _requestorUtilities = DIContext.Obtain<IRequestorUtilities>();

    // ReSharper disable once StaticMemberInGenericType
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SubGridsRequestComputeFuncBase_Executor_Base<TSubGridsRequestArgument, TSubGridRequestsResponse>>();

    /// <summary>
    /// Denotes the request 'style' in use. 
    /// </summary>
    public SubGridsRequestComputeStyle SubGridsRequestComputeStyle { get; set; } = SubGridsRequestComputeStyle.Normal;

    /// <summary>
    /// Local reference to the client sub grid factory
    /// </summary>
    private IClientLeafSubGridFactory _clientLeafSubGridFactory;

    private IClientLeafSubGridFactory ClientLeafSubGridFactory => _clientLeafSubGridFactory ??= DIContext.Obtain<IClientLeafSubGridFactory>();

    /// <summary>
    /// Mask is the internal sub grid bit mask tree created from the serialized mask contained in the
    /// ProdDataMaskBytes member of the argument. It is only used during processing of the request.
    /// It is marked as non serialized so the Ignite GridCompute Broadcast method does not attempt
    /// to serialize this member as an aspect of the compute func.
    /// </summary>
    private ISubGridTreeBitMask ProdDataMask;

    /// <summary>
    /// Mask is the internal sub grid bit mask tree created from the serialized mask contained in the
    /// SurveyedSurfaceOnlyMaskBytes member of the argument. It is only used during processing of the request.
    /// It is marked as non serialized so the Ignite GridCompute Broadcast method does not attempt
    /// to serialize this member as an aspect of the compute func.
    /// </summary>
    private ISubGridTreeBitMask SurveyedSurfaceOnlyMask;

    protected TSubGridsRequestArgument localArg;

    private ISiteModel siteModel;

    private ISiteModels siteModels;

    /// <summary>
    /// The list of address being constructed prior to submission to the processing engine
    /// </summary>
    private SubGridCellAddress[] addresses;

    /// <summary>
    /// The number of sub grids currently present in the process pending list
    /// </summary>
    private int listCount;

    /// <summary>
    /// The Design to be used for querying elevation information from in the process of calculating cut-fill values
    /// together with its offset for a reference surface
    /// </summary>
    private IDesignWrapper ReferenceDesignWrapper;

    /// <summary>
    /// Any overriding targets to be used instead of machine targets
    /// </summary>
    private IOverrideParameters Overrides;

    /// <summary>
    /// Parameters for lift analysis
    /// </summary>
    private ILiftParameters LiftParams;

    /// <summary>
    /// Cleans an array of client leaf sub grids by repatriating them to the client leaf sub grid factory
    /// </summary>
    private void CleanSubGridResultArray((ServerRequestResult requestResult, IClientLeafSubGrid clientGrid)[] subGridResultArray)
    {
      if (subGridResultArray == null) return;

      for (var i = 0; i < subGridResultArray.Length; i++)
        ClientLeafSubGridFactory.ReturnClientSubGrid(ref subGridResultArray[i].clientGrid);
    }

    /// <summary>
    /// Performs conversions from the internal sub grid client leaf type to the requested client leaf type
    /// </summary>
    /// <param name="requestGridDataType"></param>
    /// <param name="subGridResultArray"></param>
    private void ConvertIntermediarySubGridsToResult(GridDataType requestGridDataType,
      ref (ServerRequestResult requestResult, IClientLeafSubGrid clientGrid)[] subGridResultArray)
    {
      (ServerRequestResult requestResult, IClientLeafSubGrid clientGrid)[] newClientGrids = null;

      try
      {
        // If performing simple volume calculations, there may be an intermediary filter in play. If this is
        // the case then the first two sub grid results will be HeightAndTime elevation sub grids and will
        // need to be merged into a single height and time sub grid before any secondary conversion of intermediary
        // results in the logic below.

        if (SubGridsRequestComputeStyle == SubGridsRequestComputeStyle.SimpleVolumeThreeWayCoalescing && subGridResultArray.Length == 3)
        {
          // Three filters in play - check the two results we care about here
          var clientGrid1 = subGridResultArray[0].clientGrid;
          var clientGrid2 = subGridResultArray[1].clientGrid;

          if ((clientGrid1.GridDataType == GridDataType.HeightAndTime || clientGrid1.GridDataType == GridDataType.Height) &&
              (clientGrid2.GridDataType == GridDataType.HeightAndTime || clientGrid2.GridDataType == GridDataType.Height))
          {
            var heights1 = clientGrid1.GridDataType == GridDataType.HeightAndTime ? ((ClientHeightAndTimeLeafSubGrid)clientGrid1).Cells : ((ClientHeightLeafSubGrid)clientGrid1).Cells;
            var heights2 = clientGrid1.GridDataType == GridDataType.HeightAndTime ? ((ClientHeightAndTimeLeafSubGrid)clientGrid2).Cells : ((ClientHeightLeafSubGrid)clientGrid2).Cells;

            // Merge the first two results then swap the second and third items so later processing
            // uses the correct two result, and the the third is correctly recycled
            // Subgrid1 is 'latest @ first filter', sub grid 2 is earliest @ second filter
            SubGridUtilities.SubGridDimensionalIterator((i, j) =>
            {
              // Check if there is a non null candidate in the earlier @ second filter
              // ReSharper disable once CompareOfFloatsByEqualityOperator
              if (heights1[i, j] == Consts.NullHeight &&
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                  heights2[i, j] != Consts.NullHeight)
              {
                heights1[i, j] = heights2[i, j];
              }
            });

            // Return the intermediary result to the factory - it is no longer needed
            ClientLeafSubGridFactory.ReturnClientSubGrid(ref subGridResultArray[1].clientGrid);

            // Promote the 'top' sub grid into the correct place (where the intermediary grid was located)
            MinMax.Swap(ref subGridResultArray[1], ref subGridResultArray[2]);

            // Create the newClientGrids array here to exclude the null result at the end of the subGridResults array
            newClientGrids = new (ServerRequestResult requestResult, IClientLeafSubGrid clientGrid)[2];
          }
        }

        newClientGrids ??= new (ServerRequestResult requestResult, IClientLeafSubGrid clientGrid)[subGridResultArray.Length];

        if (subGridResultArray.Length == 0)
          return;

        try
        {
          for (var I = 0; I < subGridResultArray.Length; I++)
          {
            if (subGridResultArray[I].clientGrid == null)
              continue;

            var subGridResult = subGridResultArray[I];

            if (subGridResult.clientGrid.GridDataType != requestGridDataType)
            {
              switch (requestGridDataType)
              {
                case GridDataType.SimpleVolumeOverlay:
                  throw new TRexSubGridProcessingException("SimpleVolumeOverlay not implemented");

                case GridDataType.Height:
                case GridDataType.CutFill:
                  //Height requested but up to here if we have used HeightAndTime sub grids for using surveyed surfaces so now time to return Height sub grids
                  if (subGridResult.clientGrid.GridDataType == GridDataType.HeightAndTime)
                  {
                    newClientGrids[I] = (subGridResult.requestResult,
                      ClientLeafSubGridFactory.GetSubGridEx(GridDataType.Height, siteModel.CellSize, siteModel.Grid.NumLevels,
                        subGridResult.clientGrid.OriginX, subGridResult.clientGrid.OriginY));

                    // Debug.Assert(NewClientGrids[I] is ClientHeightLeafSubGrid, $"NewClientGrids[I] is ClientHeightLeafSubGrid failed, is actually {NewClientGrids[I].GetType().Name}/{NewClientGrids[I]}");
                    // if (!(SubGridResultArray[I] is ClientHeightAndTimeLeafSubGrid))
                    //    Debug.Assert(SubGridResultArray[I] is ClientHeightAndTimeLeafSubGrid, $"SubGridResultArray[I] is ClientHeightAndTimeLeafSubGrid failed, is actually {SubGridResultArray[I].GetType().Name}/{SubGridResultArray[I]}");

                    (newClientGrids[I].clientGrid as ClientHeightLeafSubGrid)?.Assign(subGridResult.clientGrid as ClientHeightAndTimeLeafSubGrid);
                  }
                  else
                  {
                    newClientGrids[I] = subGridResult;
                    subGridResultArray[I].clientGrid = null;
                  }

                  break;
              }
            }
            else
            {
              newClientGrids[I] = subGridResult;
              subGridResultArray[I].clientGrid = null;
            }
          }
        }
        finally
        {
          CleanSubGridResultArray(subGridResultArray);
        }

        subGridResultArray = newClientGrids;
      }
      catch
      {
        CleanSubGridResultArray(newClientGrids);
        throw;
      }
    }

    /// <summary>
    /// Take the supplied argument to the compute func and perform any necessary unpacking of the
    /// contents of it into a form ready to use. Also make a location reference to the arg parameter
    /// to allow other methods to access it as local state.
    /// </summary>
    public virtual void UnpackArgument(TSubGridsRequestArgument arg)
    {
      localArg = arg;

      siteModels = DIContext.Obtain<ISiteModels>();
      siteModel = siteModels.GetSiteModel(localArg.ProjectID);

      // Unpack the mask from the argument.
      if (arg.ProdDataMaskBytes != null)
      {
        ProdDataMask = new SubGridTreeSubGridExistenceBitMask();
        ProdDataMask.FromBytes(arg.ProdDataMaskBytes);
      }

      if (arg.SurveyedSurfaceOnlyMaskBytes != null)
      {
        SurveyedSurfaceOnlyMask = new SubGridTreeSubGridExistenceBitMask();
        SurveyedSurfaceOnlyMask.FromBytes(arg.SurveyedSurfaceOnlyMaskBytes);
      }

      // Set up any required cut fill design
      if ((arg.ReferenceDesign?.DesignID ?? Guid.Empty) != Guid.Empty)
      {
        ReferenceDesignWrapper = new DesignWrapper(arg.ReferenceDesign, siteModel.Designs.Locate(arg.ReferenceDesign.DesignID));
      }

      Overrides = arg.Overrides;
      LiftParams = arg.LiftParams;

      SubGridsRequestComputeStyle = arg.SubGridsRequestComputeStyle;
    }

    /// <summary>
    /// Take a sub grid address and a set of requesters and request the required client sub grid depending on GridDataType
    /// </summary>
    private (ServerRequestResult requestResult, IClientLeafSubGrid clientGrid)[] PerformSubGridRequest(ISubGridRequestor[] requesters, SubGridCellAddress address)
    {
      _log.LogDebug("In: PerformSubGridRequest");

      //################################################
      // Special case for DesignHeight sub grid requests
      // Todo: This should be refactored out into another method
      //################################################

      if (localArg.GridDataType == GridDataType.DesignHeight)
      {
        _log.LogDebug("In: Special case for DesignHeight sub grid requests");

        try
        {
          var designHeightResult = new (ServerRequestResult requestResult, IClientLeafSubGrid clientGrid)[] {(ServerRequestResult.UnknownError, null)};
          var getGetDesignHeights = ReferenceDesignWrapper.Design.GetDesignHeightsViaLocalCompute(siteModel, ReferenceDesignWrapper.Offset, address, siteModel.CellSize);

          designHeightResult[0].clientGrid = getGetDesignHeights.designHeights;
          if (getGetDesignHeights.errorCode == DesignProfilerRequestResult.OK || getGetDesignHeights.errorCode == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
          {
            designHeightResult[0].requestResult = ServerRequestResult.NoError;
            return designHeightResult;
          }

          _log.LogError($"Design profiler sub grid elevation request for {address} failed with error {getGetDesignHeights.errorCode}");

          designHeightResult[0].requestResult = ServerRequestResult.FailedToComputeDesignElevationPatch;

          return designHeightResult;
        }
        finally
        {
          _log.LogDebug("Out: Special case for DesignHeight sub grid requests");
        }
      }

      // ##################################
      // General case for sub grid requests
      // ##################################

      var result = new (ServerRequestResult requestResult, IClientLeafSubGrid clientGrid)[requesters.Length];

      var requestCount = 0;
      // Reach into the sub grid request layer and retrieve an appropriate sub grid
      foreach (var requester in requesters)
      {
        var requestSubGridInternalResult = requester.RequestSubGridInternal(address, address.ProdDataRequested, address.SurveyedSurfaceDataRequested);
        var subGridResult = (requestSubGridInternalResult.requestResult, requestSubGridInternalResult.clientGrid);

        if (subGridResult.requestResult != ServerRequestResult.NoError)
          _log.LogError($"Request for sub grid {address} request failed with code(s) {string.Join(",", result.Select(x => $"{x.requestResult}"))}");

        result[requestCount++] = subGridResult;
      }

      // ##########################################
      // Special case handling for CutFill requests
      // ##########################################

      // Some request types require additional processing of the sub grid results prior to repatriating the answers back to the caller
      // Convert the computed intermediary grids into the client grid form expected by the caller
      if (result[0].clientGrid?.GridDataType != localArg.GridDataType)
      {
        ConvertIntermediarySubGridsToResult(localArg.GridDataType, ref result);
      }

      // If the requested data is cut fill derived from elevation data previously calculated, 
      // then perform the conversion here
      if (localArg.GridDataType == GridDataType.CutFill)
      {
        _log.LogDebug("In: Special case for cut/fill sub grid requests");

        try
        {
          if (result.Length == 1)
          {
            // The cut fill is defined between one production data derived height sub grid and a
            // height sub grid to be calculated from a designated design
            var computeCutFillSubGridResult = CutFillUtilities.ComputeCutFillSubGrid(
              siteModel,
              result[0].clientGrid, // base
              ReferenceDesignWrapper // 'top'
            );

            if (!computeCutFillSubGridResult.executionResult)
            {
              ClientLeafSubGridFactory.ReturnClientSubGrid(ref result[0].clientGrid);
              result[0].requestResult = ServerRequestResult.FailedToComputeDesignElevationPatch;
            }
          }

          // If the requested data is cut fill derived from two elevation data sub grids previously calculated, 
          // then perform the conversion here
          if (result.Length == 2)
          {
            // The cut fill is defined between two production data derived height sub grids
            // depending on volume type work out height difference
            CutFillUtilities.ComputeCutFillSubGrid((IClientHeightLeafSubGrid) result[0].clientGrid, // 'base'
              (IClientHeightLeafSubGrid) result[1].clientGrid); // 'top'

            // ComputeCutFillSubGrid has placed the result of the cut fill computation into clientGrids[0],
            // so clientGrids[1] can be discarded
            ClientLeafSubGridFactory.ReturnClientSubGrid(ref result[1].clientGrid);

            result = new[] {(ServerRequestResult.NoError, result[0].clientGrid)};
          }
        }
        finally
        {
          _log.LogDebug("Out: Special case for cut/fill sub grid requests");
        }
      }

      _log.LogDebug("Out: PerformSubGridRequest");

      return result;
    }

    /// <summary>
    /// Method responsible for accepting sub grids from the query engine and processing them in the next step of
    /// the request
    /// </summary>
    protected abstract void ProcessSubGridRequestResult(IClientLeafSubGrid[][] results, int resultCount);

    /// <summary>
    /// Transforms the internal aggregation state into the desired response for the request
    /// </summary>
    protected abstract TSubGridRequestsResponse AcquireComputationResult();

    /// <summary>
    /// Performs any necessary setup and configuration of Ignite infrastructure to support the processing of this request
    /// </summary>
    protected abstract bool EstablishRequiredIgniteContext(out SubGridRequestsResponseResult contextEstablishmentResponse);

    /// <summary>
    /// Process a subset of the full set of sub grids in the request
    /// </summary>
    private void PerformSubGridRequestList(SubGridCellAddress[] addressList)
    {
      if (_log.IsDebugEnabled())
      {
        _log.LogDebug($"Starting processing of {addressList.Length} sub grids");
      }

      try
      {
        if (addressList.Length == 0)
          return;

        // Construct the set of requester objects to be used for the filters present in the request
        var requestors = _requestorUtilities.ConstructRequestors(localArg,
          siteModel, localArg.Overrides, localArg.LiftParams, _requestorIntermediaries, localArg.AreaControlSet, ProdDataMask);

        //Log.LogInformation("Sending {0} sub grids to caller for processing", count);
        //Log.LogInformation($"Requester list contains {Requestors.Length} items");

        var clientGridResults = new List<(ServerRequestResult requestResult, IClientLeafSubGrid clientGrid)[]>(addressList.Length);

        // Execute a client grid request for each requester and create an array of the results
        foreach (var address in addressList)
        {
          clientGridResults.Add(PerformSubGridRequest(requestors, address));
        }

        var clientGrids = clientGridResults.Select(c => c.Select(x => x.requestResult == ServerRequestResult.NoError ? x.clientGrid : null).ToArray()).ToArray();

        try
        {
          _log.LogDebug("About to process sub grid request result");

          ProcessSubGridRequestResult(clientGrids, addressList.Length);
        }
        finally
        {
          // Return the client grid to the factory for recycling now its role is complete here...
          ClientLeafSubGridFactory.ReturnClientSubGrids(clientGrids, addressList.Length);
        }
      }
      finally
      {
        if (_log.IsDebugEnabled())
        {
          _log.LogDebug($"Completed processing {addressList.Length} sub grids");
        }
      }
    }

    private readonly List<SubGridCellAddress[]> _taskAddresses = new List<SubGridCellAddress[]>();

    /// <summary>
    /// Processes a bucket of sub grids by creating a task for it and adding it to the tasks list for the request
    /// </summary>
    private void ProcessSubGridAddressGroup(SubGridCellAddress[] addressList, int addressCount)
    {
      var addressListCopy = new SubGridCellAddress[addressCount];
      Array.Copy(addressList, addressListCopy, addressCount);

      _taskAddresses.Add(addressListCopy);
    }

    /// <summary>
    /// Adds a new address to the list of addresses being built and triggers processing of the list if it hits the critical size
    /// </summary>
    private void AddSubGridToAddressList(SubGridCellAddress address)
    {
      addresses[listCount++] = address;

      if (listCount == _addressBucketSize)
      {
        // Process the sub grids...
        ProcessSubGridAddressGroup(addresses, listCount);
        listCount = 0;
      }
    }

    /// <summary>
    /// The collection of requestor intermediaries that are derived from to create requestor delegates
    /// </summary>
    private (GridDataType GridDataType,
      ICombinedFilter Filter, 
      ISurveyedSurfaces FilteredSurveyedSurfaces, 
      ISurfaceElevationPatchRequest surfaceElevationPatchRequest,
      ITRexSpatialMemoryCacheContext[] CacheContexts)[] _requestorIntermediaries;

    /// <summary>
    /// Process the set of sub grids in the request that have partition mappings that match their affinity with this node
    /// </summary>
    private TSubGridRequestsResponse PerformSubGridRequests()
    {
      // Scan through all the bitmap leaf sub grids, and for each, scan through all the sub grids as 
      // noted with the 'set' bits in the bitmask, processing only those that matter for this server

      _log.LogInformation("Scanning sub grids in request");

      addresses = new SubGridCellAddress[_addressBucketSize];

      // Obtain the primary partition map to allow this request to determine the elements it needs to process
      var primaryPartitionMap = ImmutableSpatialAffinityPartitionMap.Instance().PrimaryPartitions();

      // Request production data only, or hybrid production data and surveyed surface data sub grids
      ProdDataMask?.ScanAllSetBitsAsSubGridAddresses(address =>
      {
        // Is this sub grid the responsibility of this server?
        if (!primaryPartitionMap[address.ToSpatialPartitionDescriptor()])
          return;

        // Decorate the address with the production data and surveyed surface flags
        address.ProdDataRequested = true;
        address.SurveyedSurfaceDataRequested = localArg.IncludeSurveyedSurfaceInformation;

        AddSubGridToAddressList(address); // Assign the address into the group to be processed
      });

      if (localArg.IncludeSurveyedSurfaceInformation)
      {
        // Request surveyed surface only sub grids
        SurveyedSurfaceOnlyMask?.ScanAllSetBitsAsSubGridAddresses(address =>
        {
          // Is this sub grid the responsibility of this server?
          if (!primaryPartitionMap[address.ToSpatialPartitionDescriptor()])
            return;

          // Decorate the address with the production data and surveyed surface flags
          address.ProdDataRequested = false;
          address.SurveyedSurfaceDataRequested = true;

          AddSubGridToAddressList(address); // Assign the address into the group to be processed
        });
      }

      if (listCount > 0)
      {
        ProcessSubGridAddressGroup(addresses, listCount); // Process the remaining sub grids...
      }

      _log.LogInformation($"Scheduling for {_taskAddresses.Count} sub tasks to complete for sub grids request");
      try
      {
        var scheduler = DIContext.ObtainRequired<ISubGridQOSTaskScheduler>();
        if (!scheduler.Schedule(_taskAddresses, PerformSubGridRequestList, scheduler.DefaultMaxTasks()))
        {
          _log.LogError($"Failed to schedule {_taskAddresses.Count} groups of sub grids to be processed");
        }
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception waiting for group of sub grid tasks to complete");
        return null;
      }

      _log.LogInformation($"{_taskAddresses.Count} sub grid tasks completed (max size = {_addressBucketSize}), executing AcquireComputationResult()");
      return AcquireComputationResult();
    }

    /// <summary>
    /// Executes the request for sub grids
    /// </summary>
    public TSubGridRequestsResponse Execute()
    {
      var numProdDataSubGrids = ProdDataMask?.CountBits() ?? 0;
      var numSurveyedSurfaceSubGrids = SurveyedSurfaceOnlyMask?.CountBits() ?? 0;
      var numSubGridsToBeExamined = numProdDataSubGrids + numSurveyedSurfaceSubGrids;

      _log.LogInformation($"Num sub grids present in request = {numSubGridsToBeExamined} [All divisions], {numProdDataSubGrids} prod data (plus surveyed surface), {numSurveyedSurfaceSubGrids} surveyed surface only");

      if (!EstablishRequiredIgniteContext(out var contextEstablishmentResponse))
        return new TSubGridRequestsResponse {ResponseCode = contextEstablishmentResponse};

      _requestorIntermediaries = _requestorUtilities.ConstructRequestorIntermediaries
        (siteModel, localArg.Filters, localArg.IncludeSurveyedSurfaceInformation, localArg.GridDataType);

      var result = PerformSubGridRequests();

      if (result == null)
      {
        return new TSubGridRequestsResponse {ResponseCode = SubGridRequestsResponseResult.Exception};
      }

      result.NumSubgridsExamined = numSubGridsToBeExamined;

      //TODO: Map the actual response code into this
      result.ResponseCode = SubGridRequestsResponseResult.OK;

      return result;
    }
  }
}
