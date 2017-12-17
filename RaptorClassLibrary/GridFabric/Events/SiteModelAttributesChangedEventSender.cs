using Apache.Ignite.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Grids;

namespace VSS.VisionLink.Raptor.GridFabric.Events
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
        /// <param name="siteModelID"></param>
        public static void ModelAttributesChanged(long siteModelID)
        {
            // This full blown 'get the Ignite reference and messaging projection may not be very fast...
            // TODO: Add finer granularity to the event details (eg: project extents changed, surveyed surface list changed, design changed etc)
            Ignition.TryGetIgnite(RaptorGrids.RaptorGridName())?.GetMessaging().Send
                (new SiteModelAttributesChangedEvent()
                {
                    SiteModelID = siteModelID
                });
        }
    }
}
