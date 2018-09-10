using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server
{ 
  public class SubGridDirectory : ISubGridDirectory
  {
        private static ILogger Log = Logging.Logger.CreateLogger<SubGridDirectory>();

        // SegmentDirectory contains a list of all the segments that are present
        // in this subgrid. The list is time ordered and also contains references
        // to the segments that are currently loaded into memory
        public List<ISubGridCellPassesDataSegmentInfo> SegmentDirectory { get; set; } = new List<ISubGridCellPassesDataSegmentInfo>();

        // PersistedClovenSegments contains a list of all the segments that exists in the
        // persistent data store that have been cloven since the last time this leaf
        // was persisted to the data store. This is essentially a list of obsolete
        // segments whose presence in the persistent data store need to be removed
        // when the subgrid is next persisted
        private List<ISubGridCellPassesDataSegmentInfo> PersistedClovenSegments { get; set; }

        /// <summary>
        /// Adds a segment to the persistent list of cloven segments. The underlying list is created
        /// on demand under a transient lock against this subgrid
        /// </summary>
        /// <param name="segment"></param>
        public void AddPersistedClovenSegment(ISubGridCellPassesDataSegmentInfo segment)
        {
            lock (this)
            {
                (PersistedClovenSegments ?? (PersistedClovenSegments = new List<ISubGridCellPassesDataSegmentInfo>())).Add(segment);
            }
        }

        /// <summary>
        /// Extracts and returns the current list of persisted cloven segments. THe internal list is set to null
        /// </summary>
        /// <returns></returns>
        public List<ISubGridCellPassesDataSegmentInfo> ExtractPersistedClovenSegments()
        {
            lock (this)
            {
                var result = PersistedClovenSegments;
                PersistedClovenSegments = null;
                return result;
            }
        }

        // GlobalLatestCells contains the computed latest cell information that spans
        // all the segments in the subgrid
        public ISubGridCellLatestPassDataWrapper GlobalLatestCells { get; set; }

        public void AllocateGlobalLatestCells()
        {
            if (GlobalLatestCells == null)
            {
                GlobalLatestCells = SubGridCellLatestPassesDataWrapperFactory.Instance().NewWrapper(); // new  SubGridCellLatestPassDataWrapper_NonStatic();
            }
        }

        public void DeAllocateGlobalLatestCells()
        {
            GlobalLatestCells = null;
        }

        public SubGridDirectory()
        {
            //    PersistedClovenSegments = TICSubGridCellPassesDataSegmentInfoList.Create;
            //    PersistedClovenSegments.KeepSegmentsInOrder = False;
        }

        public void CreateDefaultSegment()
        {
            if (SegmentDirectory.Count != 0)
            {
                Log.LogCritical("Cannot create default segment if there are already segments in the list");
                return;
            }

            SegmentDirectory.Add(new SubGridCellPassesDataSegmentInfo());
        }

        public void Clear()
        {
            // Remove the global latest cell passes
            GlobalLatestCells?.Clear();

            // Unhook all loaded segments from the segment directory
            SegmentDirectory.ForEach(x => { x.Segment = null; });
        }

        public bool Write(BinaryWriter writer)
        {
            try
            {
                Debug.Assert(GlobalLatestCells != null, "Cannot write subgrid directory without global latest values available");

                GlobalLatestCells.Write(writer, new byte[10000]);

                // Write out the directoy of segments
                Debug.Assert(SegmentDirectory.Count > 0, "Writing a segment directory with no segments");
                writer.Write((int)SegmentDirectory.Count);

                foreach (var Segment in SegmentDirectory)
                {
                    Segment.Write(writer);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool Read_2p0(BinaryReader reader)
        {
            try
            {
                Debug.Assert(GlobalLatestCells != null, "Cannot read subgrid directory without global latest values available");

                GlobalLatestCells.Read(reader, new byte[10000]);

                // Read in the directoy of segments
                int SegmentCount = reader.ReadInt32();

                for (int I = 0; I < SegmentCount; I++)
                {
                    SubGridCellPassesDataSegmentInfo segmentInfo = new SubGridCellPassesDataSegmentInfo();
                    segmentInfo.Read(reader);

                    segmentInfo.ExistsInPersistentStore = true;
                    SegmentDirectory.Add(segmentInfo);
                }
            }
            catch //(Exception e)
            {
                return false;
            }

            return true;
        }
    }
}
