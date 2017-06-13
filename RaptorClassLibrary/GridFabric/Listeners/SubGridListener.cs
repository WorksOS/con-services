using Apache.Ignite.Core.Messaging;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Executors.Tasks.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.GridFabric.Listeners
{
    /// <summary>
    /// SubGridListener implements a listening post for subgrid results being sent by processing nodes back
    /// to the local context for further processing. Subgrids are sent as serialised streams held in memory streams
    /// to minimise serialization/deserialisation overhead
    /// </summary>
    [Serializable]
    public class SubGridListener : IMessageListener<MemoryStream>
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        // private static int responseCounter = 0;

        private static IClientLeafSubgridFactory ClientLeafSubGridFactory = ClientLeafSubgridFactoryFactory.GetClientLeafSubGridFactory();

        public ITask Task = null;

        /// <summary>
        /// The method called to announce the arrival of a message from a remote context in the cluster
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool Invoke(Guid nodeId, MemoryStream message)
        {
            try
            {
                // Decode the message into the appropriate client subgrid type
                IClientLeafSubGrid ClientGrid = ClientLeafSubGridFactory.GetSubGrid(Task.GridDataType);

                message.Position = 0;
                ClientGrid.Read(new BinaryReader(message));

                // Log.Info(String.Format("Transferring response#{0} to processor", ++responseCounter));

                // Send the decoded grid to the PipelinedTask, but ensure subgrids are serialised into the task
                // (no assumption of thread safety within the task itself)
                lock (Task)
                {
                    Task.TransferResponse(ClientGrid);
                }
            }
            catch ( Exception E )
            {
                throw;
            }


            return true;
        }

        public SubGridListener(ITask task)
        {
            Task = task;
        }
    }
}
