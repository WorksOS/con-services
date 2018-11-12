using Apache.Ignite.Core;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Messaging;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.SubGrids.GridFabric.ComputeFuncs
{
    /// <summary>
    /// The base closure/function that implements subgrid request processing on compute nodes
    /// </summary>
    public class SubGridsRequestComputeFuncProgressive<TSubGridsRequestArgument, TSubGridRequestsResponse> : SubGridsRequestComputeFuncBase<TSubGridsRequestArgument, TSubGridRequestsResponse>
        where TSubGridsRequestArgument : SubGridsRequestArgument
        where TSubGridRequestsResponse : SubGridRequestsResponse, new()
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridsRequestComputeFuncProgressive<TSubGridsRequestArgument, TSubGridRequestsResponse>>();

        [NonSerialized]
        private IMessaging rmtMsg;

        [NonSerialized]
        private string tRexNodeIDAsString = string.Empty;

        [NonSerialized]
        private MemoryStream MS;

        [NonSerialized]
        private byte[] buffer;

        /// <summary>
        /// Capture elements from the argument relevant to progressive subgrid requests
        /// </summary>
        /// <param name="arg"></param>
        protected override void UnpackArgument(SubGridsRequestArgument arg)
        {
            base.UnpackArgument(arg);

            tRexNodeIDAsString = arg.TRexNodeID;

            MS = new MemoryStream();
            buffer = new byte[10000];

            Log.LogInformation($"TRexNodeIDAsString is {tRexNodeIDAsString} in UnpackArgument()");
        }

        /// <summary>
        /// Set up Ignite elements for progressive subgrid requests
        /// </summary>
        public override bool EstablishRequiredIgniteContext(out SubGridRequestsResponseResult ResponseCode)
        {
            ResponseCode = SubGridRequestsResponseResult.OK;

            IIgnite Ignite = Ignition.TryGetIgnite(TRexGrids.ImmutableGridName());
            IClusterGroup group = Ignite?.GetCluster().ForAttribute("TRexNodeId", tRexNodeIDAsString);

            if (group == null)
            {
                ResponseCode = SubGridRequestsResponseResult.NoIgniteGroupProjection;
                return false;
            }

            Log.LogInformation($"Message group has {group.GetNodes().Count} members");

            rmtMsg = group.GetMessaging();

            if (rmtMsg == null)
            {
                ResponseCode = SubGridRequestsResponseResult.NoIgniteGroupProjection;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Processes a subgrid result that consists of a client leaf subgrid for each of the filters in the request
        /// </summary>
        /// <param name="results"></param>
        /// <param name="resultCount"></param>
        public override void ProcessSubgridRequestResult(IClientLeafSubGrid[][] results, int resultCount)
        {
            // Package the resulting subgrids into the MemoryStream
            MS.Position = 0;

            using (BinaryWriter writer = new BinaryWriter(MS, Encoding.UTF8, true))
            {
                writer.Write(resultCount);

                for (int i = 0; i < resultCount; i++)
                {
                    writer.Write(results[i].Length);
                    foreach (IClientLeafSubGrid result in results[i])
                    {
                        writer.Write(result != null); 
                        result?.Write(writer, buffer);
                    }
                }
            }

            // ... and send it to the message topic in the compute func
            try
            {
                // Log.InfoFormat("Sending result to {0} ({1} receivers) - First = {2}/{3}", 
                //                localArg.MessageTopic, rmtMsg.ClusterGroup.GetNodes().Count, 
                //                rmtMsg.ClusterGroup.GetNodes().Where(x => x.GetAttributes().Where(a => a.Key.StartsWith(ServerRoles.ROLE_ATTRIBUTE_NAME)).Count() > 0).Aggregate("|", (s1, s2) => s1 + s2 + "|"),
                //                rmtMsg.ClusterGroup.GetNodes().First().GetAttribute<string>("TRexNodeId"));
                byte[] bytes = new byte[MS.Position];
                MS.Position = 0;
                MS.Read(bytes, 0, bytes.Length);
                rmtMsg.Send(bytes, localArg.MessageTopic);
            }
            catch (Exception e)
            {
                Log.LogError("Exception sending message", e);
                throw;
            }
        }

        /// <summary>
        /// Transforms the internal aggregation state into the desired response for the request
        /// </summary>
        /// <returns></returns>
        public override TSubGridRequestsResponse AcquireComputationResult()
        {
            return new TSubGridRequestsResponse();
        }

        /// <summary>
        /// Perform dispose activities necessary for the progressive subgrid request compute function
        /// </summary>
        protected override void DoDispose()
        {
            // Dispose the memory stream nicely
            MS?.Dispose();
        }
    }
}
