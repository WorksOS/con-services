using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Exceptions;
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
    private static ILogger Log = Logging.Logger.CreateLogger(nameof(MutabilityConverter));
    private const int MinEventStreamLength = 16;

    /// <summary>
    /// Converts the structure of the global latext cells structure into an immutable form
    /// </summary>
    /// <returns></returns>
    public ISubGridCellLatestPassDataWrapper ConvertLatestPassesToImmutable(ISubGridCellLatestPassDataWrapper latestPasses)
    {
      if (latestPasses.IsImmutable())
      {
        return latestPasses; // It is already immutable
      }

      SubGridCellLatestPassDataWrapper_NonStatic oldItem = latestPasses as SubGridCellLatestPassDataWrapper_NonStatic;

      if (oldItem == null)
      {
        Log.LogDebug("LastPasses is not a SubGridCellLatestPassDataWrapper_NonStatic instance");
        return null;
      }

      latestPasses = SubGridCellLatestPassesDataWrapperFactory.Instance().NewWrapper(false, true);
      latestPasses.Assign(oldItem);

      (latestPasses as SubGridCellLatestPassDataWrapper_StaticCompressed)?.PerformEncodingForInternalCache(oldItem.PassData);

      return latestPasses;
    }

    /// <summary>
    /// Primary method for performing mutability conversion to immutable. It accepts either
    /// a) a sourceObject, from which the immutable stream can be built directly, or
    /// b) a stream, which must be de-serialized into a sourceobject, from which the immutable stream can be built
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

      if ((mutableStream == null && source == null))
      {
        throw new TRexException("Unable to determine a single valid source for immutability conversion.");
      }

      switch (streamType)
      {
        case FileSystemStreamType.SubGridDirectory:
        {
            return source == null 
              ? ConvertSubgridDirectoryToImmutable(mutableStream, out immutableStream)
              : ConvertSubgridDirectoryToImmutable(source, out immutableStream);
        }
        case FileSystemStreamType.SubGridSegment:
        {
          return source == null
            ? ConvertSubgridSegmentToImmutable(mutableStream, out immutableStream)
            : ConvertSubgridSegmentToImmutable(source, out immutableStream);
        }
        case FileSystemStreamType.Events:
        {
          return source == null
            ? ConvertEventListToImmutable(mutableStream, out immutableStream)
            : ConvertEventListToImmutable(source, out immutableStream);
        }
        default:
        {
          // EG: Subgrid existence map etc
          immutableStream = mutableStream;
          return true;
        }
      }
    }

    /// <summary>
    /// Converts a subgrid directory into its immutable form, using the source object
    /// </summary>
    /// <param name="source"></param>
    /// <param name="immutableStream"></param>
    /// <returns></returns>
    public bool ConvertSubgridDirectoryToImmutable(object source, out MemoryStream immutableStream)
    {
      try
      {
        var originSource = (IServerLeafSubGrid) source;

        // create a copy and compress the latestPasses(and ensure the global latest cells is the mutable variety)
        IServerLeafSubGrid leaf = new ServerSubGridTreeLeaf(null, null, SubGridTreeConsts.SubGridTreeLevels)
        {
          LeafStartTime = originSource.LeafStartTime,
          LeafEndTime = originSource.LeafEndTime,
          Directory =
          {
            SegmentDirectory = originSource.Directory.SegmentDirectory,
            GlobalLatestCells = ConvertLatestPassesToImmutable(originSource.Directory.GlobalLatestCells)
          }
        };

        immutableStream = new MemoryStream();
        leaf.SaveDirectoryToStream(immutableStream);

        return true;
      }
      catch (Exception e)
      {
        immutableStream = null;

        Log.LogError(e, "Exception in conversion of subgrid directory mutable data to immutable schema");
        return false;
      }
    }

    /// <summary>
    /// Converts a subgrid directory into its immutable form, using a cached stream
    /// </summary>
    /// <param name="mutableStream"></param>
    /// <param name="immutableStream"></param>
    /// <returns></returns>
    public bool ConvertSubgridDirectoryToImmutable(MemoryStream mutableStream, out MemoryStream immutableStream)
    {
      try
      {
        // create a leaf to contain the mutable directory (and ensure the global latest cells is the mutable variety)
        IServerLeafSubGrid leaf = new ServerSubGridTreeLeaf(null, null, SubGridTreeConsts.SubGridTreeLevels)
        {
          Directory =
          {
            GlobalLatestCells = SubGridCellLatestPassesDataWrapperFactory.Instance().NewWrapper(true, false)
          }
        };

        // Load the mutable stream of information
        mutableStream.Position = 0;
        leaf.LoadDirectoryFromStream(mutableStream);

        leaf.Directory.GlobalLatestCells = ConvertLatestPassesToImmutable(leaf.Directory.GlobalLatestCells);

        immutableStream = new MemoryStream();
        leaf.SaveDirectoryToStream(immutableStream);

        return true;
      }
      catch (Exception e)
      {
        immutableStream = null;

        Log.LogError(e, "Exception in conversion of subgrid directory mutable data to immutable schema");
        return false;
      }
    }

    /// <summary>
    /// Converts a subgrid segment into its immutable form, using the source object
    /// </summary>
    /// <param name="source"></param>
    /// <param name="immutableStream"></param>
    /// <returns></returns>
    public bool ConvertSubgridSegmentToImmutable(object source, out MemoryStream immutableStream)
    {
      try
      {
        var originSource = (ISubGridCellPassesDataSegment) source;

        // create a copy and compress the latestPasses(and ensure the global latest cells is the mutable variety)
        SubGridCellPassesDataSegment segment = new SubGridCellPassesDataSegment
        (ConvertLatestPassesToImmutable(originSource.LatestPasses),
          SubGridCellSegmentPassesDataWrapperFactory.Instance().NewWrapper(false, true))
        {
          StartTime = originSource.SegmentInfo.StartTime,
          EndTime = originSource.SegmentInfo.EndTime
        };

        segment.PassesData.SetState(originSource.PassesData.GetState());

        // Write out the segment to the immutable stream
        immutableStream = new MemoryStream();
        using (var writer = new BinaryWriter(immutableStream, Encoding.UTF8, true))
        {
          segment.Write(writer);
        }

        return true;
      }
      catch (Exception e)
      {
        immutableStream = null;

        Log.LogError(e, "Exception in conversion of subgrid segment mutable data to immutable schema");
        return false;
      }
    }

    /// <summary>
    /// Converts a subgrid segment into its immutable form, using a cached stream
    /// </summary>
    /// <param name="mutableStream"></param>
    /// <param name="immutableStream"></param>
    /// <returns></returns>
    public bool ConvertSubgridSegmentToImmutable(MemoryStream mutableStream, out MemoryStream immutableStream)
    {
      try
      {
        // Read in the subgrid segment from the mutable stream
        SubGridCellPassesDataSegment segment = new SubGridCellPassesDataSegment
        (SubGridCellLatestPassesDataWrapperFactory.Instance().NewWrapper(true, false),
          SubGridCellSegmentPassesDataWrapperFactory.Instance().NewWrapper(true, false));

        mutableStream.Position = 0;
        using (var reader = new BinaryReader(mutableStream, Encoding.UTF8, true))
        {
          segment.Read(reader, true, true);
        }

        // Convert to the immutable form
        segment.LatestPasses = ConvertLatestPassesToImmutable(segment.LatestPasses);

        ISubGridCellSegmentPassesDataWrapper mutablePassesData = segment.PassesData;

        segment.PassesData = SubGridCellSegmentPassesDataWrapperFactory.Instance().NewWrapper(false, true);
        segment.PassesData.SetState(mutablePassesData.GetState());

        // Write out the segment to the immutable stream
        immutableStream = new MemoryStream();
        using (var writer = new BinaryWriter(immutableStream, Encoding.UTF8, true))
        {
          segment.Write(writer);
        }

        return true;
      }
      catch (Exception e)
      {
        immutableStream = null;

        Log.LogError(e, "Exception in conversion of subgrid segment mutable data to immutable schema");
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
    public bool ConvertEventListToImmutable(object source, out MemoryStream immutableStream)
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
    public bool ConvertEventListToImmutable(MemoryStream mutableStream, out MemoryStream immutableStream)
    {
      immutableStream = null;
      try
      {
        IProductionEvents events;
        using (var reader = new BinaryReader(mutableStream, Encoding.UTF8, true))
        {
          if (mutableStream.Length < MinEventStreamLength)
          {
            Log.LogError($"ProductionEvent mutable stream length is too short. Expected greater than: {MinEventStreamLength} retrieved {mutableStream.Length}");
            return false;
          }
          mutableStream.Position = 8;

          var eventType = reader.ReadInt32();
          if (!Enum.IsDefined(typeof(ProductionEventType), eventType))
          {
            Log.LogError($"ProductionEvent eventType is not recognized. Invalid stream.");
            return false;
          }

          events = DIContext.Obtain<IProductionEventsFactory>().NewEventList(-1, Guid.Empty, (ProductionEventType)eventType);

          mutableStream.Position = 0;
          events.ReadEvents(reader);

          mutableStream.Position = 8;
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
