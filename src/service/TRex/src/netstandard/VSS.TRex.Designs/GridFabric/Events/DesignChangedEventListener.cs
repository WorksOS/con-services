using System;
using Microsoft.Extensions.Logging;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Messaging;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Common;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Caching.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using System.IO;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Common.Utilities;

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

        // Tell the DesignManager instance to remove the designated design
        var designFileName = message.FileType switch
        {
          ImportedFileType.DesignSurface => DIContext.Obtain<IDesignManager>()?.List(message.SiteModelUid)?.Locate(message.DesignUid)?.DesignDescriptor.FileName,
          ImportedFileType.SurveyedSurface => DIContext.Obtain<ISurveyedSurfaceManager>()?.List(message.SiteModelUid)?.Locate(message.DesignUid)?.DesignDescriptor.FileName,
          ImportedFileType.Alignment => DIContext.Obtain<IAlignmentManager>()?.List(message.SiteModelUid)?.Locate(message.DesignUid)?.DesignDescriptor.FileName,
          _ => string.Empty
        };

        IDesignBase design = null;
        if (!string.IsNullOrEmpty(designFileName))
        {
          design = DIContext.ObtainRequired<IDesignClassFactory>().NewInstance(message.DesignUid, designFileName, siteModel.CellSize, message.SiteModelUid);
        }

        if (design != null)
        {
          var localStorage = Path.Combine(FilePathHelper.GetTempFolderForProject(siteModel.ID), design.FileName);
          if (designFiles.RemoveDesignFromCache(message.DesignUid, design, message.SiteModelUid, false))
          {
            if (File.Exists(localStorage))
            {
              File.Delete(localStorage);
            }
          }
        }
        else
        {
          // No current record of the design
          _log.LogWarning($"Design {message.DesignUid} not present in designs for project {message.SiteModelUid} when responding to design change event");
          return true;
        }

        // Advise the spatial memory general sub grid result cache of the change so it can invalidate cached derivatives
        DIContext.ObtainOptional<ITRexSpatialMemoryCache>()?.InvalidateDueToDesignChange(message.SiteModelUid, message.DesignUid);
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
