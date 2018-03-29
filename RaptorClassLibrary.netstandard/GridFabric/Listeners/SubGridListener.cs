using Apache.Ignite.Core.Messaging;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Executors.Tasks.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.GridFabric.Listeners
{
    /// <summary>
    /// SubGridListener implements a listening post for subgrid results being sent by processing nodes back
    /// to the local context for further processing when using a proressive style of subgrid requesting. 
    /// Subgrids are sent in groups as serialised streams held in memory streams to minimise serialization/deserialisation overhead
    /// </summary>
    [Serializable]
    public class SubGridListener : IMessageListener<byte[]>
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Count of the number of responses recieved by this listener
        /// </summary>
        [NonSerialized]
        private int responseCounter = 0;

        [NonSerialized]
        private static IClientLeafSubgridFactory ClientLeafSubGridFactory = ClientLeafSubgridFactoryFactory.GetClientLeafSubGridFactory();

        /// <summary>
        /// The reference to the task responsible for handling the returned subgrid information from the processing cluster
        /// </summary>
        [NonSerialized]
        public ITask Task = null;

        /// <summary>
        /// The memory stream to be used for deserialising message packets when they arrive
        /// </summary>
        [NonSerialized]
        [ThreadStatic]
        private static MemoryStream MS = null; //= new MemoryStream();

        /// <summary>
        /// Processes a response containing a set of subgrids from the subgrid processor for a request
        /// </summary>
        /// <param name="message"></param>
        private void ProcessResponse(byte[] message)
        {
            try
            {
                if (MS == null)
                {
                    MS = new MemoryStream();
                }

                // Decode the message into the appropriate client subgrid type
                MS.Position = 0;
                MS.Write(message, 0, message.Length);
                MS.Position = 0;

                using (BinaryReader reader = new BinaryReader(MS, Encoding.UTF8, true))
                {
                    // Read the number of subgrid present in the stream
                    int responseCount = reader.ReadInt32();

                    // Create a single instance of the client grid. The approach here is that TransferResponse does not move ownership 
                    // to the called context (it may clone the passed in client grid if desired)
                    IClientLeafSubGrid [][] clientGrids = new IClientLeafSubGrid[responseCount][];

                    try
                    {
                        byte[] buffer = new byte[10000];

                        for (int i = 0; i < responseCount; i++)
                        {
                            int subgridCount = reader.ReadInt32();
                            clientGrids[i] = new IClientLeafSubGrid[subgridCount];

                            for (int j = 0; j < subgridCount; j++)
                            {
                                clientGrids[i][j] = ClientLeafSubGridFactory.GetSubGrid(Task.GridDataType);
                                clientGrids[i][j].Read(reader, buffer);
                            }

                            int thisResponseCount = ++responseCounter;

                            // Log.InfoFormat("Transferring response#{0} to processor (from thread {1})", thisResponseCount, System.Threading.Thread.CurrentThread.ManagedThreadId);

                            // Send the decoded grid to the PipelinedTask, but ensure subgrids are serialised into the task
                            // (no assumption of thread safety within the task itself)
                            try
                            {
                                lock (Task)
                                {
                                    if (Task.TransferResponse(clientGrids[i]))
                                    {
                                        // Log.DebugFormat("Processed response#{0} (from thread {1})", thisResponseCount, System.Threading.Thread.CurrentThread.ManagedThreadId);
                                    }
                                    else
                                    {
                                        Log.InfoFormat("Processing response#{0} FAILED (from thread {1})", thisResponseCount, System.Threading.Thread.CurrentThread.ManagedThreadId);
                                    }
                                }
                            }
                            finally
                            {
                                // Tell the pipeline that a subgrid has been completely processed
                                Task.PipeLine.SubgridProcessed();
                            }
                        }
                    }
                    finally
                    {
                        // Return the client grid to the factory for recycling now its role is complete here... when using ConcurrentBag
                        // ClientLeafSubGridFactory.ReturnClientSubGrid(ref clientGrid);

                        // Return the client grid to the factory for recycling now its role is complete here... when using SimpleConcurrentBag
                        ClientLeafSubGridFactory.ReturnClientSubGrids(clientGrids, responseCount);
                    }
                }
            }
            catch // (Exception E)
            {
                throw;
            }
        }

        /// <summary>
        /// The method called to announce the arrival of a message from a remote context in the cluster
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool Invoke(Guid nodeId, byte[] message)
        {
            ProcessResponse(message);

            return true;
        }

        /// <summary>
        /// Constructor accepting a task to pass subgrids into
        /// </summary>
        /// <param name="task"></param>
        public SubGridListener(ITask task)
        {
            Task = task;
       }
    }
}
