using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.Common.Types;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using SubGridUtilities = VSS.TRex.SubGridTrees.Core.Utilities.SubGridUtilities;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Models;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace VSS.TRex.SubGrids.Executors
{
  /// <summary>
  /// The closure/function that implements sub grid request processing on compute nodes
  /// </summary>
  public abstract class SubGridsRequestComputeFuncBase_Executor_Base<TSubGridsRequestArgument, TSubGridRequestsResponse>
    where TSubGridsRequestArgument : SubGridsRequestArgument
    where TSubGridRequestsResponse : SubGridRequestsResponse, new()
  {
    private const int AddressBucketSize = 20;

    // ReSharper disable once StaticMemberInGenericType
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridsRequestComputeFuncBase_Executor_Base<TSubGridsRequestArgument, TSubGridRequestsResponse>>();

    private readonly bool _enableGeneralSubGridResultCaching = DIContext.Obtain<IConfigurationStore>().GetValueBool("ENABLE_GENERAL_SUBGRID_RESULT_CACHING", Consts.ENABLE_GENERAL_SUBGRID_RESULT_CACHING);

    /// <summary>
    /// Local reference to the client sub grid factory
    /// </summary>
    private IClientLeafSubGridFactory clientLeafSubGridFactory;

    private IClientLeafSubGridFactory ClientLeafSubGridFactory
      => clientLeafSubGridFactory ?? (clientLeafSubGridFactory = DIContext.Obtain<IClientLeafSubGridFactory>());

    // private static int requestCount = 0;

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

    protected SubGridsRequestArgument localArg;

    private ISiteModel siteModel;

    private ISiteModels siteModels;

    /// <summary>
    /// The list of address being constructed prior to submission to the processing engine
    /// </summary>
    private ISubGridCellAddress[] addresses;

    /// <summary>
    /// The number of sub grids currently present in the process pending list
    /// </summary>
    private int listCount;

    private AreaControlSet AreaControlSet = AreaControlSet.CreateAreaControlSet();

    /// <summary>
    /// The Design to be used for querying elevation information from in the process of calculating cut-fill values
    /// </summary>
    private IDesign ReferenceDesign;

    private ITRexSpatialMemoryCache subGridCache;

    /// <summary>
    /// The DI injected TRex spatial memory cache for general sub grid results
    /// </summary>
    private ITRexSpatialMemoryCache SubGridCache => subGridCache ?? (subGridCache = DIContext.Obtain<ITRexSpatialMemoryCache>());

    /// <summary>
    /// Cleans an array of client leaf sub grids by repatriating them to the client leaf sub grid factory
    /// </summary>
    /// <param name="SubGridResultArray"></param>
    private void CleanSubGridResultArray(IClientLeafSubGrid[] SubGridResultArray)
    {
      if (SubGridResultArray != null)
      {
        ClientLeafSubGridFactory.ReturnClientSubGrids(SubGridResultArray, SubGridResultArray.Length);
      }
    }

    /// <summary>
    /// Performs conversions from the internal sub grid client leaf type to the requested client leaf type
    /// </summary>
    /// <param name="RequestGridDataType"></param>
    /// <param name="SubGridResultArray"></param>
    private void ConvertIntermediarySubGridsToResult(GridDataType RequestGridDataType,
      ref IClientLeafSubGrid[] SubGridResultArray)
    {
      IClientLeafSubGrid[] NewClientGrids = new IClientLeafSubGrid[SubGridResultArray.Length];
      ClientHeightAndTimeLeafSubGrid SubGrid1, SubGrid2;

      try
      {
        // If performing simple volume calculations, there may be an intermediary filter in play. If this is
        // the case then the first two sub grid results will be HeightAndTime elevation sub grids and will
        // need to be merged into a single height and time sub grid before any secondary conversion of intermediary
        //  results in the logic below.

        if (SubGridResultArray.Length == 3 // Three filters in play
            && SubGridResultArray[0].GridDataType == GridDataType.HeightAndTime // Height and time sub grids
            && SubGridResultArray[1].GridDataType == GridDataType.HeightAndTime
            && SubGridResultArray[2].GridDataType == GridDataType.HeightAndTime
          //&& Requests.ReferenceVolumeType == VolumeComputationType.Between2Filters // Between two filters volume request
        )
        {
          SubGrid1 = (ClientHeightAndTimeLeafSubGrid) SubGridResultArray[0];
          SubGrid2 = (ClientHeightAndTimeLeafSubGrid) SubGridResultArray[1];

          // Merge the first two results then swap the second and third items so later processing
          // uses the correct two result, and the the third is correctly recycled
          // Subgrid1 is 'latest @ first filter', sub grid 2 is earliest @ second filter
          SubGridUtilities.SubGridDimensionalIterator((I, J) =>
          {
            // Check if there is a non null candidate in the earlier @ second filter
            if (SubGrid1.Cells[I, J] == Consts.NullHeight && SubGrid2.Cells[I, J] != Consts.NullHeight)
              SubGrid1.Cells[I, J] = SubGrid2.Cells[I, J];
          });

          // Swap the lst two elements...
          MinMax.Swap(ref SubGridResultArray[1], ref SubGridResultArray[2]);
        }

        if (SubGridResultArray.Length == 0)
        {
          return;
        }

        try
        {
          for (int I = 0; I < SubGridResultArray.Length; I++)
          {
            if (SubGridResultArray[I] == null)
              continue;

            if (SubGridResultArray[I].GridDataType != RequestGridDataType)
            {
              switch (RequestGridDataType)
              {
                case GridDataType.SimpleVolumeOverlay:
                  Debug.Assert(false, "SimpleVolumeOverlay not implemented");
                  break;

                case GridDataType.Height:
                  NewClientGrids[I] = ClientLeafSubGridFactory.GetSubGrid(GridDataType.Height);
                  NewClientGrids[I].CellSize = siteModel.Grid.CellSize;

                  /*
                  Debug.Assert(NewClientGrids[I] is ClientHeightLeafSubGrid, $"NewClientGrids[I] is ClientHeightLeafSubGrid failed, is actually {NewClientGrids[I].GetType().Name}/{NewClientGrids[I]}");
                  if (!(SubGridResultArray[I] is ClientHeightAndTimeLeafSubGrid))
                      Debug.Assert(SubGridResultArray[I] is ClientHeightAndTimeLeafSubGrid, $"SubGridResultArray[I] is ClientHeightAndTimeLeafSubGrid failed, is actually {SubGridResultArray[I].GetType().Name}/{SubGridResultArray[I]}");
                  */

                  (NewClientGrids[I] as ClientHeightLeafSubGrid).Assign(SubGridResultArray[I] as ClientHeightAndTimeLeafSubGrid);
                  break;

                case GridDataType.CutFill:
                  // Just copy the height sub grid to new sub grid list
                  NewClientGrids[I] = SubGridResultArray[I];
                  SubGridResultArray[I] = null;
                  break;
              }
            }
            else
            {
              NewClientGrids[I] = SubGridResultArray[I];
              SubGridResultArray[I] = null;
            }
          }

        }
        finally
        {
          CleanSubGridResultArray(SubGridResultArray);
        }

        SubGridResultArray = NewClientGrids;
      }
      catch
      {
        CleanSubGridResultArray(NewClientGrids);
        throw;
      }
    }

    /// <summary>
    /// Take the supplied argument to the compute func and perform any necessary unpacking of the
    /// contents of it into a form ready to use. Also make a location reference to the arg parameter
    /// to allow other methods to access it as local state.
    /// </summary>
    /// <param name="arg"></param>
    public virtual void UnpackArgument(SubGridsRequestArgument arg)
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
      if (arg.ReferenceDesignUID != Guid.Empty)
        ReferenceDesign = siteModel.Designs.Locate(arg.ReferenceDesignUID);

      AreaControlSet = arg.AreaControlSet;
    }

    /// <summary>
    /// Take a sub grid address and request the required client sub grid depending on GridDataType
    /// </summary>
    /// <param name="requester"></param>
    /// <param name="address"></param>
    /// <param name="clientGrid"></param>
    private ServerRequestResult PerformSubGridRequest(ISubGridRequestor requester,
      ISubGridCellAddress address,
      out IClientLeafSubGrid clientGrid)
    {
      // Log.InfoFormat("Requesting sub grid #{0}:{1}", ++requestCount, address.ToString());

      if (localArg.GridDataType == GridDataType.DesignHeight)
      {
        ReferenceDesign.GetDesignHeights(localArg.ProjectID, address, siteModel.Grid.CellSize,
          out IClientHeightLeafSubGrid DesignElevations, out DesignProfilerRequestResult ProfilerRequestResult);

        clientGrid = DesignElevations;
        if (ProfilerRequestResult == DesignProfilerRequestResult.OK || ProfilerRequestResult == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
          return ServerRequestResult.NoError;

        Log.LogError($"Design profiler sub grid elevation request for {address} failed with error {ProfilerRequestResult}");
        return ServerRequestResult.FailedToComputeDesignElevationPatch;
      }

      clientGrid = ClientLeafSubGridFactory.GetSubGridEx(Utilities.IntermediaryICGridDataTypeForDataType(localArg.GridDataType, address.SurveyedSurfaceDataRequested),
        siteModel.Grid.CellSize, SubGridTreeConsts.SubGridTreeLevels, 
        (uint) (address.X & ~SubGridTreeConsts.SubGridLocalKeyMask), (uint) (address.Y & ~SubGridTreeConsts.SubGridLocalKeyMask));

      // Reach into the sub grid request layer and retrieve an appropriate sub grid
      requester.CellOverrideMask.Fill();

      ServerRequestResult result = requester.RequestSubGridInternal((SubGridCellAddress) address, address.ProdDataRequested, address.SurveyedSurfaceDataRequested, clientGrid);

      if (result != ServerRequestResult.NoError)
        Log.LogError($"Request for sub grid {address} request failed with code {result}");

      // Some request types require additional processing of the sub grid results prior to repatriating the answers back to the caller
      // Convert the computed intermediary grids into the client grid form expected by the caller
      if (clientGrid?.GridDataType != localArg.GridDataType)
      {
        // Convert to an array to preserve the multiple filter semantic giving a list of sub grids to be converted (eg: volumes)
        IClientLeafSubGrid[] ClientArray = {clientGrid};
        ConvertIntermediarySubGridsToResult(localArg.GridDataType, ref ClientArray);

        // If the requested data is cut fill derived from elevation data previously calculated, 
        // then perform the conversion here
        if (localArg.GridDataType == GridDataType.CutFill)
        {
          if (ClientArray.Length == 2)
          {
            // The cut fill is defined between two production data derived height sub grids
            // depending on volume type work out height difference
            CutFillUtilities.ComputeCutFillSubgrid((IClientHeightLeafSubGrid) ClientArray[0], // 'base'
              (IClientHeightLeafSubGrid) ClientArray[1]); // 'top'
          }
          else
          {
            // The cut fill is defined between one production data derived height sub grid and a
            // height sub grid to be calculated from a designated design
            if (!CutFillUtilities.ComputeCutFillSubgrid(ClientArray[0], // base
              ReferenceDesign, // 'top'
              localArg.ProjectID,
              out _ /*ProfilerRequestResult*/))
              result = ServerRequestResult.FailedToComputeDesignElevationPatch;
          }
        }

        clientGrid = ClientArray[0];
      }

      return result;
    }

    /// <summary>
    /// Method responsible for accepting sub grids from the query engine and processing them in the next step of
    /// the request
    /// </summary>
    /// <param name="results"></param>
    /// <param name="resultCount"></param>
    protected abstract void ProcessSubGridRequestResult(IClientLeafSubGrid[][] results, int resultCount);

    /// <summary>
    /// Transforms the internal aggregation state into the desired response for the request
    /// </summary>
    /// <returns></returns>
    protected abstract TSubGridRequestsResponse AcquireComputationResult();

    /// <summary>
    /// Performs any necessary setup and configuration of Ignite infrastructure to support the processing of this request
    /// </summary>
    protected abstract bool EstablishRequiredIgniteContext(out SubGridRequestsResponseResult contextEstablishmentResponse);

    /// <summary>
    /// Process a subset of the full set of sub grids in the request
    /// </summary>
    private void PerformSubGridRequestList(ISubGridCellAddress[] addressList, int addressCount)
    {
      if (addressCount == 0)
        return;

      // Construct the set of requester objects to be used for the filters present in the request
      var Requestors = DIContext.Obtain<IRequestorUtilities>().ConstructRequestors(siteModel, RequestorIntermediaries, localArg.AreaControlSet, ProdDataMask);

      //Log.LogInformation("Sending {0} sub grids to caller for processing", count);
      //Log.LogInformation($"Requester list contains {Requestors.Length} items");

      IClientLeafSubGrid[][] clientGrids = new IClientLeafSubGrid[addressCount][];

      // Execute a client grid request for each requester and create an array of the results
      for (int i = 0; i < addressCount; i++)
      {
        clientGrids[i] = Requestors.Select(x => PerformSubGridRequest(x, addressList[i], out IClientLeafSubGrid clientGrid) == ServerRequestResult.NoError ? clientGrid : null).ToArray();
      }

      try
      {
        ProcessSubGridRequestResult(clientGrids, addressCount);
      }
      finally
      {
        // Return the client grid to the factory for recycling now its role is complete here...
        ClientLeafSubGridFactory.ReturnClientSubGrids(clientGrids, addressCount);
      }
    }

    private readonly List<Task> tasks = new List<Task>();

    private void ProcessSubGridAddressGroup(ISubGridCellAddress[] addressList, int addressCount)
    {
      ISubGridCellAddress[] addressListCopy = new ISubGridCellAddress[addressCount];
      Array.Copy(addressList, addressListCopy, addressCount);

      tasks.Add(Task.Run(() => PerformSubGridRequestList(addressListCopy, addressCount)));
    }

    /// <summary>
    /// Adds a new address to the list of addresses being built and triggers processing of the list if it hits the critical size
    /// </summary>
    /// <param name="address"></param>
    private void AddSubGridToAddressList( ISubGridCellAddress address)
    {
      addresses[listCount++] = address;

      if (listCount == AddressBucketSize)
      {
        // Process the sub grids...
        ProcessSubGridAddressGroup(addresses, listCount);
        listCount = 0;
      }
    }

    /// <summary>
    /// The collection of requestor intermediaries that are derived from to create requestor delegates
    /// </summary>
    private (ICombinedFilter Filter, 
      ISurveyedSurfaces FilteredSurveyedSurfaces, 
      Guid[] FilteredSurveyedSurfacesAsArray,
      ISurfaceElevationPatchRequest surfaceElevationPatchRequest,
      ITRexSpatialMemoryCacheContext CacheContext)[] RequestorIntermediaries;

    /// <summary>
    /// Process the set of sub grids in the request that have partition mappings that match their affinity with this node
    /// </summary>
    private TSubGridRequestsResponse PerformSubGridRequests()
    {
      // Scan through all the bitmap leaf sub grids, and for each, scan through all the sub grids as 
      // noted with the 'set' bits in the bitmask, processing only those that matter for this server

      Log.LogInformation("Scanning sub grids in request");

      addresses = new ISubGridCellAddress[AddressBucketSize];

      // Obtain the primary partition map to allow this request to determine the elements it needs to process
      bool[] primaryPartitionMap = ImmutableSpatialAffinityPartitionMap.Instance().PrimaryPartitions;

      // Request production data only, or hybrid production data and surveyed surface data sub grids
      ProdDataMask?.ScanAllSetBitsAsSubGridAddresses(address =>
      {
        // Is this sub grid is the responsibility of this server?
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

      ProcessSubGridAddressGroup(addresses, listCount); // Process the remaining sub grids...

      // Wait for all the sub-tasks to complete
      var summaryTask = Task.WhenAll(tasks);
      summaryTask.Wait();

      if (summaryTask.Status == TaskStatus.RanToCompletion)
        return AcquireComputationResult();

      Log.LogError("Failed to process all sub grids");
      return null;
    }

  public TSubGridRequestsResponse Execute()
    {
      long NumSubGridsToBeExamined = ProdDataMask?.CountBits() ?? 0 + SurveyedSurfaceOnlyMask?.CountBits() ?? 0;

      Log.LogInformation($"Num sub grids present in request = {NumSubGridsToBeExamined} [All divisions]");

      if (!EstablishRequiredIgniteContext(out SubGridRequestsResponseResult contextEstablishmentResponse))
        return new TSubGridRequestsResponse {ResponseCode = contextEstablishmentResponse};

      RequestorIntermediaries = DIContext.Obtain<IRequestorUtilities>().ConstructRequestorIntermediaries(siteModel, localArg.Filters, localArg.IncludeSurveyedSurfaceInformation, localArg.GridDataType);

      TSubGridRequestsResponse result = PerformSubGridRequests();
      result.NumSubgridsExamined = NumSubGridsToBeExamined;

      //TODO: Map the actual response code into this
      result.ResponseCode = SubGridRequestsResponseResult.OK;

      return result;
    }
  }
}
