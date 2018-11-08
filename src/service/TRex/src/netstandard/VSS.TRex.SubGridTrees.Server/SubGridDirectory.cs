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
     
        /// <summary>
        /// This subgrid is present in the persistent store
        /// </summary>
        public bool ExistsInPersistentStore { get; set; }

        // SegmentDirectory contains a list of all the segments that are present
        // in this subgrid. The list is time ordered and also contains references
        // to the segments that are currently loaded into memory
        public List<ISubGridCellPassesDataSegmentInfo> SegmentDirectory { get; set; } = new List<ISubGridCellPassesDataSegmentInfo>();

        // GlobalLatestCells contains the computed latest cell information that spans
        // all the segments in the subgrid
        public ISubGridCellLatestPassDataWrapper GlobalLatestCells { get; set; }

        public void AllocateGlobalLatestCells()
        {
            if (GlobalLatestCells == null)
            {
                GlobalLatestCells = SubGridCellLatestPassesDataWrapperFactory.Instance().NewWrapper();
            }
        }

        public void DeAllocateGlobalLatestCells()
        {
            GlobalLatestCells = null;
        }

        public SubGridDirectory()
        {
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

                // Write out the directory of segments
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

                // Read in the directory of segments
                int SegmentCount = reader.ReadInt32();

                for (int I = 0; I < SegmentCount; I++)
                {
                    SubGridCellPassesDataSegmentInfo segmentInfo = new SubGridCellPassesDataSegmentInfo();
                    segmentInfo.Read(reader);

                    segmentInfo.ExistsInPersistentStore = true;
                    SegmentDirectory.Add(segmentInfo);
                }

              ExistsInPersistentStore = true;
            }
            catch //(Exception e)
            {
                return false;
            }

            return true;
        }
    }
}
