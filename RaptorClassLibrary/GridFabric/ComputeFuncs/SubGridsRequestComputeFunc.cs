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

namespace VSS.VisionLink.Raptor.GridFabric.ComputeFuncs
{
    /// <summary>
    /// The closure/function that implements subgrid request processing on compute nodes
    /// </summary>
    [Serializable]
    class SubGridsRequestComputeFunc : IComputeFunc<SubGridsRequestArgument, SubGridRequestsResponse>
    {
        private static IClientLeafSubgridFactory ClientLeafSubGridFactory = ClientLeafSubgridFactoryFactory.GetClientLeafSubGridFactory();
        private static int requestCount = 0;

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

        /// <summary>
        /// Take the supplied argument to the compute func and perform any necessary unpacking of the
        /// contents of it into a form ready to use. Also make a location reference to the arg parameter
        /// to allow other methods to access it as local state.
        /// </summary>
        /// <param name="arg"></param>
        private void UnpackArgument(SubGridsRequestArgument arg)
        {
            localArg = arg;

            // Unpack the mask from the argument
            mask = new SubGridTreeBitMask();
            arg.MaskStream.Position = 0;
            SubGridTreePersistor.Read(mask, new BinaryReader(arg.MaskStream));
        }

        /// <summary>
        /// Take a subgrid address and request the required client subgrid depending on GridDataType
        /// </summary>
        /// <param name="address"></param>
        private void PerformSubgridRequest(SubGridCellAddress address)
        {
            try
            {
//                Console.WriteLine("PerformSubgridRequest #{0}", ++requestCount);

                AreaControlSet AreaControlSet = AreaControlSet.Null();

                SiteModel SiteModel = SiteModels.Instance().GetSiteModel(localArg.SiteModelID);

                IClientLeafSubGrid ClientGrid = ClientLeafSubGridFactory.GetSubGrid(localArg.GridDataType);
                ClientGrid.CellSize = SiteModel.Grid.CellSize;

                // Reach into the subgrid request layer and retrieve an appropriate subgrid
                ServerRequestResult result = SubGridRequestor.RequestSubGridInternal
                   (null, // Try it with a null filter to start
                    int.MaxValue, // MaxCellPasses
                    false, // Override cell restriction
                    BoundingIntegerExtent2D.Inverted(), // Override cell restriction
                    SiteModel,
                    address,
                    SubGridTree.SubGridTreeLevels,
                    true, // Want production data
                    false, // Dont want surveyed surface data
                    ClientGrid,
                    SubGridTreeBitmapSubGridBits.FullMask,
                    ref AreaControlSet
                    );

                if (result == ServerRequestResult.NoError)
                {
                    // Package the resulting subgrid in to a MemoryStream
                    MemoryStream MS = new MemoryStream();
                    ClientGrid.Write(new BinaryWriter(MS));
                    // MS.Position = 0;

                    // .. and send it to the message topic in the compute func
                    IIgnite ignite = Ignition.GetIgnite("Raptor");

                    //var rmtMsg = ignite.GetCluster().ForRemotes().GetMessaging();
                    IMessaging rmtMsg = ignite.GetCompute().ClusterGroup.GetMessaging();

                    try
                    {
                        rmtMsg.Send(MS, localArg.MessageTopic);
                    }
                    catch (Exception E)
                    {
                        throw;
                    }
                }
                else
                {
                    //throw new ArgumentException(String.Format("Subgrid request failed with code {0}", result));
                }
            }
            catch (Exception E)
            {
                throw;
            }
        }

        /// <summary>
        /// Invoke function called in the context of the cluster compute node
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public SubGridRequestsResponse Invoke(SubGridsRequestArgument arg)
        {
            SubGridRequestsResponse result = new SubGridRequestsResponse();

            UnpackArgument(arg);

            result.NumSubgridsExamined = mask.CountBits();
            result.ResponseCode = SubGridRequestsResponseResult.NotImplemented;

            Console.WriteLine("result.NumSubgridsExamined = {0}", result.NumSubgridsExamined);

            // TODO Perform implementation here and craft appropriate modified result

            // For now, pretend this context cares about all the subgrids...
            // When we do care, the lambda function below needs to filter on the affinity predicate
            // Scan through all the bitmap leaf subgrids, and for each, scan through all the subgrids as 
            // noted with the 'set' bits in the bitmask
            mask.ScanAllSetBitsAsSubGridAddresses(PerformSubgridRequest);
        
            return result;
        }
    }
}
