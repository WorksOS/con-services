using Apache.Ignite.Core;
using Apache.Ignite.Core.Compute;
using Apache.Ignite.Core.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.GridFabric.Arguments;
using VSS.VisionLink.Raptor.GridFabric.Responses;
using VSS.VisionLink.Raptor.GridFabric.Types;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.Interfaces;
using log4net;
using System.Reflection;
using Apache.Ignite.Core.Cluster;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using System.Diagnostics;
using VSS.VisionLink.Raptor.Utilities;

namespace VSS.VisionLink.Raptor.GridFabric.ComputeFuncs
{
    /// <summary>
    /// The closure/function that implements subgrid request processing on compute nodes
    /// </summary>
    [Serializable]
    class SubGridsRequestComputeFunc : IComputeFunc<SubGridsRequestArgument, SubGridRequestsResponse>
    {
        [NonSerialized]
        private const int addressBucketSize = 20;

        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Local copy of the number of processing subdivisions
        /// </summary>
        [NonSerialized]
        private static uint numSpatialProcessingDivisions = RaptorConfig.numSpatialProcessingDivisions;

        /// <summary>
        /// Local copy of the spatial subdision descriptor supplied to this compute server
        /// </summary>
        [NonSerialized]
        private static uint spatialSubdivisionDescriptor = RaptorServerConfig.Instance().SpatialSubdivisionDescriptor;

        [NonSerialized]
        private static IClientLeafSubgridFactory ClientLeafSubGridFactory = ClientLeafSubgridFactoryFactory.GetClientLeafSubGridFactory();
//        private static int requestCount = 0;

        /// <summary>
        /// Mask is the internal sub grid bit mask tree created from the serialised mask contained in the 
        /// MaskStream member of the argument. It is only used during processing of the request.
        /// It is marked as non serialised so the Ignite GridCompute Broadcast method does not attempt 
        /// to serialise this member as an aspect of the compute func.
        /// </summary>
        [NonSerialized]
        private SubGridTreeBitMask mask = null;

        [NonSerialized]
        private SubGridsRequestArgument localArg = null;

        [NonSerialized]
        private string raptorNodeIDAsString = String.Empty;

        [NonSerialized]
        private IIgnite ignite = null;

        [NonSerialized]
        private IClusterGroup group = null;

        [NonSerialized]
        private IMessaging rmtMsg = null;

        [NonSerialized]
        private AreaControlSet areaControlSet;

        [NonSerialized]
        private SiteModel siteModel = null;

        [NonSerialized]
        private MemoryStream MS = null; 

        [NonSerialized]
        private IClientLeafSubGrid[] clientGrids = null;

        /// <summary>
        /// Take the supplied argument to the compute func and perform any necessary unpacking of the
        /// contents of it into a form ready to use. Also make a location reference to the arg parameter
        /// to allow other methods to access it as local state.
        /// </summary>
        /// <param name="arg"></param>
        private void UnpackArgument(SubGridsRequestArgument arg)
        {
            localArg = arg;
            raptorNodeIDAsString = arg.RaptorNodeID.ToString();

            Log.InfoFormat("raptorNodeIDAsString is {0} in UnpackArgument()", raptorNodeIDAsString);
            
            // Unpack the mask from the argument
            mask = new SubGridTreeBitMask();
            arg.MaskStream.Position = 0;

            using (BinaryReader reader = new BinaryReader(arg.MaskStream, Encoding.UTF8, true))
            {
                SubGridTreePersistor.Read(mask, reader);
            }
        }

        /// <summary>
        /// Take a subgrid address and request the required client subgrid depending on GridDataType
        /// </summary>
        /// <param name="address"></param>
        private ServerRequestResult PerformSubgridRequest(SubGridCellAddress address, out IClientLeafSubGrid clientGrid)
        {
            try
            {
                //Log.Info(String.Format("PerformSubgridRequest: {0} --> Examine spatial descriptor ({1} vs {2}) on {3} divisions", 
                //    address.ToString(), address.ToSpatialDivisionDescriptor(numSpatialProcessingDivisions), 
                //    spatialSubdivisionDescriptor, numSpatialProcessingDivisions));
                // Log.InfoFormat("Requesting subgrid #{0}:{1}", ++requestCount, address.ToString());

                clientGrid = ClientLeafSubGridFactory.GetSubGrid(localArg.GridDataType);
                clientGrid.CellSize = siteModel.Grid.CellSize;

                // Reach into the subgrid request layer and retrieve an appropriate subgrid
                ServerRequestResult result = SubGridRequestor.RequestSubGridInternal
                   (localArg.Filters.Filters.Count() > 0 ? localArg.Filters.Filters[0] : null,
                    int.MaxValue, // MaxCellPasses
                    false, // Override cell restriction
                    BoundingIntegerExtent2D.Inverted(), // Override cell restriction
                    siteModel,
                    address,
                    SubGridTree.SubGridTreeLevels,
                    true, // Want production data
                    false, // Dont want surveyed surface data
                    clientGrid,
                    SubGridTreeBitmapSubGridBits.FullMask,
                    ref areaControlSet
                    );

                if (result != ServerRequestResult.NoError)
                {
                    Log.Info(String.Format("Request for subgrid {0} request failed with code {1}", address, result));
                    //throw new ArgumentException(String.Format("Subgrid request failed with code {0}", result));
                }

                return result;
            }
            catch (Exception E)
            {
                Log.Error("Exception in PerformSubgridRequest", E);
                throw;
            }
        }

        /// <summary>
        /// Process a subset of the full set of subgrids in the request
        /// </summary>
        /// <param name="addresses"></param>
        /// <param name="count"></param>
        private void PerformSubgridRequestList(SubGridCellAddress[] addresses, int count)
        {
            int resultCount = 0;

            //Log.InfoFormat("Sending {0} subgrids to caller for processing", count);

            for (int i = 0; i < count; i++)
            {
                ServerRequestResult result = PerformSubgridRequest(addresses[i], out IClientLeafSubGrid clientGrid);

                if (result == ServerRequestResult.NoError)
                {
                    clientGrids[resultCount++] = clientGrid;
                }
            }

            // Package the resulting subgrids into the MemoryStream
            MS.Position = 0;

            using (BinaryWriter writer = new BinaryWriter(MS, Encoding.UTF8, true))
            {
                byte[] buffer = new byte[10000];

                writer.Write(resultCount);

                for (int i = 0; i < resultCount; i++)
                {
                    clientGrids[i].Write(writer, buffer);

                    // Return the client grid to the factory for recycling now its role is complete here... when using ConcurrentBag
                    //ClientLeafSubGridFactory.ReturnClientSubGrid(ref clientGrids[i]);
                }

                // Return the client grid to the factory for recycling now its role is complete here... when using SimpleConcurrentBag
                ClientLeafSubGridFactory.ReturnClientSubGrids(clientGrids[0].GridDataType, clientGrids, resultCount);
            }

            // ... and send it to the message topic in the compute func
            try
            {
                // Log.InfoFormat("Sending result to {0} ({1} receivers) - First = {2}/{3}", 
                //                localArg.MessageTopic, rmtMsg.ClusterGroup.GetNodes().Count, 
                //                rmtMsg.ClusterGroup.GetNodes().First().GetAttribute<string>("Role"),
                //                rmtMsg.ClusterGroup.GetNodes().First().GetAttribute<string>("RaptorNodeID"));
                byte[] bytes = new byte[MS.Position];
                MS.Position = 0;
                MS.Read(bytes, 0, bytes.Length);
                rmtMsg.Send(bytes, localArg.MessageTopic);
            }
            catch (Exception E)
            {
                Log.Error("Exception sending message", E);
                throw;
            }
        }

        /// <summary>
        /// Process the full set of subgrids in the request
        /// </summary>
        private void PerformSubgridRequests()
        {
            using (MS = new MemoryStream())
            {
                clientGrids = new IClientLeafSubGrid[addressBucketSize];

                // Scan through all the bitmap leaf subgrids, and for each, scan through all the subgrids as 
                // noted with the 'set' bits in the bitmask, processing only those that matter for this server

                Log.Info("Scanning subgrids in request");

                areaControlSet = AreaControlSet.Null();
                siteModel = SiteModels.SiteModels.Instance().GetSiteModel(localArg.SiteModelID);

                SubGridCellAddress[] addresses = new SubGridCellAddress[addressBucketSize];
                int listCount = 0;

                mask.ScanAllSetBitsAsSubGridAddresses(address =>
                {
                    if (address.ToSpatialDivisionDescriptor(numSpatialProcessingDivisions) == spatialSubdivisionDescriptor)
                    {
                        // This subgrid is the responsibility of this server
                        addresses[listCount++] = address;

                        if (listCount == addressBucketSize)
                        {
                            // Process the subgrids...
                            PerformSubgridRequestList(addresses, listCount);
                            listCount = 0;
                        }
                    }
                });

                if (listCount > 0)
                {
                    // Process the remaining subgrids...
                    PerformSubgridRequestList(addresses, listCount);
                }
            }
        }

        /// <summary>
        /// Invoke function called in the context of the cluster compute node
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public SubGridRequestsResponse Invoke(SubGridsRequestArgument arg)
        {
            Debug.Assert(Range.InRange(spatialSubdivisionDescriptor, 0, numSpatialProcessingDivisions),
                         String.Format("Invalid spatial sub division descriptor (must in be in range {0} -> [{1}, {2}])", spatialSubdivisionDescriptor, 0, numSpatialProcessingDivisions));

            Log.Info("In SubGridsRequestComputeFunc.invoke()");

            SubGridRequestsResponse result = new SubGridRequestsResponse();

            UnpackArgument(arg);

            result.NumSubgridsExamined = mask.CountBits();
            result.ResponseCode = SubGridRequestsResponseResult.Unknown;

            Log.Info(String.Format("Num subgrids present in request = {0} [All divisions]", result.NumSubgridsExamined));

            // TODO Perform implementation here and craft appropriate modified result

            ignite = Ignition.GetIgnite(RaptorGrids.RaptorGridName());
            group = ignite.GetCluster().ForAttribute("RaptorNodeID", raptorNodeIDAsString);

            Log.InfoFormat("Message group has {0} members", group.GetNodes().Count);

            rmtMsg = group.GetMessaging();

            PerformSubgridRequests();

            Log.Info("Out SubGridsRequestComputeFunc.invoke()");

            result.ResponseCode = SubGridRequestsResponseResult.OK;
            return result;
        }
    }
}
