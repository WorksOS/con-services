using System;
using Microsoft.Extensions.Logging;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Messaging;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Common;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Alignments.Interfaces;

namespace VSS.TRex.Designs.GridFabric.Events
{

  /// <summary>
  /// The listener that responds to design change notifications emitted by actions such as changing a design
  /// </summary>
  public class DesignChangedEventListener : IMessageListener<IDesignChangedEvent>, IDisposable, IDesignChangedEventListener, IBinarizable, IFromToBinary
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<DesignChangedEventListener>();

    private const byte VERSION_NUMBER = 1;

    public const string DESIGN_CHANGED_EVENTS_TOPIC_NAME = "DesignStateChangedEvents";

    /// <summary>
    ///  Message group the listener has been added to
    /// </summary>
    private IMessaging MsgGroup;

    public string MessageTopicName { get; set; } = DESIGN_CHANGED_EVENTS_TOPIC_NAME;

    public string GridName { get; private set; }

    public bool Invoke(Guid nodeId, IDesignChangedEvent message)
    {
      try
      {
        Log.LogInformation(
          $"Received notification of design changed for site:{message.SiteModelUid}, design:{message.DesignUid}, DesignType:{message.FileType}, DesignRemoved:{message.DesignRemoved}, ImportedFileType:{message.FileType}");

        // Tell the DesignManager instance to remove the designated design
        if (message.FileType == ImportedFileType.DesignSurface)
        {
          var designs = DIContext.Obtain<IDesignManager>();
          if (designs != null)
          {
            if (message.DesignRemoved)
              designs.Remove(message.SiteModelUid, message.DesignUid);
          }
          else
          {
            Log.LogWarning("No IDesignManager instance available from DIContext to send attributes change message to");
            return false;
          }
        }
        else if (message.FileType == ImportedFileType.SurveyedSurface)
        {
          var surveyedSurface = DIContext.Obtain<ISurveyedSurfaceManager>();
          if (surveyedSurface != null)
          {
            if (message.DesignRemoved)
              surveyedSurface.Remove(message.SiteModelUid, message.DesignUid);
          }
          else
          {
            Log.LogWarning("No ISurveyedSurfaceManager instance available from DIContext to send attributes change message to");
            return false;
          }
        }
        else if (message.FileType == ImportedFileType.Alignment)
        {
          var alignment = DIContext.Obtain<IAlignmentManager>();
          if (alignment != null)
          {
            if (message.DesignRemoved)
              alignment.Remove(message.SiteModelUid, message.DesignUid);
          }
          else
          {
            // Note! not all listeners maybe interested in the design type removed so only log as warning 
            Log.LogWarning("No IAlignmentManager instance available from DIContext to send attributes change message to");
            return false;
          }
        }


      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception occured processing design changed event");
        return false;
      }
      finally
      {
        Log.LogInformation(
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
      Log.LogInformation($"Start listening for design state notification events on {MessageTopicName}");

      // Create a messaging group the cluster can use to send messages back to and establish a local listener
      // All nodes (client and server) want to know about design state change
      MsgGroup = DIContext.Obtain<ITRexGridFactory>()?.Grid(GridName)?.GetCluster().GetMessaging();

      if (MsgGroup != null)
        MsgGroup.LocalListen(this, MessageTopicName);
      else
        Log.LogError("Unable to get messaging projection to add design state change event to");
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
