using Apache.Ignite.Core;
using System;

namespace VSS.TRex.SiteModels.GridFabric.Events
{
    /// <summary>
    /// Responsible for sending a notification that the attributes of a site model have changed
    /// By definition, all server and client nodes should react to this message
    /// </summary>
    public static class SiteModelAttributesChangedEventSender
    {
        /// <summary>
        /// Notify all nodes in the grid a site model has changed attributes
        /// </summary>
        /// <param name="ignite"></param>
        /// <param name="siteModelID"></param>
        public static void ModelAttributesChanged(IIgnite ignite, Guid siteModelID)
        {
            ignite?.GetMessaging().Send(new SiteModelAttributesChangedEvent
            {
                SiteModelID = siteModelID
            });
        }
    }
}
