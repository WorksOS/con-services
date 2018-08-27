using System;
using System.IO;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server
{
  public class SubGridCellPassesDataSegmentInfo : ISubGridCellPassesDataSegmentInfo
  {
        public ISubGridCellPassesDataSegment Segment { get; set; }
        public DateTime StartTime { get; set; } = DateTime.MinValue;
        public DateTime EndTime { get; set; } = DateTime.MaxValue;

        public double MinElevation { get; set; } = Consts.NullDouble;
        public double MaxElevation { get; set; } = Consts.NullDouble;

        public bool ExistsInPersistentStore { get; set; }

        public DateTime MidTime => DateTime.FromOADate((StartTime.ToOADate() + EndTime.ToOADate()) / 2);

        public SubGridCellPassesDataSegmentInfo()
        {
        }
        public SubGridCellPassesDataSegmentInfo(DateTime startTime, DateTime endTime,
                                                ISubGridCellPassesDataSegment segment)
        {
            StartTime = startTime;
            EndTime = endTime;
            Segment = segment;
        }

        //        procedure SaveToStream(Stream: TStream);
        //        procedure LoadFromStream(Stream: TStream);

        /// <summary>
        /// IncludesTimeWithinBounds determines if ATime is strictly greater than
        /// the start time and strictly less than the end time of this segment.
        /// It is not intended to resolve boundary edge cases where ATime is exactly
        /// equal to the start or end time of the segment
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool IncludesTimeWithinBounds(DateTime time)
        {
            const double Epsilon = 1E-9;

            double testTime = time.ToOADate();
            return (testTime > StartTime.ToOADate() + Epsilon) && (testTime < EndTime.ToOADate() - Epsilon);
        }

        public string FileName(SubGridCellAddress Origin) => $"{Origin.X:d10}-{Origin.Y:d10}-({StartTime.ToOADate():F6}-{EndTime.ToOADate():F6}).sgs";

        public void Write(BinaryWriter writer)
        {
            writer.Write(StartTime.ToBinary());
            writer.Write(EndTime.ToBinary());
            writer.Write(MinElevation);
            writer.Write(MaxElevation);
       }

        public void Read(BinaryReader reader)
        {
            StartTime = DateTime.FromBinary(reader.ReadInt64());
            EndTime = DateTime.FromBinary(reader.ReadInt64());
            MinElevation = reader.ReadDouble();
            MaxElevation = reader.ReadDouble();
        }
    }
}
