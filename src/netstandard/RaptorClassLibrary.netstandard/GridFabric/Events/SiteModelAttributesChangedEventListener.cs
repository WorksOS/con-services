﻿using Apache.Ignite.Core;
using Apache.Ignite.Core.Messaging;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace VSS.TRex.GridFabric.Events
{
    public class SiteModelAttributesChangedEventListener : IMessageListener<SiteModelAttributesChangedEvent>, IDisposable
    {
        [NonSerialized]
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        ///  Message group the listener has been added to
        /// </summary>
        [NonSerialized] private IMessaging MsgGroup;

        [NonSerialized]
        private string MessageTopicName = "SiteModelAttributesChangedEventListener";

        [NonSerialized] private string GridName;

        public bool Invoke(Guid nodeId, SiteModelAttributesChangedEvent message)
        {
            // Tell the SiteModels instance to reload the designated site model that has changed
            try
            {
                SiteModels.SiteModels.Instance().SiteModelAttributesHaveChanged(message.SiteModelID);
            }
            catch (Exception E)
            {
                Log.LogError($"Exception in SiteModelAttributesChangedEventListener.Invvoke:\n{E}");
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

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SiteModelAttributesChangedEventListener() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
