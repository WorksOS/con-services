using System;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModels.GridFabric.Events;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;

namespace VSS.TRex.SiteModels.GridFabric.ComputeFuncs
{
  public class SiteModelAttributesChangedEventSenderComputeFunc : IComputeFunc<ISiteModelAttributesChangedEvent, ISiteModelAttributesChangedEventSenderResponse>
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SiteModelAttributesChangedEventSenderComputeFunc>();

    public ISiteModelAttributesChangedEventSenderResponse Invoke(ISiteModelAttributesChangedEvent arg)
    {
      var localNodeId = Guid.Empty;

      try
      {
        var siteModels = DIContext.Obtain<ISiteModels>();
        if (siteModels == null)
        {
          _log.LogError("No ISiteModels instance available from DIContext to send attributes change message to");

          return new SiteModelAttributesChangedEventSenderResponse {Success = false, NodeUid = localNodeId };
        }

        localNodeId = DIContext.ObtainRequired<ITRexGridFactory>().Grid(siteModels.PrimaryMutability).GetCluster().GetLocalNode().Id;

        _log.LogInformation(
          $"Received notification of site model attributes changed for {arg.SiteModelID}: ExistenceMapModified={arg.ExistenceMapModified}, DesignsModified={arg.DesignsModified}, SurveyedSurfacesModified {arg.SurveyedSurfacesModified} CsibModified={arg.CsibModified}, MachinesModified={arg.MachinesModified}, MachineTargetValuesModified={arg.MachineTargetValuesModified}, AlignmentsModified {arg.AlignmentsModified}, ExistenceMapChangeMask {arg.ExistenceMapChangeMask != null}");

        // Tell the SiteModels instance to reload the designated site model that has changed
        siteModels.SiteModelAttributesHaveChanged(arg);

        return new SiteModelAttributesChangedEventSenderResponse
        {
          Success = true,
          NodeUid = localNodeId
        };
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception occurred processing site model attributes changed event");

        return new SiteModelAttributesChangedEventSenderResponse
        {
          Success = false,
          NodeUid = localNodeId
        };
      }
      finally
      {
        _log.LogInformation(
          $"Completed handling notification of site model attributes changed for '{arg.SiteModelID}': ExistenceMapModified={arg.ExistenceMapModified}, DesignsModified={arg.DesignsModified}, SurveyedSurfacesModified {arg.SurveyedSurfacesModified} CsibModified={arg.CsibModified}, MachinesModified={arg.MachinesModified}, MachineTargetValuesModified={arg.MachineTargetValuesModified}, AlignmentsModified {arg.AlignmentsModified}, ExistenceMapChangeMask {arg.ExistenceMapChangeMask != null}");
      }
    }
  }
}
