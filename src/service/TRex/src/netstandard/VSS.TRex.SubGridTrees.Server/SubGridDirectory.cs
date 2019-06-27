using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.DI;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server
{ 
  public class SubGridDirectory : ISubGridDirectory
  {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridDirectory>();

        /// <summary>
        /// Controls whether segment and cell pass information held within this sub grid is represented
        /// in the mutable or immutable forms supported by TRex
        /// </summary>
        public bool IsMutable { get; set; } = false;

        /// <summary>
        /// This sub grid is present in the persistent store
        /// </summary>
        public bool ExistsInPersistentStore { get; set; }

        // SegmentDirectory contains a list of all the segments that are present
        // in this sub grid. The list is time ordered and also contains references
        // to the segments that are currently loaded into memory
        public List<ISubGridCellPassesDataSegmentInfo> SegmentDirectory { get; set; } = new List<ISubGridCellPassesDataSegmentInfo>();

        // GlobalLatestCells contains the computed latest cell information that spans
        // all the segments in the sub grid
        public ISubGridCellLatestPassDataWrapper GlobalLatestCells { get; set; }

        private readonly ISubGridCellLatestPassesDataWrapperFactory subGridCellLatestPassesDataWrapperFactory = DIContext.Obtain<ISubGridCellLatestPassesDataWrapperFactory>();

        public void AllocateGlobalLatestCells()
        {
            if (GlobalLatestCells == null)
            {
              GlobalLatestCells = IsMutable
                ? subGridCellLatestPassesDataWrapperFactory.NewMutableWrapper_Global()
                : subGridCellLatestPassesDataWrapperFactory.NewImmutableWrapper_Global();
            }
        }

        public SubGridDirectory()
        {
        }

        public void DumpSegmentDirectoryToLog()
        {
          Log.LogInformation($"Segment directory contains {SegmentDirectory.Count} segments");

          foreach (var si in SegmentDirectory)
            Log.LogInformation(si.ToString());
        }

        public void CreateDefaultSegment()
        {
            if (SegmentDirectory.Count == 0)
              SegmentDirectory.Add(new SubGridCellPassesDataSegmentInfo());
        }

        public void Clear()
        {
            // Remove the global latest cell passes
            GlobalLatestCells?.Clear();

            // Unhook all loaded segments from the segment directory
            SegmentDirectory.ForEach(x => { x.Segment = null; });
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(GlobalLatestCells != null);
            GlobalLatestCells?.Write(writer);

            // Write out the directory of segments
            writer.Write(SegmentDirectory.Count);

            foreach (var Segment in SegmentDirectory)
            {
              Segment.Write(writer);
            }

            ExistsInPersistentStore = true;
        }

        public void Read(BinaryReader reader)
        {
          if (reader.ReadBoolean())
          {
            if (GlobalLatestCells == null)
              throw new TRexSubGridIOException("Cannot read sub grid directory without global latest values available");
            GlobalLatestCells.Read(reader);
          }

          // Read in the directory of segments
          int SegmentCount = reader.ReadInt32();

          for (int I = 0; I < SegmentCount; I++)
          {
            var segmentInfo = new SubGridCellPassesDataSegmentInfo();
            segmentInfo.Read(reader);

            segmentInfo.ExistsInPersistentStore = true;
            SegmentDirectory.Add(segmentInfo);
          }

          ExistsInPersistentStore = true;
        }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          GlobalLatestCells?.Dispose();
        }

        disposedValue = true;
      }
    }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
    }
    #endregion
  }
}
