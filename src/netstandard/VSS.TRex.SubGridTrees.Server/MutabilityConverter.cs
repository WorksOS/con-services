using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.Events.Interfaces;
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
    /// Primary method for performing mutability conversion to immutable. It accepts a stream and an indication of the type of stream
    /// then delegates to conversion responsibility based on the stream type.
    /// </summary>
    /// <param name="streamType"></param>
    /// <param name="mutableStream"></param>
    /// <param name="source"></param>
    /// <param name="immutableStream"></param>
    /// <returns></returns>
    public bool ConvertToImmutable(FileSystemStreamType streamType, MemoryStream mutableStream, object source, out MemoryStream immutableStream)
    {
      immutableStream = null;

      switch (streamType)
      {
        case FileSystemStreamType.SubGridDirectory:
        {
          return ConvertSubgridDirectoryToImmutable(source, out immutableStream);
        }
        case FileSystemStreamType.SubGridSegment:
        {
          return ConvertSubgridSegmentToImmutable(source, out immutableStream);
        }
        case FileSystemStreamType.Events:
        {
          immutableStream = ((IProductionEvents) source).GetImmutableStream();
          return true;
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
    /// Converts a subgrid directory into its immutable form i.e. compressingLatestPasses
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

        Log.LogError($"Exception in conversion of subgrid directory mutable data to immutable schema: {e}");
        return false;
      }
    }

    /// <summary>
    /// Converts a subgrid segment into its immutable form i.e. compressingLatestPasses
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

        Log.LogError($"Exception in conversion of subgrid segment mutable data to immutable schema: {e}");
        return false;
      }
    }

  }
}
