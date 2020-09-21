using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.SiteModels.GridFabric.ComputeFuncs;
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
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SiteModelAttributesChangedEventSender>();

    // ReSharper disable once IdentifierTypo
    private const int DEFAULT_TREX_IGNITE_ORDERED_MESSAGE_SEND_TIMEOUT_SECONDS = 30;

    /// <summary>
    /// Set the message timeout for the ordered messages being sent to 30 seconds. By default this is
    /// set to the network timeout (which is 5 seconds by default).
    /// </summary>
    private static readonly TimeSpan _messageSendTimeout = new TimeSpan(0, 0, (int)DIContext.Obtain<IConfigurationStore>().GetValueUint("TREX_IGNITE_ORDERED_MESSAGE_SEND_TIMEOUT", DEFAULT_TREX_IGNITE_ORDERED_MESSAGE_SEND_TIMEOUT_SECONDS));

    private const string MESSAGE_TOPIC_NAME = "SiteModelAttributesChangedEvents";

    private readonly SiteModelAttributesChangedEventSenderComputeFunc _messageSendComputeFunc = new SiteModelAttributesChangedEventSenderComputeFunc();

    private static readonly string _messageRoleAttributeName = $"Role-{ServerRoles.RECEIVES_SITEMODEL_CHANGE_EVENTS}";

    /// <summary>
    /// Sends the message notification schema using the Ignite compute/invoke pattern as a reliable delivery mechanism
    /// </summary>
    private void SendInvokeStyleMessage(string gridName, ISiteModelAttributesChangedEvent evt)
    {
      var gridFactory = DIContext.Obtain<ITRexGridFactory>();

      var compute = gridFactory.Grid(StorageMutability.Immutable)?.GetCluster()?.ForAttribute(_messageRoleAttributeName, "True")?.GetCompute();
      if (compute != null)
      {
        var responses = compute.Broadcast(_messageSendComputeFunc, evt);

        if (responses == null || responses.Count == 0)
        {
          _log.LogWarning($"Site model change notification responses collection from {gridName} is null or empty");
        }
        else
        {
          _log.LogInformation($"Received notification confirmation for {responses.Count} recipients in the {gridName} grid.");
          if (responses.Any(x => !x.Success))
          {
            _log.LogWarning($"Not all targeted nodes in {gridName} successfully processed message, Failures = {string.Join(',', responses.Where(x => !x.Success).Select(x => x.NodeUid))}");
          }
        }
      }
    }

    /// <summary>
    /// Notify all interested nodes in the immutable grid a site model has changed attributes
    /// </summary>
    public void ModelAttributesChanged(SiteModelNotificationEventGridMutability targetGrids,
      Guid siteModelID,
      bool existenceMapChanged = false,
      ISubGridTreeBitMask existenceMapChangeMask = null,
      bool designsChanged = false,
      bool surveyedSurfacesChanged = false,
      bool csibChanged = false,
      bool machinesChanged = false,
      bool machineTargetValuesChanged = false,
      bool machineDesignsModified = false,
      bool proofingRunsModified = false,
      bool alignmentsChanged = false,
      bool siteModelMarkedForDeletion = false)
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
        MachineTargetValuesModified = machineTargetValuesChanged,
        MachineDesignsModified = machineDesignsModified,
        ProofingRunsModified = proofingRunsModified,
        AlignmentsModified = alignmentsChanged,
        SiteModelMarkedForDeletion = siteModelMarkedForDeletion,
        ChangeEventUid = Guid.NewGuid(),
        TimeSentUtc = DateTime.UtcNow
      };

      /*
      if ((targetGrids & SiteModelNotificationEventGridMutability.NotifyImmutable) != 0)
        gridFactory.Grid(StorageMutability.Immutable).GetMessaging().SendOrdered(evt, MESSAGE_TOPIC_NAME, _messageSendTimeout);

      if ((targetGrids & SiteModelNotificationEventGridMutability.NotifyMutable) != 0)
        gridFactory.Grid(StorageMutability.Mutable).GetMessaging().SendOrdered(evt, MESSAGE_TOPIC_NAME, _messageSendTimeout);
      */

      if ((targetGrids & SiteModelNotificationEventGridMutability.NotifyImmutable) != 0)
      {
        evt.SourceNodeUid = gridFactory.Grid(StorageMutability.Immutable).GetCluster().GetLocalNode().Id;
        SendInvokeStyleMessage("Immutable", evt);
      }

      if ((targetGrids & SiteModelNotificationEventGridMutability.NotifyMutable) != 0)
      {
        evt.SourceNodeUid = gridFactory.Grid(StorageMutability.Mutable).GetCluster().GetLocalNode().Id;
        SendInvokeStyleMessage("Mutable", evt);
      }
    }
  }
}
