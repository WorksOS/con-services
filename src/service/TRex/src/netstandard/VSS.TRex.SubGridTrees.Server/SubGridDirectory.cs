using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.DI;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server
{ 
  public class SubGridDirectory : ISubGridDirectory
  {
        private static readonly ILogger _log = Logging.Logger.CreateLogger<SubGridDirectory>();

        private static byte SERIALIZATION_VERSION = 2;

        /// <summary>
        /// Controls whether segment and cell pass information held within this sub grid is represented
        /// in the mutable or immutable forms supported by TRex
        /// </summary>
        public bool IsMutable { get; set; } = false;

        /// <summary>
        /// This sub grid is present in the persistent store
        /// </summary>
        public bool ExistsInPersistentStore { get; set; }

        /// <summary>
        /// SegmentDirectory contains a list of all the segments that are present
        /// in this sub grid. The list is time ordered and also contains references
        /// to the segments that are currently loaded into memory
        /// </summary>
        public List<ISubGridCellPassesDataSegmentInfo> SegmentDirectory { get; set; } = new List<ISubGridCellPassesDataSegmentInfo>();

        /// <summary>
        /// GlobalLatestCells contains the computed latest cell information that spans all the segments in the sub grid
        /// </summary>
        public ISubGridCellLatestPassDataWrapper GlobalLatestCells { get; set; }

        /// <summary>
        /// Machine directory containing a list of machine internal IDs identifying all machines in the project that
        /// contributed cell passes to this sub grid
        /// </summary>
        public short[] MachineDirectory { get; private set; }

        private readonly ISubGridCellLatestPassesDataWrapperFactory _subGridCellLatestPassesDataWrapperFactory = DIContext.Obtain<ISubGridCellLatestPassesDataWrapperFactory>();

        public void AllocateGlobalLatestCells()
        {
            if (GlobalLatestCells == null)
            {
              GlobalLatestCells = IsMutable
                ? _subGridCellLatestPassesDataWrapperFactory.NewMutableWrapper_Global()
                : _subGridCellLatestPassesDataWrapperFactory.NewImmutableWrapper_Global();
            }
        }

        public SubGridDirectory()
        {
        }

        public void DumpSegmentDirectoryToLog()
        {
          _log.LogInformation($"Segment directory contains {SegmentDirectory.Count} segments");

          foreach (var si in SegmentDirectory)
            _log.LogInformation(si.ToString());
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
            SegmentDirectory.ForEach(x =>
            {
              x.Segment.Dispose();
              x.Segment = null;
            });
        }

        public void Write(BinaryWriter writer)
        {
            VersionSerializationHelper.EmitVersionByte(writer, SERIALIZATION_VERSION);

            writer.Write(GlobalLatestCells != null);
            GlobalLatestCells?.Write(writer);

            // Write out the directory of segments
            writer.Write(SegmentDirectory.Count);

            foreach (var segment in SegmentDirectory)
            {
              segment.Write(writer);
            }

            // Write the machine directory
            writer.Write(MachineDirectory != null);
            if (MachineDirectory != null)
            {
              var machineCount = MachineDirectory.Length;
              writer.Write(MachineDirectory.Length);
              for (var i = 0; i < machineCount; i++)
                writer.Write(MachineDirectory[i]);
            }

            ExistsInPersistentStore = true;
        }

        /// <summary>
        /// Supports reading the initial non-versioned serialization of the sub grid directory
        /// </summary>
        public void ReadUnVersioned(BinaryReader reader)
        {
            if (reader.ReadBoolean())
            {
              if (GlobalLatestCells == null)
              {
                throw new TRexSubGridIOException("Cannot read sub grid directory without global latest values available");
              }
              
              GlobalLatestCells.Read(reader);
            }

            // Read in the directory of segments
            var segmentCount = reader.ReadInt32();

            for (var i = 0; i < segmentCount; i++)
            {
              var segmentInfo = new SubGridCellPassesDataSegmentInfo();
              segmentInfo.ReadUnVersioned(reader);

              segmentInfo.ExistsInPersistentStore = true;
              SegmentDirectory.Add(segmentInfo);
            }

            ExistsInPersistentStore = true;
        }

        public void Read(BinaryReader reader)
        {
          var serializationVersion = VersionSerializationHelper.CheckVersionByte(reader, SERIALIZATION_VERSION);

          if (serializationVersion >= 1)
          {
            if (reader.ReadBoolean())
            {
              if (GlobalLatestCells == null)
              {
                throw new TRexSubGridIOException("Cannot read sub grid directory without global latest values available");
              }

              GlobalLatestCells.Read(reader);
            }

            // Read in the directory of segments
            var segmentCount = reader.ReadInt32();

            for (var i = 0; i < segmentCount; i++)
            {
              var segmentInfo = new SubGridCellPassesDataSegmentInfo();
              segmentInfo.Read(reader);

              segmentInfo.ExistsInPersistentStore = true;
              SegmentDirectory.Add(segmentInfo);
            }
          }

          if (serializationVersion >= 2)
          {
            if (reader.ReadBoolean())
            {
              // Read in the machine list for the sub grid
              var machineCount = reader.ReadInt32();
              MachineDirectory = new short[machineCount];
              for (var i = 0; i < machineCount; i++)
              {
                MachineDirectory[i] = reader.ReadInt16();
              }
            }
          }

          ExistsInPersistentStore = true;
        }
    #region IDisposable Support
    private bool _disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
          GlobalLatestCells?.Dispose();
        }

        _disposedValue = true;
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
