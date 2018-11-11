using Apache.Ignite.Core.Compute;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using VSS.TRex.Caching;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using SubGridUtilities = VSS.TRex.SubGridTrees.Core.Utilities.SubGridUtilities;
using VSS.TRex.Types;
using VSS.TRex.Utilities;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Models;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace VSS.TRex.SubGrids.GridFabric.ComputeFuncs
{
    /// <summary>
    /// The closure/function that implements subgrid request processing on compute nodes
    /// </summary>
    public abstract class SubGridsRequestComputeFuncBase<TSubGridsRequestArgument, TSubGridRequestsResponse> : IComputeFunc<TSubGridsRequestArgument, TSubGridRequestsResponse>, IDisposable
        where TSubGridsRequestArgument : SubGridsRequestArgument
        where TSubGridRequestsResponse : SubGridRequestsResponse, new()
    {
        private const int AddressBucketSize = 20;

        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridsRequestComputeFuncBase<TSubGridsRequestArgument, TSubGridRequestsResponse>>();

        /// <summary>
        /// Local reference to the client subgrid factory
        /// </summary>
        [NonSerialized]
        private IClientLeafSubgridFactory clientLeafSubGridFactory;

        private IClientLeafSubgridFactory ClientLeafSubGridFactory
          => clientLeafSubGridFactory ?? (clientLeafSubGridFactory = DIContext.Obtain<IClientLeafSubgridFactory>());

        // private static int requestCount = 0;
     
        /// <summary>
        /// Mask is the internal sub grid bit mask tree created from the serialized mask contained in the 
        /// ProdDataMaskBytes member of the argument. It is only used during processing of the request.
        /// It is marked as non serialized so the Ignite GridCompute Broadcast method does not attempt 
        /// to serialise this member as an aspect of the compute func.
        /// </summary>
        [NonSerialized]
        private ISubGridTreeBitMask ProdDataMask;

        /// <summary>
        /// Mask is the internal sub grid bit mask tree created from the serialized mask contained in the 
        /// SurveyedSurfaceOnlyMaskBytes member of the argument. It is only used during processing of the request.
        /// It is marked as non serialized so the Ignite GridCompute Broadcast method does not attempt 
        /// to serialise this member as an aspect of the compute func.
        /// </summary>
        [NonSerialized]
        private ISubGridTreeBitMask SurveyedSurfaceOnlyMask;

        [NonSerialized]
        protected SubGridsRequestArgument localArg;

        [NonSerialized]
        private ISiteModel siteModel;

        [NonSerialized]
        private ISiteModels siteModels;

        [NonSerialized]
        private IClientLeafSubGrid[][] clientGrids;

        /// <summary>
        /// The list of address being constructed prior to submission to the processing engine
        /// </summary>
        [NonSerialized]
        private ISubGridCellAddress[] addresses;

        /// <summary>
        /// The number of subgrids currently present in the process pending list
        /// </summary>
        [NonSerialized]
        private int listCount;

        [NonSerialized]
        private SubGridRequestor[] Requestors;

        [NonSerialized]
        private AreaControlSet AreaControlSet = AreaControlSet.Null();

        /// <summary>
        /// The Design to be used for querying elevation information from in the process of calculating cut-fill values
        /// </summary>
        [NonSerialized]
        private IDesign ReferenceDesign;

        [NonSerialized] private IDesignManager designManager;

        /// <summary>
        /// DI injected context for designs service
        /// </summary>
        private IDesignManager DesignManager => designManager ?? (designManager = DIContext.Obtain<IDesignManager>());

        [NonSerialized] private ITRexSpatialMemoryCache subGridCache;

        /// <summary>
        /// The DI injected TRex spatial memory cache for general subgrid results
        /// </summary>
        private ITRexSpatialMemoryCache SubGridCache => subGridCache ?? (subGridCache = DIContext.Obtain<ITRexSpatialMemoryCache>());

        [NonSerialized]
        private ITRexSpatialMemoryCacheContext SubGridCacheContext;

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SubGridsRequestComputeFuncBase()
        {
        }

        /// <summary>
        /// Cleans an array of client leaf subgrids by repatriating them to the client leaf subgrid factory
        /// </summary>
        /// <param name="SubgridResultArray"></param>
        private void CleanSubgridResultArray(IClientLeafSubGrid[] SubgridResultArray)
        {
            if (SubgridResultArray != null)
            {
                ClientLeafSubGridFactory.ReturnClientSubGrids(SubgridResultArray, SubgridResultArray.Length);
            }
        }

        /// <summary>
        /// Performs conversions from the internal subgrid client leaf type to the requested client leaf type
        /// </summary>
        /// <param name="RequestGridDataType"></param>
        /// <param name="SubgridResultArray"></param>
        private void ConvertIntermediarySubgridsToResult(GridDataType RequestGridDataType,
                                                         ref IClientLeafSubGrid[] SubgridResultArray)
        {
            IClientLeafSubGrid[] NewClientGrids = new IClientLeafSubGrid[SubgridResultArray.Length];
            ClientHeightAndTimeLeafSubGrid Subgrid1, Subgrid2;

          try
          {
            // If performing simple volume calculations, there may be an intermediary filter in play. If this is
            // the case then the first two subgrid results will be HeightAndTime elevation subgrids and will
            // need to be merged into a single height and time subgrid before any secondary conversion of intermediary
            //  results in the logic below.

            if (SubgridResultArray.Length == 3 // Three filters in play
                && SubgridResultArray[0].GridDataType == GridDataType.HeightAndTime // Height and time subgrids
                && SubgridResultArray[1].GridDataType == GridDataType.HeightAndTime
                && SubgridResultArray[2].GridDataType == GridDataType.HeightAndTime
                //&& Requests.ReferenceVolumeType == VolumeComputationType.Between2Filters // Between two filters volume request
              )
              {
                Subgrid1 = (ClientHeightAndTimeLeafSubGrid)SubgridResultArray[0];
                Subgrid2 = (ClientHeightAndTimeLeafSubGrid)SubgridResultArray[1];

          // Merge the first two results then swap the second and third items so later processing
          // uses the correct two result, and the the third is correctly recycled
          // Subgrid1 is 'latest @ first filter', subgrid 2 is earliest @ second filter
                SubGridUtilities.SubGridDimensionalIterator((I, J) =>
                {
                  // Check if there is a non null candidate in the earlier @ second filter
                  if (Subgrid1.Cells[I, J] == Consts.NullHeight && Subgrid2.Cells[I, J] != Consts.NullHeight)
                    Subgrid1.Cells[I, J] = Subgrid2.Cells[I, J];
                });

                // Swap the lst two elements...
                MinMax.Swap(ref SubgridResultArray[1], ref SubgridResultArray[2]);
              }

              if (SubgridResultArray.Length == 0)
                {
                    return;
                }

                try
                {
                    for (int I = 0; I < SubgridResultArray.Length; I++)
                    {
                        if (SubgridResultArray[I] == null)
                            continue;

                        if (SubgridResultArray[I].GridDataType != RequestGridDataType)
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
                                    if (!(SubgridResultArray[I] is ClientHeightAndTimeLeafSubGrid))
                                        Debug.Assert(SubgridResultArray[I] is ClientHeightAndTimeLeafSubGrid, $"SubgridResultArray[I] is ClientHeightAndTimeLeafSubGrid failed, is actually {SubgridResultArray[I].GetType().Name}/{SubgridResultArray[I]}");
                                    */

                                    (NewClientGrids[I] as ClientHeightLeafSubGrid).Assign(SubgridResultArray[I] as ClientHeightAndTimeLeafSubGrid);
                                    break;

                                case GridDataType.CutFill:
                                    // Just copy the height subgrid to new subgrid list
                                    NewClientGrids[I] = SubgridResultArray[I];
                                    SubgridResultArray[I] = null;
                                    break;
                            }
                        }
                        else
                        {
                            NewClientGrids[I] = SubgridResultArray[I];
                            SubgridResultArray[I] = null;
                        }
                    }

                }
                finally
                {
                    CleanSubgridResultArray(SubgridResultArray);
                }

                SubgridResultArray = NewClientGrids;
            }
            catch
            {
                CleanSubgridResultArray(NewClientGrids);
                throw;
            }
        }

        /// <summary>
        /// Take the supplied argument to the compute func and perform any necessary unpacking of the
        /// contents of it into a form ready to use. Also make a location reference to the arg parameter
        /// to allow other methods to access it as local state.
        /// </summary>
        /// <param name="arg"></param>
        protected virtual void UnpackArgument(SubGridsRequestArgument arg)
        {
            localArg = arg;

            siteModels = DIContext.Obtain<ISiteModels>();
            siteModel = siteModels.GetSiteModel(localArg.SiteModelID);

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
            if (arg.ReferenceDesignID != Guid.Empty)
                ReferenceDesign = siteModel.Designs.Locate(arg.ReferenceDesignID);
        }

        /// <summary>
        /// Take a subgrid address and request the required client subgrid depending on GridDataType
        /// </summary>
        /// <param name="requestor"></param>
        /// <param name="address"></param>
        /// <param name="clientGrid"></param>
        private ServerRequestResult PerformSubgridRequest(SubGridRequestor requestor, 
                                                          ISubGridCellAddress address, 
                                                          out IClientLeafSubGrid clientGrid)
        {
            try
            {
                // Log.InfoFormat("Requesting subgrid #{0}:{1}", ++requestCount, address.ToString());

                if (localArg.GridDataType == GridDataType.DesignHeight)
                {
                  bool designResult = ReferenceDesign.GetDesignHeights(localArg.SiteModelID, address, siteModel.Grid.CellSize,
                    out IClientHeightLeafSubGrid DesignElevations, out DesignProfilerRequestResult ProfilerRequestResult);
           
                  clientGrid = DesignElevations;
                  if (designResult || ProfilerRequestResult == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
                     return ServerRequestResult.NoError;
           
                  Log.LogError($"Design profiler subgrid elevation request for {address} failed with error {ProfilerRequestResult}");
                  return ServerRequestResult.FailedToComputeDesignElevationPatch;
                }
           
                clientGrid = ClientLeafSubGridFactory.GetSubGrid(SubGridTrees.Client.Utilities.IntermediaryICGridDataTypeForDataType(localArg.GridDataType, address.SurveyedSurfaceDataRequested));

                clientGrid.CellSize = siteModel.Grid.CellSize;
                clientGrid.SetAbsoluteLevel(SubGridTreeConsts.SubGridTreeLevels);
                clientGrid.SetAbsoluteOriginPosition((uint)(address.X & ~SubGridTreeConsts.SubGridLocalKeyMask),
                                                     (uint)(address.Y & ~SubGridTreeConsts.SubGridLocalKeyMask));

                // Reach into the subgrid request layer and retrieve an appropriate subgrid
                requestor.CellOverrideMask.Fill();

                ServerRequestResult result = requestor.RequestSubGridInternal((SubGridCellAddress)address, address.ProdDataRequested, address.SurveyedSurfaceDataRequested, clientGrid);

                if (result != ServerRequestResult.NoError)
                    Log.LogInformation($"Request for subgrid {address} request failed with code {result}");

                // Some request types require additional processing of the subgrid results prior to repatriating the answers back to the caller
                // Convert the computed intermediary grids into the client grid form expected by the caller
                if (clientGrid?.GridDataType != localArg.GridDataType)
                {
                    // Convert to an array to preserve the multiple filter semantic giving a list of subgrids to be converted (eg: volumes)
                    IClientLeafSubGrid[] ClientArray = { clientGrid };
                    ConvertIntermediarySubgridsToResult(localArg.GridDataType, ref ClientArray);

                  // If the requested data is cut fill derived from elevation data previously calculated, 
                  // then perform the conversion here
                  if (localArg.GridDataType == GridDataType.CutFill)
                    {
                        if (ClientArray.Length == 2)
                        {
                            // The cut fill is defined between two production data derived height subgrids
                            // depending on volume type work out height difference
                            CutFillUtilities.ComputeCutFillSubgrid((IClientHeightLeafSubGrid)ClientArray[0], // 'base'
                                                                   (IClientHeightLeafSubGrid)ClientArray[1]); // 'top'
                        }
                        else
                        {
                            // The cut fill is defined between one production data derived height subgrid and a
                            // height subgrid to be calculated from a designated design
                            if (!CutFillUtilities.ComputeCutFillSubgrid(ClientArray[0], // base
                                                                        ReferenceDesign, // 'top'
                                                                        localArg.SiteModelID,
                                                                        out _ /*ProfilerRequestResult*/))
                                result = ServerRequestResult.FailedToComputeDesignElevationPatch;
                        }
                    }

                    clientGrid = ClientArray[0];
                }

                return result;
            }
            catch (Exception E)
            {
                Log.LogError("Exception in PerformSubgridRequest", E);
                throw;
            }
        }

        /// <summary>
        /// Method responsible for accepting subgrids from the query engine and processing them in the next step of
        /// the request
        /// </summary>
        /// <param name="results"></param>
        /// <param name="resultCount"></param>
        public abstract void ProcessSubgridRequestResult(IClientLeafSubGrid[][] results, int resultCount);

        /// <summary>
        /// Transforms the internal aggregation state into the desired response for the request
        /// </summary>
        /// <returns></returns>
        public abstract TSubGridRequestsResponse AcquireComputationResult();

        /// <summary>
        /// Performs any necessary setup and configuration of Ignite infrastructure to support the processing of this request
        /// </summary>
        public abstract bool EstablishRequiredIgniteContext(out SubGridRequestsResponseResult contextEstablishmentResponse);

        /// <summary>
        /// Process a subset of the full set of subgrids in the request
        /// </summary>
        private void PerformSubgridRequestList()
        {
            int resultCount = 0;

            if (listCount == 0)
                return;

            //Log.LogInformation("Sending {0} subgrids to caller for processing", count);
            //Log.LogInformation($"Requestors contains {Requestors.Length} items");
          
            for (int i = 0; i < listCount; i++)
            {
                // Execute a client grid request for each requestor and create an array of the results
                clientGrids[resultCount++] = Requestors.Select(x =>
                {
                    ServerRequestResult result = PerformSubgridRequest(x, addresses[i], out IClientLeafSubGrid clientGrid);
                    return result == ServerRequestResult.NoError ? clientGrid : null;
                }).ToArray();
            }

            if (resultCount > 0)
            {
                try
                {
                    ProcessSubgridRequestResult(clientGrids, resultCount);
                }
                finally
                {
                    // Return the client grid to the factory for recycling now its role is complete here...
                    ClientLeafSubGridFactory.ReturnClientSubGrids(clientGrids, resultCount);
                }
            }
        }

        /// <summary>
        /// Adds a new address to the list of addresses being built and triggers processing of the list if it hits the critical size
        /// </summary>
        /// <param name="address"></param>
        private void AddSubgridToAddressList(ISubGridCellAddress address)
        {
            addresses[listCount++] = address;

            if (listCount == AddressBucketSize)
            {
                // Process the subgrids...
                PerformSubgridRequestList();
                listCount = 0;
            }
        }

        /// <summary>
        /// Process the set of subgrids in the request that have partition mappings that match their affinity with this node
        /// </summary>
        private TSubGridRequestsResponse PerformSubgridRequests()
        {
            clientGrids = new IClientLeafSubGrid[AddressBucketSize][];      

            // Scan through all the bitmap leaf subgrids, and for each, scan through all the subgrids as 
            // noted with the 'set' bits in the bitmask, processing only those that matter for this server

            Log.LogInformation("Scanning subgrids in request");

            ISurveyedSurfaces SurveyedSurfaceList = siteModel.SurveyedSurfaces;

            // Construct the set of requester objects to be used for the filters present in the request
            Requestors = localArg.Filters.Filters.Select
            (x => {
              // Construct the appropriate list of surveyed surfaces
              // Obtain local reference to surveyed surface list. If it is replaced while processing the
              // list then the local reference will still be valid allowing lock free read access to the list.
              ISurveyedSurfaces FilteredSurveyedSurfaces = null;

              if (localArg.IncludeSurveyedSurfaceInformation && SurveyedSurfaceList?.Count > 0)
              {
                FilteredSurveyedSurfaces = DIContext.Obtain<ISurveyedSurfaces>();

                // Filter out any surveyed surfaces which don't match current filter (if any) - realistically, this is time filters we're thinking of here
                SurveyedSurfaceList.FilterSurveyedSurfaceDetails(x.AttributeFilter.HasTimeFilter,
                  x.AttributeFilter.StartTime, x.AttributeFilter.EndTime,
                  x.AttributeFilter.ExcludeSurveyedSurfaces(), FilteredSurveyedSurfaces,
                  x.AttributeFilter.SurveyedSurfaceExclusionList);

                // Ensure that the filtered surveyed surfaces are in a known ordered state
                FilteredSurveyedSurfaces.SortChronologically(x.AttributeFilter.ReturnEarliestFilteredCellPass);
              }

              Guid[] FilteredSurveyedSurfacesAsArray = FilteredSurveyedSurfaces?.Count > 0 ? FilteredSurveyedSurfaces.Select(s => s.ID).ToArray() : new Guid[0];

              // Get a caching context for the subgrids returned by this requester
              SubGridCacheContext = SubGridCache.LocateOrCreateContext(SpatialCacheFingerprint.ConstructFingerprint
                (siteModel.ID, localArg.GridDataType, x, FilteredSurveyedSurfacesAsArray));

              return new SubGridRequestor(siteModel,
                siteModels.StorageProxy,
                x,
                false, // Override cell restriction
                BoundingIntegerExtent2D.Inverted(),
                SubGridTreeConsts.SubGridTreeLevels,
                int.MaxValue, // MaxCellPasses
                AreaControlSet,
                new FilteredValuePopulationControl(),
                ProdDataMask,
                SubGridCache,
                SubGridCacheContext,
                FilteredSurveyedSurfaces,
                FilteredSurveyedSurfacesAsArray);
            }).ToArray();

            addresses = new ISubGridCellAddress[AddressBucketSize];

            // Obtain the primary partition map to allow this request to determine the elements it needs to process
            bool[] primaryPartitionMap = ImmutableSpatialAffinityPartitionMap.Instance().PrimaryPartitions;

            // Request production data only, or hybrid production data and surveyed surface data subgrids
            ProdDataMask?.ScanAllSetBitsAsSubGridAddresses(address =>
            {
                // Is this subgrid is the responsibility of this server?
                if (!primaryPartitionMap[address.ToSpatialPartitionDescriptor()])
                   return;

                // Decorate the address with the production data and surveyed surface flags
                address.ProdDataRequested = true;
                address.SurveyedSurfaceDataRequested = localArg.IncludeSurveyedSurfaceInformation;

                AddSubgridToAddressList(address);      // Assign the address into the group to be processed
            });

            if (localArg.IncludeSurveyedSurfaceInformation)
            {  
                // Request surveyed surface only subgrids
                SurveyedSurfaceOnlyMask?.ScanAllSetBitsAsSubGridAddresses(address =>
                {
                    // Is this subgrid the responsibility of this server?
                    if (!primaryPartitionMap[address.ToSpatialPartitionDescriptor()])
                      return;
                  
                    // Decorate the address with the production data and surveyed surface flags
                    address.ProdDataRequested = false; 
                    address.SurveyedSurfaceDataRequested = true;
                  
                    AddSubgridToAddressList(address); // Assign the address into the group to be processed
                });
            }
        
            PerformSubgridRequestList();    // Process the remaining subgrids...

            return AcquireComputationResult();
        }

        /// <summary>
        /// Performs any initialization of this compute func that might ordinarily be achieved through automatic property
        /// initialization or constructor initialization that is not invoked when the compute func is created at the
        /// target server. Ignite performs an under-the-hood instantiation and hydration of state that does not invoke either
        /// automatics field initialization or constructor invocation.
        /// </summary>
        private void InitialiseComputeFunc()
        {
          AreaControlSet = AreaControlSet.Null();
        }

        /// <summary>
        /// Invoke function called in the context of the cluster compute node
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public TSubGridRequestsResponse Invoke(TSubGridsRequestArgument arg)
        {
            TSubGridRequestsResponse result;

            Log.LogInformation("In SubGridsRequestComputeFunc.invoke()");

            try
            {
                try
                {
                    InitialiseComputeFunc();

                    UnpackArgument(arg);

                    long NumSubgridsToBeExamined = ProdDataMask?.CountBits() ?? 0 + SurveyedSurfaceOnlyMask?.CountBits() ?? 0;

                    Log.LogInformation($"Num subgrids present in request = {NumSubgridsToBeExamined} [All divisions]");

                    if (!EstablishRequiredIgniteContext(out SubGridRequestsResponseResult contextEstablishmentResponse))
                        return new TSubGridRequestsResponse { ResponseCode = contextEstablishmentResponse };

                    result = PerformSubgridRequests();
                    result.NumSubgridsExamined = NumSubgridsToBeExamined;

                    //TODO: Map the actual response code into this
                    result.ResponseCode = SubGridRequestsResponseResult.OK;
                }
                finally
                {
                    Log.LogInformation("Out SubGridsRequestComputeFunc.invoke()");
                }
            }
            catch (Exception E)
            {
                Log.LogError($"Exception occurred:\n{E}");

                return new TSubGridRequestsResponse { ResponseCode = SubGridRequestsResponseResult.Unknown };
            }

            return result;
        }

        protected virtual void DoDispose()
        {
           // No dispose behaviour for base compute function
        }

        /// <summary>
        /// Implementation of the IDisposable interface
        /// </summary>
        public void Dispose()
        {
            DoDispose();
        }
    }
}
