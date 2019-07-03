using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.Log4NetExtensions;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.IO.Helpers;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Server
{
  /// <summary>
  /// Supports conversion of stream representations of spatial and non-spatial information from mutable to immutable forms.
  /// Note: The transformation may be one way trapdoor function in the case of spatial data immutability conversions that
  /// result in the compressed form of the appropriate schema.
  /// </summary>
  public class MutabilityConverter : IMutabilityConverter
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<MutabilityConverter>();

    private const int MinEventStreamLength = 16;

    private readonly ISubGridCellLatestPassesDataWrapperFactory subGridCellLatestPassesDataWrapperFactory = DIContext.Obtain<ISubGridCellLatestPassesDataWrapperFactory>();
    private readonly ISubGridCellSegmentPassesDataWrapperFactory subGridCellSegmentPassesDataWrapperFactory = DIContext.Obtain<ISubGridCellSegmentPassesDataWrapperFactory>();

    private static readonly IProductionEventsFactory _ProductionEventsFactory = DIContext.Obtain<IProductionEventsFactory>();

    /// <summary>
    /// Converts the structure of the global latest cells structure into an immutable form
    /// </summary>
    /// <returns></returns>
    private ISubGridCellLatestPassDataWrapper ConvertLatestPassesToImmutable(ISubGridCellLatestPassDataWrapper latestPasses, SegmentLatestPassesContext context)
    {
      if (latestPasses.IsImmutable())
      {
        return latestPasses; // It is already immutable
      }

      if (!(latestPasses is SubGridCellLatestPassDataWrapper_NonStatic oldItem))
      {
        Log.LogDebug("LastPasses is not a SubGridCellLatestPassDataWrapper_NonStatic instance");
        return null;
      }

      latestPasses = context == SegmentLatestPassesContext.Global
        ? subGridCellLatestPassesDataWrapperFactory.NewImmutableWrapper_Global()
        : subGridCellLatestPassesDataWrapperFactory.NewImmutableWrapper_Segment();

      // Immutable segments do not concern themselves with latest cell pass information so the factory will return null in this case
      if (latestPasses != null)
      {
        latestPasses.Assign(oldItem);

        (latestPasses as SubGridCellLatestPassDataWrapper_StaticCompressed)?.PerformEncodingForInternalCache(
          oldItem.PassData);
      }

      return latestPasses;
    }

    /// <summary>
    /// Primary method for performing mutability conversion to immutable. It accepts either
    /// a) a sourceObject, from which the immutable stream can be built directly, or
    /// b) a stream, which must be de-serialized into a source object, from which the immutable stream can be built
    /// i.e. either mutableStream, or source are null
    /// </summary>
    /// <param name="streamType"></param>
    /// <param name="mutableStream"></param>
    /// <param name="source"></param>
    /// <param name="immutableStream"></param>
    /// <returns></returns>
    public bool ConvertToImmutable(FileSystemStreamType streamType, MemoryStream mutableStream, object source, out MemoryStream immutableStream)
    {
      immutableStream = null;

      if (mutableStream == null && source == null)
      {
        throw new TRexException("Unable to determine a single valid source for immutability conversion.");
      }

      bool result;

      switch (streamType)
      {
        case FileSystemStreamType.SubGridDirectory:
        {
            result = source == null 
              ? ConvertSubGridDirectoryToImmutable(mutableStream, out immutableStream)
              : ConvertSubGridDirectoryToImmutable(source, out immutableStream);
            break;
        }
        case FileSystemStreamType.SubGridSegment:
        {
          result = source == null
            ? ConvertSubGridSegmentToImmutable(mutableStream, out immutableStream)
            : ConvertSubGridSegmentToImmutable(source, out immutableStream);
          break;
        }
        case FileSystemStreamType.Events:
        {
          result = source == null
            ? ConvertEventListToImmutable(mutableStream, out immutableStream)
            : ConvertEventListToImmutable(source, out immutableStream);
          break;
        }
        default:
        {
          // EG: Sub grid existence map etc
          immutableStream = mutableStream;
          result = true;
          break;
        }
      }

      if (mutableStream != null && immutableStream != null && Log.IsTraceEnabled())
        Log.LogInformation($"Mutability conversion: Type:{streamType}, Initial Size: {mutableStream.Length}, Final Size: {immutableStream.Length}, Ratio: {(immutableStream.Length/(1.0*mutableStream.Length)) * 100}%");

      return result;
    }

    /// <summary>
    /// Converts a sub grid directory into its immutable form, using the source object
    /// </summary>
    /// <param name="source"></param>
    /// <param name="immutableStream"></param>
    /// <returns></returns>
    private bool ConvertSubGridDirectoryToImmutable(object source, out MemoryStream immutableStream)
    {
      try
      {
        var originSource = (IServerLeafSubGrid) source;

        // create a copy and compress the latestPasses(and ensure the global latest cells is the mutable variety)
        using (var leaf = new ServerSubGridTreeLeaf(null, null, SubGridTreeConsts.SubGridTreeLevels, StorageMutability.Immutable)
          {
            LeafStartTime = originSource.LeafStartTime,
            LeafEndTime = originSource.LeafEndTime,
            Directory =
            {
              SegmentDirectory = originSource.Directory.SegmentDirectory,
              GlobalLatestCells = ConvertLatestPassesToImmutable(originSource.Directory.GlobalLatestCells,
                SegmentLatestPassesContext.Global)
            }
          })
        {
          immutableStream = RecyclableMemoryStreamManagerHelper.Manager.GetStream();
          leaf.SaveDirectoryToStream(immutableStream);
        }

        return true;
      }
      catch (Exception e)
      {
        immutableStream = null;

        Log.LogError(e, "Exception in conversion of sub grid directory mutable data to immutable schema");
        return false;
      }
    }

    /// <summary>
    /// Converts a sub grid directory into its immutable form, using a cached stream
    /// </summary>
    /// <param name="mutableStream"></param>
    /// <param name="immutableStream"></param>
    /// <returns></returns>
    private bool ConvertSubGridDirectoryToImmutable(MemoryStream mutableStream, out MemoryStream immutableStream)
    {
      try
      {
        // create a leaf to contain the mutable directory (and ensure the global latest cells is the mutable variety)
        using (var leaf = new ServerSubGridTreeLeaf(null, null, SubGridTreeConsts.SubGridTreeLevels, StorageMutability.Immutable)
          {
            Directory = {GlobalLatestCells = subGridCellLatestPassesDataWrapperFactory.NewMutableWrapper_Global()}
          })
        {
          // Load the mutable stream of information
          mutableStream.Position = 0;
          leaf.LoadDirectoryFromStream(mutableStream);

          using (var directory = leaf.Directory.GlobalLatestCells)
          {
            leaf.Directory.GlobalLatestCells = ConvertLatestPassesToImmutable(directory, SegmentLatestPassesContext.Global);
          }

          immutableStream = RecyclableMemoryStreamManagerHelper.Manager.GetStream();
          leaf.SaveDirectoryToStream(immutableStream);
        }

        return true;
      }
      catch (Exception e)
      {
        immutableStream = null;

        Log.LogError(e, "Exception in conversion of sub grid directory mutable data to immutable schema");
        return false;
      }
    }

    /// <summary>
    /// Converts a sub grid segment into its immutable form, using the source object
    /// </summary>
    /// <param name="source"></param>
    /// <param name="immutableStream"></param>
    /// <returns></returns>
    private bool ConvertSubGridSegmentToImmutable(object source, out MemoryStream immutableStream)
    {
      try
      {
        var originSource = (ISubGridCellPassesDataSegment) source;

        // create a copy and compress the latestPasses(and ensure the global latest cells is the mutable variety)
        using (var segment = new SubGridCellPassesDataSegment(ConvertLatestPassesToImmutable(originSource.LatestPasses, SegmentLatestPassesContext.Segment),
          subGridCellSegmentPassesDataWrapperFactory.NewImmutableWrapper())
        {
          StartTime = originSource.SegmentInfo.StartTime,
          EndTime = originSource.SegmentInfo.EndTime
        })
        {
          segment.PassesData.SetState(originSource.PassesData.GetState());

          // Write out the segment to the immutable stream
          immutableStream = RecyclableMemoryStreamManagerHelper.Manager.GetStream();
          using (var writer = new BinaryWriter(immutableStream, Encoding.UTF8, true))
          {
            segment.Write(writer);
          }
        }

        return true;
      }
      catch (Exception e)
      {
        immutableStream = null;

        Log.LogError(e, "Exception in conversion of sub grid segment mutable data to immutable schema");
        return false;
      }
    }

    /// <summary>
    /// Converts a sub grid segment into its immutable form, using a cached stream
    /// </summary>
    /// <param name="mutableStream"></param>
    /// <param name="immutableStream"></param>
    /// <returns></returns>
    private bool ConvertSubGridSegmentToImmutable(MemoryStream mutableStream, out MemoryStream immutableStream)
    {
      try
      {
        // Read in the sub grid segment from the mutable stream
        using (var segment = new SubGridCellPassesDataSegment(subGridCellLatestPassesDataWrapperFactory.NewMutableWrapper_Segment(),
                      subGridCellSegmentPassesDataWrapperFactory.NewMutableWrapper()))
        {
          mutableStream.Position = 0;
          using (var reader = new BinaryReader(mutableStream, Encoding.UTF8, true))
          {
            segment.Read(reader, true, true);
          }

          // Convert to the immutable form
          using (var mutableLatestPasses = segment.LatestPasses)
          {
            segment.LatestPasses = ConvertLatestPassesToImmutable(mutableLatestPasses, SegmentLatestPassesContext.Segment);
          }

          using (var mutablePasses = segment.PassesData)
          {
            segment.PassesData = subGridCellSegmentPassesDataWrapperFactory.NewImmutableWrapper();
            segment.PassesData.SetState(mutablePasses.GetState());
          }

          // Write out the segment to the immutable stream
          immutableStream = RecyclableMemoryStreamManagerHelper.Manager.GetStream();
          using (var writer = new BinaryWriter(immutableStream, Encoding.UTF8, true))
          {
            segment.Write(writer);
          }
        }

        return true;
      }
      catch (Exception e)
      {
        immutableStream = null;

        Log.LogError(e, "Exception in conversion of sub grid segment mutable data to immutable schema");
        return false;
      }
    }

    /// <summary>
    /// Convert an event list to it's immutable form. 
    /// Currently this is a no-op - the original stream is returned as there is not yet an immutable description for events
    /// </summary>
    /// <param name="source"></param>
    /// <param name="immutableStream"></param>
    /// <returns></returns>
    private bool ConvertEventListToImmutable(object source, out MemoryStream immutableStream)
    {
      immutableStream = ((IProductionEvents)source).GetImmutableStream();

      return true;
    }

    /// <summary>
    /// Convert an event list to it's immutable form. 
    /// Currently this is a no-op - the original stream is returned as there is not yet an immutable description for events
    /// </summary>
    /// <param name="mutableStream"></param>
    /// <param name="immutableStream"></param>
    /// <returns></returns>
    private bool ConvertEventListToImmutable(MemoryStream mutableStream, out MemoryStream immutableStream)
    {
      immutableStream = null;
      try
      {
        using (var reader = new BinaryReader(mutableStream, Encoding.UTF8, true))
        {
          if (mutableStream.Length < MinEventStreamLength)
          {
            Log.LogError($"ProductionEvent mutable stream length is too short. Expected greater than: {MinEventStreamLength} retrieved {mutableStream.Length}");
            return false;
          }

          mutableStream.Position = 1; // Skip the version to get the event list type

          var eventType = reader.ReadInt32();
          if (!Enum.IsDefined(typeof(ProductionEventType), eventType))
          {
            Log.LogError("ProductionEvent eventType is not recognized. Invalid stream.");
            return false;
          }

          var events = _ProductionEventsFactory.NewEventList(-1, Guid.Empty, (ProductionEventType)eventType);

          mutableStream.Position = 0;
          events.ReadEvents(reader);

          mutableStream.Position = 1; // Skip the version to get the event list type
          immutableStream = events.GetImmutableStream();
        }

        return true;
      }
      catch (Exception e)
      {
        immutableStream = null;

        Log.LogError(e, "Exception in conversion of event mutable data to immutable schema");
        return false;
      }
    }
  }
}
