using System;
using System.IO;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server
{
  public class SubGridCellPassesDataSegmentInfo : ISubGridCellPassesDataSegmentInfo
  {
        private static byte SERIALIZATION_VERSION = 2;

        /// <summary>
        /// The version number of this segment when it is stored in the persistent layer, defined
        /// as the number of ticks in DateTime.UtcNow at the time it is written.
        /// </summary>
        public long Version { get; set; }

        public ISubGridCellPassesDataSegment Segment { get; set; }
        public DateTime StartTime { get; set; } = Consts.MIN_DATETIME_AS_UTC;
        public DateTime EndTime { get; set; } = Consts.MAX_DATETIME_AS_UTC;

        public double MinElevation { get; set; } = Consts.NullDouble;
        public double MaxElevation { get; set; } = Consts.NullDouble;

        /// <summary>
        /// Machine directory containing a list of machine internal IDs identifying all machines in the project that
        /// contributed cell passes to this sub grid
        /// </summary>
        public short[] MachineDirectory { get; set; }

        public bool ExistsInPersistentStore { get; set; }

        public DateTime MidTime => DateTime.FromOADate((StartTime.ToOADate() + EndTime.ToOADate()) / 2);

        public SubGridCellPassesDataSegmentInfo()
        {
            Touch();
        }

        public SubGridCellPassesDataSegmentInfo(DateTime startTime, DateTime endTime,
                                                ISubGridCellPassesDataSegment segment) : this()
        {
            StartTime = startTime;
            EndTime = endTime;
            Segment = segment;
        }

        public ISubGridSpatialAffinityKey AffinityKey(Guid projectUid)
        {
          return new SubGridSpatialAffinityKey(Version, projectUid, Segment.Owner.OriginX, Segment.Owner.OriginY,
                                               Segment.SegmentInfo.StartTime.Ticks, Segment.SegmentInfo.EndTime.Ticks);
        }

        /// <summary>
        /// IncludesTimeWithinBounds determines if ATime is strictly greater than
        /// the start time and strictly less than the end time of this segment.
        /// It is not intended to resolve boundary edge cases where ATime is exactly
        /// equal to the start or end time of the segment
        /// </summary>
        public bool IncludesTimeWithinBounds(DateTime time)
        {
            var testTime = time.Ticks;
            return testTime > StartTime.Ticks && testTime < EndTime.Ticks;
        }

        /// <summary>
        /// Returns a string representing the segment identifier for this segment within this sub grid. The identifier
        /// is based on the time range this segment is responsible for storing cell passes for.
        /// </summary>
        public string SegmentIdentifier() => StartTime.Ticks + "-" + EndTime.Ticks; // 30% faster than $"{StartTime.Ticks}-{EndTime.Ticks}"

        /// <summary>
        /// Returns the 'filename', and string that encodes the segment version, spatial location and time range it 
        /// is responsible for.
        /// </summary>
        public string FileName(int originX, int originY) => $"{Version}-{originX:d10}-{originY:d10}-{SegmentIdentifier()}";

        public void Write(BinaryWriter writer)
        {
            VersionSerializationHelper.EmitVersionByte(writer, SERIALIZATION_VERSION);
            writer.Write(Version);
            writer.Write(StartTime.ToBinary());
            writer.Write(EndTime.ToBinary());
            writer.Write(MinElevation);
            writer.Write(MaxElevation);

            // Write the machine directory
            writer.Write(MachineDirectory != null);
            if (MachineDirectory != null)
            {
              var machineCount = MachineDirectory.Length;
              writer.Write(MachineDirectory.Length);
              for (var i = 0; i < machineCount; i++)
                writer.Write(MachineDirectory[i]);
            }
        }

        public void ReadUnversioned(BinaryReader reader)
        {
            Version = reader.ReadInt64();
            StartTime = DateTime.FromBinary(reader.ReadInt64());
            EndTime = DateTime.FromBinary(reader.ReadInt64());
            MinElevation = reader.ReadDouble();
            MaxElevation = reader.ReadDouble();
        }

        public void Read(BinaryReader reader)
        {
          var serializationVersion = VersionSerializationHelper.CheckVersionByte(reader, SERIALIZATION_VERSION);

          if (serializationVersion >= 1)
          {
            Version = reader.ReadInt64();
            StartTime = DateTime.FromBinary(reader.ReadInt64());
            EndTime = DateTime.FromBinary(reader.ReadInt64());
            MinElevation = reader.ReadDouble();
            MaxElevation = reader.ReadDouble();
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
        }

        /// <summary>
        /// Updates the version of the segment to reflect the current date time
        /// </summary>
        public void Touch()
        {
          Version = DateTime.UtcNow.Ticks;
        }

        public override string ToString()
        {
          return $"ID: {SegmentIdentifier()}, MinElev: {MinElevation}, MaxElev: {MaxElevation}, ExistsInPersistentStore?:{ExistsInPersistentStore}, AllPasses?:{Segment?.HasAllPasses ?? false}, LatestData?:{Segment?.HasLatestData ?? false}";
        }
    }
}
