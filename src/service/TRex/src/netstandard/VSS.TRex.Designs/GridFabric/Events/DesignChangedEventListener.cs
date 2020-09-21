using System;
using Microsoft.Extensions.Logging;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Messaging;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Common;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Designs.GridFabric.Events
{
  /// <summary>
  /// The listener that responds to design change notifications emitted by actions such as changing a design
  /// </summary>
  public class DesignChangedEventListener : VersionCheckedBinarizableSerializationBase, IMessageListener<IDesignChangedEvent>, IDisposable, IDesignChangedEventListener
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<DesignChangedEventListener>();

    private const byte VERSION_NUMBER = 1;

    public const string DESIGN_CHANGED_EVENTS_TOPIC_NAME = "DesignStateChangedEvents";

    public string MessageTopicName { get; set; } = DESIGN_CHANGED_EVENTS_TOPIC_NAME;

    public string GridName { get; private set; }

    public bool Invoke(Guid nodeId, IDesignChangedEvent message)
    {
      try
      {
        _log.LogInformation(
          $"Received notification of design changed for site:{message.SiteModelUid}, design:{message.DesignUid}, DesignType:{message.FileType}, DesignRemoved:{message.DesignRemoved}, ImportedFileType:{message.FileType}");

        var designFiles = DIContext.ObtainOptional<IDesignFiles>();

        if (designFiles == null)
        {
          // No cache, leave early...
          return true;
        }

        var siteModel = DIContext.ObtainRequired<ISiteModels>().GetSiteModel(message.SiteModelUid);
        if (siteModel == null)
        {
          _log.LogWarning($"No site model found for ID {message.SiteModelUid}");
          return true;
        }

        designFiles.DesignChangedEventHandler(message.DesignUid, siteModel, message.FileType);
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception occurred processing design changed event");
        return true; // Stay subscribed
      }
      finally
      {
        _log.LogInformation(
          $"Completed handling notification of design changed for Site:{message.SiteModelUid}, Design:{message.DesignUid}, DesignRemoved:{message.DesignRemoved}, ImportedFileType:{message.FileType}");
      }

      return true;
    }

    public DesignChangedEventListener()
    {
    }

    /// <summary>
    /// Constructor taking the name of the grid to install the message listener into
    /// </summary>
    public DesignChangedEventListener(string gridName)
    {
      GridName = gridName;
    }

    public void StartListening()
    {
      _log.LogInformation($"Start listening for design state notification events on {MessageTopicName}");

      // Create a messaging group the cluster can use to send messages back to and establish a local listener
      // All nodes (client and server) want to know about design state change
      var msgGroup = DIContext.Obtain<ITRexGridFactory>()?.Grid(GridName)?.GetCluster().GetMessaging();

      if (msgGroup != null)
        msgGroup.LocalListen(this, MessageTopicName);
      else
        _log.LogError("Unable to get messaging projection to add design state change event to");
    }

    public void StopListening()
    {
      // Un-register the listener from the message group
      DIContext.Obtain<ITRexGridFactory>()?.Grid(GridName)?.GetCluster().GetMessaging()?.StopLocalListen(this, MessageTopicName);
    }

    public void Dispose()
    {
      StopListening();
    }

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);
      writer.WriteString(GridName);
      writer.WriteString(MessageTopicName);
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        GridName = reader.ReadString();
        MessageTopicName = reader.ReadString();
      }
    }
  }

}
