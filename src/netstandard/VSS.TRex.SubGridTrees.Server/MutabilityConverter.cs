using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.Events.Interfaces;
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
    public ISubGridCellLatestPassDataWrapper ConvertLatestPassesToImmutable(ISubGridCellLatestPassDataWrapper LatestPasses)
    {
      if (LatestPasses.IsImmutable())
      {
        return LatestPasses; // It is already immutable
      }

      SubGridCellLatestPassDataWrapper_NonStatic oldItem = LatestPasses as SubGridCellLatestPassDataWrapper_NonStatic;

      if (oldItem == null)
      {
        Log.LogDebug("LastPasses is not a SubGridCellLatestPassDataWrapper_NonStatic instance");
        return null;
      }

      LatestPasses = SubGridCellLatestPassesDataWrapperFactory.Instance().NewWrapper(false, true);
      LatestPasses.Assign(oldItem);

      (LatestPasses as SubGridCellLatestPassDataWrapper_StaticCompressed)?.PerformEncodingForInternalCache(oldItem.PassData);

      return LatestPasses;
    }

    /// <summary>
    /// Primary method for performing mutability conversion to immutable. It accepts a stream and an indication of the type of stream
    /// then delegates to conversion responsibility based on the stream type.
    /// </summary>
    /// <param name="streamType"></param>
    /// <param name="mutableStream"></param>
    /// <param name="immutableStream"></param>
    /// <returns></returns>
    public bool ConvertToImmutable(FileSystemStreamType streamType, MemoryStream mutableStream, Object source, out MemoryStream immutableStream)
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
    public bool ConvertSubgridDirectoryToImmutable(Object source, out MemoryStream immutableStream)
    {
      try
      {
        // create a copy and compress the LatestPasses(and ensure the global latest cells is the mutable variety)
        IServerLeafSubGrid leaf = (IServerLeafSubGrid) source;
        leaf.Directory.GlobalLatestCells = ConvertLatestPassesToImmutable(leaf.Directory.GlobalLatestCells);

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
    public bool ConvertSubgridSegmentToImmutable(Object source, out MemoryStream immutableStream)
    {
      try
      {
        // Read in the subgrid segment from the mutable stream
        ISubGridCellPassesDataSegment segment = (ISubGridCellPassesDataSegment) source;

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

        Log.LogError($"Exception in conversion of subgrid segment mutable data to immutable schema: {e}");
        return false;
      }
    }

  }
}
