using Apache.Ignite.Core.Compute;
using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Diagnostics;
using VSS.TRex.Designs.Storage;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.GridFabric.Types;
using VSS.TRex.Services.Designs;
using VSS.TRex.SiteModels;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Utilities;
using VSS.TRex.Types;
using VSS.TRex.Utilities;
using VSS.TRex.DesignProfiling;
using VSS.TRex.Filters;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.GridFabric.ComputeFuncs
{
    /// <summary>
    /// The closure/function that implements subgrid request processing on compute nodes
    /// </summary>
    [Serializable]
    public abstract class SubGridsRequestComputeFuncBase<TSubGridsRequestArgument, TSubGridRequestsResponse> : IComputeFunc<TSubGridsRequestArgument, TSubGridRequestsResponse>, IDisposable
        where TSubGridsRequestArgument : SubGridsRequestArgument
        where TSubGridRequestsResponse : SubGridRequestsResponse, new()
    {
        private const int addressBucketSize = 20;

        [NonSerialized]
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        [NonSerialized]
        private static IClientLeafSubgridFactory ClientLeafSubGridFactory = ClientLeafSubgridFactoryFactory.GetClientLeafSubGridFactory();

        // private static int requestCount = 0;

        /// <summary>
        /// Mask is the internal sub grid bit mask tree created from the serialised mask contained in the 
        /// ProdDataMaskBytes member of the argument. It is only used during processing of the request.
        /// It is marked as non serialised so the Ignite GridCompute Broadcast method does not attempt 
        /// to serialise this member as an aspect of the compute func.
        /// </summary>
        [NonSerialized]
        private SubGridTreeSubGridExistenceBitMask ProdDataMask;

        /// <summary>
        /// Mask is the internal sub grid bit mask tree created from the serialised mask contained in the 
        /// SurveydSurfaceOnlyMaskBytes member of the argument. It is only used during processing of the request.
        /// It is marked as non serialised so the Ignite GridCompute Broadcast method does not attempt 
        /// to serialise this member as an aspect of the compute func.
        /// </summary>
        [NonSerialized]
        private SubGridTreeSubGridExistenceBitMask SurveyedSurfaceOnlyMask;

        [NonSerialized]
        protected SubGridsRequestArgument localArg;

        [NonSerialized]
        private ISiteModel siteModel;

        [NonSerialized]
        private IClientLeafSubGrid[][] clientGrids;

        /// <summary>
        /// The list of address being constructed prior to summission to the processing engine
        /// </summary>
        [NonSerialized]
        private SubGridCellAddress[] addresses;

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
        private Design CutFillDesign;

        [NonSerialized]
        private bool[] PrimaryPartitionMap; 

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

            try
            {
                if (SubgridResultArray.Length == 0)
                {
                    return;
                }

                try
                {
                    for (int I = 0; I < SubgridResultArray.Length; I++)
                    {
                        if (SubgridResultArray[I] == null)
                        {
                            continue;
                        }

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

            // Unpack the mask from the argument.
            // TODO: Would be nice to use the FromBytes/ToBytes pattern here

            if (arg.ProdDataMaskBytes != null)
            {
                ProdDataMask = new SubGridTreeSubGridExistenceBitMask();

                using (MemoryStream ms = new MemoryStream(arg.ProdDataMaskBytes))
                {
                    using (BinaryReader reader = new BinaryReader(ms))
                    {
                        SubGridTreePersistor.Read(ProdDataMask, reader);
                    }
                }
            }

            if (arg.SurveyedSurfaceOnlyMaskBytes != null)
            {
                SurveyedSurfaceOnlyMask = new SubGridTreeSubGridExistenceBitMask();
                using (MemoryStream ms = new MemoryStream(arg.SurveyedSurfaceOnlyMaskBytes))
                {
                    using (BinaryReader reader = new BinaryReader(ms))
                    {
                        SubGridTreePersistor.Read(SurveyedSurfaceOnlyMask, reader);
                    }
                }
            }

            // Set up any required cut fill design
            if (arg.CutFillDesignID != Guid.Empty)
            {
                CutFillDesign = DesignsService.Instance().Find(arg.SiteModelID, arg.CutFillDesignID);
            }
        }


        /// <summary>
        /// Take a subgrid address and request the required client subgrid depending on GridDataType
        /// </summary>
        /// <param name="requestor"></param>
        /// <param name="address"></param>
        /// <param name="clientGrid"></param>
        private ServerRequestResult PerformSubgridRequest(SubGridRequestor requestor, 
                                                          SubGridCellAddress address, 
                                                          out IClientLeafSubGrid clientGrid)
        {
            try
            {
                // Log.InfoFormat("Requesting subgrid #{0}:{1}", ++requestCount, address.ToString());

                clientGrid = ClientLeafSubGridFactory.GetSubGrid(SubGridTrees.Client.Utilities.IntermediaryICGridDataTypeForDataType(localArg.GridDataType, address.SurveyedSurfaceDataRequested));

                clientGrid.CellSize = siteModel.Grid.CellSize;
                clientGrid.SetAbsoluteLevel(SubGridTree.SubGridTreeLevels);
                clientGrid.SetAbsoluteOriginPosition((uint)(address.X & ~((int)SubGridTree.SubGridLocalKeyMask)),
                                                     (uint)(address.Y & ~((int)SubGridTree.SubGridLocalKeyMask)));

                // Reach into the subgrid request layer and retrieve an appropriate subgrid
                requestor.CellOverrideMask = SubGridTreeBitmapSubGridBits.FullMask;
                ServerRequestResult result = requestor.RequestSubGridInternal(address, address.ProdDataRequested, address.SurveyedSurfaceDataRequested, clientGrid);

                if (result != ServerRequestResult.NoError)
                {
                    Log.LogInformation(string.Format("Request for subgrid {0} request failed with code {1}", address, result));
                }

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
                            // depending on volumetype work out height difference
                            CutFillUtilities.ComputeCutFillSubgrid(ClientArray[0], // 'base'
                                                                   ClientArray[1]); // 'top'
                        }
                        else
                        {
                            // The cut fill is defined between one production data derived height subgrid and a
                            // height subgrid to be calculated from a designated design
                            if (!CutFillUtilities.ComputeCutFillSubgrid(ClientArray[0], // base
                                                                        CutFillDesign, // 'top'
                                                                        localArg.SiteModelID,
                                                                        out DesignProfilerRequestResult ProfilerRequestResult))
                            {
                                result = ServerRequestResult.FailedToComputeDesignElevationPatch;
                            }
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
        /// Performs any necessary setup and configuration of Ignite insfrastructure to support the processing of this request
        /// </summary>
        public abstract bool EstablishRequiredIgniteContext(out SubGridRequestsResponseResult contextEstablishmentResponse);

        /// <summary>
        /// Process a subset of the full set of subgrids in the request
        /// </summary>
        private void PerformSubgridRequestList()
        {
            int resultCount = 0;

            if (listCount == 0)
            {
                return;
            }

            //Log.InfoFormat("Sending {0} subgrids to caller for processing", count);

            for (int i = 0; i < listCount; i++)
            {
                // Execute a client grid request for each reqeustor and create an array of the results
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
        private void AddSubgridToAddressList(SubGridCellAddress address)
        {
            addresses[listCount++] = address;

            if (listCount == addressBucketSize)
            {
                // Process the subgrids...
                PerformSubgridRequestList();
                listCount = 0;
            }
        }

        /// <summary>
        /// Process the full set of subgrids in the request
        /// </summary>
        private TSubGridRequestsResponse PerformSubgridRequests()
        {
            clientGrids = new IClientLeafSubGrid[addressBucketSize][];

            // Scan through all the bitmap leaf subgrids, and for each, scan through all the subgrids as 
            // noted with the 'set' bits in the bitmask, processing only those that matter for this server

            Log.LogInformation("Scanning subgrids in request");

            siteModel = SiteModels.SiteModels.Instance().GetSiteModel(localArg.SiteModelID);

          FilteredValuePopulationControl PopulationControl = new FilteredValuePopulationControl();

      // TODO: Use TICServerProfiler.PreparePopulationControl per Raptor to set flags appropriately.
          // Currently all events are requested except for the unimplemented ones
       PopulationControl.Fill();
          PopulationControl.WantsEventMachineCompactionRMVJumpThreshold = false;
          PopulationControl.WantsEventMapResetValues = false;
          PopulationControl.WantsEventInAvoidZoneStateValues = false;
        //  TICServerProfiler.PreparePopulationControl(ClientGrid.GridDataType, LiftBuildSettings, PassFilter, ClientGrid);

        // Construct the set of requestors to be used for the filters present in the request
        Requestors = localArg.Filters.Filters.Select
                (x => new SubGridRequestor(siteModel,
                                           SiteModels.SiteModels.ImmutableStorageProxy,
                                           x,
                                           false, // Override cell restriction
                                           BoundingIntegerExtent2D.Inverted(),
                                           SubGridTree.SubGridTreeLevels,
                                           int.MaxValue, // MaxCellPasses
                                           AreaControlSet,
                                           PopulationControl)
                 ).ToArray();

            addresses = new SubGridCellAddress[addressBucketSize];

            // Obtain the primary partition map to allow this request to determine the elements it needs to process
            PrimaryPartitionMap = ImmutableSpatialAffinityPartitionMap.Instance().PrimaryPartitions;

            // Request production data only, or hybrid production data and surveyd surface data subgrids
            ProdDataMask?.ScanAllSetBitsAsSubGridAddresses(address =>
            {
                // Is this subgrid is the responsibility of this server?
                if (!PrimaryPartitionMap[address.ToSpatialPartitionDescriptor()])
                   return;

                // Decorate the address with the production data and surveyed surface flags
                address.ProdDataRequested = true;
                address.SurveyedSurfaceDataRequested = localArg.IncludeSurveyedSurfaceInformation;

                AddSubgridToAddressList(address);      // Assign the address into the group to be processed
            });

            // Request surveyd surface only subgrids
            SurveyedSurfaceOnlyMask?.ScanAllSetBitsAsSubGridAddresses(address =>
            {
                // Is this subgrid the responsibility of this server?
                if (!PrimaryPartitionMap[address.ToSpatialPartitionDescriptor()])
                    return;

                // Decorate the address with the production data and surveyed surface flags
                address.ProdDataRequested = false; // TODO: This is a bit of an assumption and assumes the subgrid request is not solely driven by the existance of a subgrid in a surveyed surface
                address.SurveyedSurfaceDataRequested = localArg.IncludeSurveyedSurfaceInformation;

                AddSubgridToAddressList(address);      // Assign the address into the group to be processed
            });

            PerformSubgridRequestList();    // Process the remaining subgrids...

            return AcquireComputationResult();
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
                    UnpackArgument(arg);

                    long NumSubgridsToBeExamined = ProdDataMask?.CountBits() ?? 0 + SurveyedSurfaceOnlyMask?.CountBits() ?? 0;

                    Log.LogInformation($"Num subgrids present in request = {NumSubgridsToBeExamined} [All divisions]");

                    if (!EstablishRequiredIgniteContext(out SubGridRequestsResponseResult contextEstablishmentResponse))
                    {
                        return new TSubGridRequestsResponse { ResponseCode = contextEstablishmentResponse };
                    }

                    result = PerformSubgridRequests();
                    result.NumSubgridsExamined = NumSubgridsToBeExamined;

                    //TODO: Map the actual response code in to this
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
        /// Implementation of the IDisposabe interface
        /// </summary>
        public void Dispose()
        {
            DoDispose();
        }
    }
}
