using Apache.Ignite.Core.Messaging;
using Microsoft.Extensions.Logging;
using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;

namespace VSS.TRex.SiteModels.GridFabric.Events
{
  /// <summary>
  /// The listener that responds to site model change notifications emitted by actors such as TAG file processing
  /// </summary>
  public class SiteModelAttributesChangedEventListener : IMessageListener<ISiteModelAttributesChangedEvent>, IDisposable, ISiteModelAttributesChangedEventListener, IBinarizable, IFromToBinary
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SiteModelAttributesChangedEventListener>();

    private const byte VERSION_NUMBER = 1;

    public const string SITE_MODEL_ATTRIBUTES_CHANGED_EVENTS_TOPIC_NAME = "SiteModelAttributesChangedEvents";

    /// <summary>
    ///  Message group the listener has been added to
    /// </summary>
    private IMessaging MsgGroup;

    public string MessageTopicName { get; set; } = SITE_MODEL_ATTRIBUTES_CHANGED_EVENTS_TOPIC_NAME;

    public string GridName { get; private set; }

    public bool Invoke(Guid nodeId, ISiteModelAttributesChangedEvent message)
    {
      try
      {
        Log.LogInformation(
          $"Received notification of site model attributes changed for {message.SiteModelID}: ExistenceMapModified={message.ExistenceMapModified}, DesignsModified={message.DesignsModified}, SurveyedSurfacesModified {message.SurveyedSurfacesModified} CsibModified={message.CsibModified}, MachinesModified={message.MachinesModified}, MachineTargetValuesModified={message.MachineTargetValuesModified}, AlignmentsModified {message.AlignmentsModified}, ExistenceMapChangeMask {message.ExistenceMapChangeMask != null}");

        // Tell the SiteModels instance to reload the designated site model that has changed
        var siteModels = DIContext.Obtain<ISiteModels>();
        if (siteModels != null)
        {
          siteModels.SiteModelAttributesHaveChanged(message);
        }
        else
        {
          Log.LogError("No ISiteModels instance available from DIContext to send attributes change message to");
          return false;
        }
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception occured processing site model attributes changed event");
        return false;
      }
      finally
      {
        Log.LogInformation(
          $"Completed handling notification of site model attributes changed for {message.SiteModelID}: ExistenceMapModified={message.ExistenceMapModified}, DesignsModified={message.DesignsModified}, SurveyedSurfacesModified {message.SurveyedSurfacesModified} CsibModified={message.CsibModified}, MachinesModified={message.MachinesModified}, MachineTargetValuesModified={message.MachineTargetValuesModified}, AlignmentsModified {message.AlignmentsModified}, ExistenceMapChangeMask {message.ExistenceMapChangeMask != null}");
      }

      return true;
    }

    public SiteModelAttributesChangedEventListener()
    {
    }

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
    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    /// <summary>
    /// Listener has no serializable content
    /// </summary>
    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

    public void ToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteString(GridName);
      writer.WriteString(MessageTopicName);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      GridName = reader.ReadString();
      MessageTopicName = reader.ReadString();
    }
  }
}
