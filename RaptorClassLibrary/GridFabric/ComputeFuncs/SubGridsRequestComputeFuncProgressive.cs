using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Arguments;
using VSS.VisionLink.Raptor.GridFabric.Responses;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.GridFabric.ComputeFuncs
{
    /// <summary>
    /// The base closure/function that implements subgrid request processing on compute nodes
    /// </summary>
    [Serializable]
    public class SubGridsRequestComputeFuncProgressive<TSubGridsRequestArgument, TSubGridRequestsResponse> : SubGridsRequestComputeFuncBase<TSubGridsRequestArgument, TSubGridRequestsResponse>
        where TSubGridsRequestArgument : SubGridsRequestArgument, new()
        where TSubGridRequestsResponse : SubGridRequestsResponse, new()
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private MemoryStream MS = new MemoryStream();

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
                byte[] buffer = new byte[10000];

                writer.Write(resultCount);

                for (int i = 0; i < resultCount; i++)
                {
                    writer.Write(results[i].Length);
                    foreach (IClientLeafSubGrid result in results[i])
                    {
                        result.Write(writer, buffer);
                    }
                }
            }

            // ... and send it to the message topic in the compute func
            try
            {
                //// Log.InfoFormat("Sending result to {0} ({1} receivers) - First = {2}/{3}", 
                //                localArg.MessageTopic, rmtMsg.ClusterGroup.GetNodes().Count, 
                //                rmtMsg.ClusterGroup.GetNodes().First().GetAttribute<string>(ServerRoles.ROLE_ATTRIBUTE_NAME),
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
        /// Transforms the internal aggregation state into the desired response for the request
        /// </summary>
        /// <param name="results"></param>
        /// <param name="resultCount"></param>
        /// <returns></returns>
        public override TSubGridRequestsResponse AcquireComputationResult()
        {
            return new TSubGridRequestsResponse();
        }

        /// <summary>
        /// Perform dispose activities necessary for the progressive subgrid request compute function
        /// </summary>
        public override void DoDispose()
        {
            // Diospose the memory stream nicely
            MS?.Dispose();
        }
    }
}
