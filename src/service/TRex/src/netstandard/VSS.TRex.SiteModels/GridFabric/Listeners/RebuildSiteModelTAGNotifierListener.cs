using Apache.Ignite.Core.Messaging;
using Microsoft.Extensions.Logging;
using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModels.Interfaces.Listeners;
using VSS.TRex.SiteModels.Interfaces.Executors;
using System.Threading.Tasks;

namespace VSS.TRex.SiteModels.GridFabric.Listeners
{
  public class RebuildSiteModelTAGNotifierListener : IMessageListener<IRebuildSiteModelTAGNotifierEvent>, IDisposable, IRebuildSiteModelTAGNotifierListener, IBinarizable, IFromToBinary
  {
    /// <summary>
    /// The listener that responds to TAG file processing notifications for the project rebuilder
    /// </summary>
    private static readonly ILogger _log = Logging.Logger.CreateLogger<RebuildSiteModelTAGNotifierListener>();

    private const byte VERSION_NUMBER = 1;

    public const string SITE_MODEL_REBUILDER_TAG_FILE_PROCESSED_EVENT_TOPIC_NAME = "RebuilderTAGNotifierEvent";

    /// <summary>
    ///  Message group the listener has been added to
    /// </summary>
    private IMessaging MsgGroup;

    public string MessageTopicName { get; set; } = SITE_MODEL_REBUILDER_TAG_FILE_PROCESSED_EVENT_TOPIC_NAME;

    public bool Invoke(Guid nodeId, IRebuildSiteModelTAGNotifierEvent message)
    {
      try
      {
        _log.LogInformation($"Received notification of TAG file processing for {message.ProjectUid}, #TAG files = {message.ResponseItems.Length}");

        // Tell the rebuilder manager instance about the notification
        var rebuilderManager = DIContext.Obtain<ISiteModelRebuilderManager>();
        if (rebuilderManager != null)
        {
          Task.Run(() => rebuilderManager.TAGFileProcessed(message.ProjectUid, message.ResponseItems));
        }
        else
        {
          _log.LogError("No ISiteModelRebuilderManager instance available from DIContext to send TAg file processing notification to");
          return false;
        }
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception occured processing site model attributes changed event");
        return false;
      }
      finally
      {
        _log.LogInformation($"Completed handling of notification of TAG file processing for {message.ProjectUid}, #TAG files = {message.ResponseItems.Length}");
      }

      return true;
    }

    public RebuildSiteModelTAGNotifierListener()
    {
    }

    public void StartListening()
    {
      _log.LogInformation($"Start listening for TAG file processing events on {MessageTopicName}");

      // Create a messaging group the cluster can use to send messages back to and establish a local listener
      // All nodes (client and server) want to know about TAG file processing notifications
      MsgGroup = DIContext.Obtain<ITRexGridFactory>()?.Grid(Storage.Models.StorageMutability.Mutable)?.GetCluster().GetMessaging();

      if (MsgGroup != null)
        MsgGroup.LocalListen(this, MessageTopicName);
      else
        _log.LogError("Unable to get messaging projection to add project rebuilder TAG file processed listener to");
    }

    public void StopListening()
    {
      // Un-register the listener from the message group
      MsgGroup?.StopLocalListen(this, MessageTopicName);

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

      writer.WriteString(MessageTopicName);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      MessageTopicName = reader.ReadString();
    }
  }
}

