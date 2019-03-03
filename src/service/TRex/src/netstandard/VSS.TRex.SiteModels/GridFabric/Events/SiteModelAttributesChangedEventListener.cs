using Apache.Ignite.Core.Messaging;
using Microsoft.Extensions.Logging;
using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;

namespace VSS.TRex.SiteModels.GridFabric.Events
{
    /// <summary>
    /// The listener that responds to site model change notifications emitted by actors such as TAG file processing
    /// </summary>
    public class SiteModelAttributesChangedEventListener : IMessageListener<ISiteModelAttributesChangedEvent>, IDisposable, ISiteModelAttributesChangedEventListener, IBinarizable
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<SiteModelAttributesChangedEventListener>();

        /// <summary>
        ///  Message group the listener has been added to
        /// </summary>
        private IMessaging MsgGroup;

        public string MessageTopicName { get; set; } = "SiteModelAttributesChangedEvents";

        public string GridName { get; set; }

        public bool Invoke(Guid nodeId, ISiteModelAttributesChangedEvent message)
        {
            Log.LogInformation($"Received notification site model attributes changed for {message.SiteModelID}: ExistenceMapModified={message.ExistenceMapModified}, DesignsModified={message.DesignsModified}, SurveyedSurfacesModified {message.SurveyedSurfacesModified} CsibModified={message.CsibModified}, MachinesModified={message.MachinesModified}, MachineTargetValuesModified={message.MachineTargetValuesModified}, AlignmentsModified {message.AlignmentsModified}, ExistenceMapChangeMask {message.ExistenceMapChangeMask != null}");

            // Tell the SiteModels instance to reload the designated site model that has changed
            try
            {
                DIContext.Obtain<ISiteModels>().SiteModelAttributesHaveChanged(message.SiteModelID, message);
            }
            catch (Exception E)
            {
                Log.LogError(E, "Exception in SiteModelAttributesChangedEventListener.Invoke:");
            }

            return true;
        }

        public SiteModelAttributesChangedEventListener() { }

        /// <summary>
        /// Constructor taking the name of the grid to install the message listener into
        /// </summary>
        public SiteModelAttributesChangedEventListener(string gridName)
        {
            GridName = gridName;
        }

        public void StartListening()
        {
            Log.LogInformation($"Start listening for site model notification events on {MessageTopicName}");

            // Create a messaging group the cluster can use to send messages back to and establish a local listener
            // All nodes (client and server) want to know about site model attribute changes
            MsgGroup = DIContext.Obtain<ITRexGridFactory>()?.Grid(GridName)?.GetCluster().GetMessaging();

            if (MsgGroup != null)
                MsgGroup.LocalListen(this, MessageTopicName);
            else
                Log.LogError("Unable to get messaging projection to add site model attribute changed event to");
        }

        public void StopListening()
        {
            // Unregister the listener from the message group
            MsgGroup?.StopLocalListen(this);

            MsgGroup = null;
        }

        public void Dispose()
        {
            StopListening();
        }

      /// <summary>
      /// Listener has no serializable content
      /// </summary>
      /// <param name="writer"></param>
      public void WriteBinary(IBinaryWriter writer)
      {
      }

      /// <summary>
      /// Listener has no serializable content
      /// </summary>
      public void ReadBinary(IBinaryReader reader)
      {
      }
    }
}
