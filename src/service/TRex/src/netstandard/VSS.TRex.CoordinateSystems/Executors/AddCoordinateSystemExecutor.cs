using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;

namespace VSS.TRex.CoordinateSystems.Executors
{
  /// <summary>
  /// Contains the business logic for adding a coordinate system to a project
  /// </summary>
  public class AddCoordinateSystemExecutor
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<AddCoordinateSystemExecutor>();

    /// <summary>
    /// Adds the given coordinate system to the identified project by placing the coordinate system
    /// into the mutable non spatial cache for the project. This will then be propagated to the immutable
    /// non spatial cache for the project
    /// Additionally, it notifies listeners of the coordinate system change.
    /// </summary>
    // ReSharper disable once IdentifierTypo
    public bool Execute(Guid projectUid, string csib)
    {
      // todo: Enrich return value to encode or provide additional information relating to failures

      try
      {
        // Tell the SiteModels instance to reload the designated site model that has changed
        var siteModels = DIContext.Obtain<ISiteModels>();
        if (siteModels == null)
        {
          _log.LogError("No ISiteModels instance available from DIContext to send attributes change message to");
          return false;
        }

        var siteModel = siteModels.GetSiteModel(projectUid, true);

        if (siteModel == null)
        {
          _log.LogError($"Failed to obtain site model for UID = {projectUid}");
          return false;
        }

        if (siteModel.SetCSIB(csib))
        {
          // Notify the  grid listeners that attributes of this site model have changed.
          var sender = DIContext.ObtainRequired<ISiteModelAttributesChangedEventSender>();
          sender.ModelAttributesChanged(SiteModelNotificationEventGridMutability.NotifyAll, siteModel.ID, CsibChanged: true);
        }
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception occurred adding coordinate system to project");
        throw;
      }

      return true;
    }
  }
}
