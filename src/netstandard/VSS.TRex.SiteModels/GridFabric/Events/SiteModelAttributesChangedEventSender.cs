using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.SiteModels.GridFabric.Events
{
    /// <summary>
    /// Responsible for sending a notification that the attributes of a site model have changed
    /// By definition, all server and client nodes should react to this message
    /// </summary>
    public class SiteModelAttributesChangedEventSender : ISiteModelAttributesChangedEventSender
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger("SiteModelAttributesChangedEventSender");

        /// <summary>
        /// Notify all interested nodes in the immutable grid a site model has changed attributes
        /// </summary>
        /// <param name="siteModelID"></param>
        public void ModelAttributesChanged(Guid siteModelID)
        {
          try
          {
            DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Immutable)?.GetMessaging().Send
            (new SiteModelAttributesChangedEvent
            {
                SiteModelID = siteModelID
            });
          }
          catch (Exception e)
          {
            Log.LogDebug($"Exception occurred sending model attributes changed notification: {e}");
          }
        }
    }
}
