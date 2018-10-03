using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Interfaces;


namespace VSS.TRex.SiteModels.GridFabric.Events
{
  /// <summary>
  /// Responsible for sending a notification that the attributes of a site model have changed
  /// By definition, all server and client nodes should react to this message
  /// </summary>
  public class SiteModelAttributesChangedEventSender : ISiteModelAttributesChangedEventSender
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger("SiteModelAttributesChangedEventSender");

    private const string MessageTopicName = "SiteModelAttributesChangedEvents";

    /// <summary>
    /// Notify all interested nodes in the immutable grid a site model has changed attributes
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="existenceMapChanged"></param>
    /// <param name="existenceMapChangeMask"></param>
    /// <param name="designsChanged"></param>
    /// <param name="surveyedSurfacesChanged"></param>
    /// <param name="machinesChanged"></param>
    /// <param name="machineTargetValuesChanged"></param>
    public void ModelAttributesChanged(SiteModelNotificationEventGridMutability targetGrids,
      Guid siteModelID,
      bool existenceMapChanged = false,
      ISubGridTreeBitMask existenceMapChangeMask = null,
      bool designsChanged = false,
      bool surveyedSurfacesChanged = false,
      bool machinesChanged = false,
      bool machineTargetValuesChanged = false)
    {
      ModelAttributesChanged(targetGrids, siteModelID, existenceMapChanged, existenceMapChangeMask, designsChanged, surveyedSurfacesChanged, false, machinesChanged, machineTargetValuesChanged);
    }

    /// <summary>
    /// Notify all interested nodes in the immutable grid a site model has changed attributes
    /// </summary>
    /// <param name="targetGrids"></param>
    /// <param name="siteModelID"></param>
    /// <param name="existenceMapChanged"></param>
    /// <param name="existenceMapChangeMask"></param>
    /// <param name="designsChanged"></param>
    /// <param name="csibChanged"></param>
    /// <param name="machinesChanged"></param>
    /// <param name="machineTargetValuesChanged"></param>
    /// <param name="surveyedSurfacesChanged"></param>
    public void ModelAttributesChanged(SiteModelNotificationEventGridMutability targetGrids,
      Guid siteModelID,
      bool existenceMapChanged = false,
      ISubGridTreeBitMask existenceMapChangeMask = null,
      bool designsChanged = false,
      bool surveyedSurfacesChanged = false,
      bool csibChanged = false,
      bool machinesChanged = false,
      bool machineTargetValuesChanged = false)
    {
      try
      {
        var gridFactory = DIContext.Obtain<ITRexGridFactory>();
        var evt = new SiteModelAttributesChangedEvent
        {
          SiteModelID = siteModelID,
          ExistenceMapModified = existenceMapChanged,
          ExistenceMapChangeMask = existenceMapChangeMask?.ToBytes(),
          CsibModified = csibChanged,
          DesignsModified = designsChanged,
          SurveyedSurfacesModified = surveyedSurfacesChanged,
          MachinesModified = machinesChanged,
          MachineTargetValuesModified = machineTargetValuesChanged
        };

        if ((targetGrids & SiteModelNotificationEventGridMutability.NotifyImmutable) != 0)
          gridFactory.Grid(StorageMutability.Immutable).GetMessaging().SendOrdered(evt, MessageTopicName);

        if ((targetGrids & SiteModelNotificationEventGridMutability.NotifyMutable) != 0)
          gridFactory.Grid(StorageMutability.Mutable).GetMessaging().SendOrdered(evt, MessageTopicName);
      }
      catch (Exception e)
      {
        Log.LogError($"Exception occurred sending model attributes changed notification: {e}");
      }
    }
  }
}
