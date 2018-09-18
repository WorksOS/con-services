using Apache.Ignite.Core;
using Apache.Ignite.Core.Messaging;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;

namespace VSS.TRex.SiteModels.GridFabric.Events
{
    /// <summary>
    /// The listener that responds to site model change notifications emiited by actors such as TAG file processing
    /// </summary>
    [Serializable]
    public class SiteModelAttributesChangedEventListener : IMessageListener<ISiteModelAttributesChangedEvent>, IDisposable, ISiteModelAttributesChangedEventListener
    {
        [NonSerialized]
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        ///  Message group the listener has been added to
        /// </summary>
        [NonSerialized]
        private IMessaging MsgGroup;

        [NonSerialized]
        private string MessageTopicName = "SiteModelAttributesChangedEvents";

        [NonSerialized] private string GridName;

        public bool Invoke(Guid nodeId, ISiteModelAttributesChangedEvent message)
        {
            Log.LogInformation($"Received notification site model attributes changed for {message.SiteModelID}");

            // Tell the SiteModels instance to reload the designated site model that has changed
            try
            {
                DIContext.Obtain<ISiteModels>().SiteModelAttributesHaveChanged(message.SiteModelID);
            }
            catch (Exception E)
            {
                Log.LogError($"Exception in SiteModelAttributesChangedEventListener.Invoke:\n{E}");
            }

            return true;
        }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SiteModelAttributesChangedEventListener(string gridName)
        {
            GridName = gridName;
        }

        /// <summary>
        /// Constructor accepting an override for the default message topi cname used for site model
        /// attribute changed messages
        /// </summary>
        /// <param name="gridName"></param>
        /// <param name="messageTopicName"></param>
        public SiteModelAttributesChangedEventListener(string gridName, string messageTopicName) : this(gridName)
        {
            MessageTopicName = messageTopicName;
        }

        public void StartListening()
        {
            Log.LogError($"Start listening for site model notification events on {MessageTopicName}");

            // Create a messaging group the cluster can use to send messages back to and establish a local listener
            // All nodes (client and server) want to know about site model attribute changes
            MsgGroup = Ignition.TryGetIgnite(GridName)?.GetCluster().GetMessaging();

            if (MsgGroup != null)
            {
              MsgGroup.LocalListen(this, MessageTopicName);
            }
            else
            {
                Log.LogError("Unable to get messaging projection to add site model attribute changed event to");
            }
        }

        public void StopListening()
        {
            // Unregister the listener from the message group
            MsgGroup?.StopLocalListen(this);

            MsgGroup = null;
        }

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    StopListening();
                }

                disposedValue = true;
            }
        }

        // ...Override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SiteModelAttributesChangedEventListener() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // ...uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
