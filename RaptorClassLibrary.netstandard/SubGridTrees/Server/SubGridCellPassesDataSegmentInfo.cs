using System;
using System.IO;
using VSS.VisionLink.Raptor.Common;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server
{
    public class SubGridCellPassesDataSegmentInfo
    {
        public SubGridCellPassesDataSegment Segment { get; set; }
        public DateTime StartTime { get; set; } = DateTime.MinValue;
        public DateTime EndTime { get; set; } = DateTime.MaxValue;

//        public uint FSGranuleIndex { get; set; }
//        public uint FSGranuleCount { get; set; }

        public double MinElevation { get; set; } = Consts.NullDouble;
        public double MaxElevation { get; set; } = Consts.NullDouble;

        public bool ExistsInPersistentStore { get; set; }

        public DateTime MidTime => DateTime.FromOADate((StartTime.ToOADate() + EndTime.ToOADate()) / 2);

        public SubGridCellPassesDataSegmentInfo()
        {
            //            FSGranuleIndex:= kICFSNullGranuleIndex;
            //            FSGranuleCount:= 0;
        }
        public SubGridCellPassesDataSegmentInfo(DateTime startTime, DateTime endTime,
                                                SubGridCellPassesDataSegment segment
                         //const AFSGranuleIndex : TICFSGranuleIndex;
                         //const AFSGranuleCount : Longword
                         )
        {
            StartTime = startTime;
            EndTime = endTime;
            Segment = segment;
            // FSGranuleIndex = AFSGranuleIndex;
            // FSGranuleCount = AFSGranuleCount;
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

        public string FileName(SubGridCellAddress Origin)
        {
            return string.Format("{0:d10}-{1:d10}-({2:F6}-{3:F6}).sgs", // '%.10d-%.10d(%s-%s).sgs'
                                 Origin.X, Origin.Y, StartTime.ToOADate(), EndTime.ToOADate());
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(StartTime.ToBinary());
            writer.Write(EndTime.ToBinary());
//            writer.Write(FSGranuleIndex);
//            writer.Write(FSGranuleCount);
            writer.Write(MinElevation);
            writer.Write(MaxElevation);
       }

        public void Read(BinaryReader reader)
        {
            StartTime = DateTime.FromBinary(reader.ReadInt64());
            EndTime = DateTime.FromBinary(reader.ReadInt64());
//            FSGranuleIndex = reader.ReadUInt32();
//            FSGranuleCount = reader.ReadUInt32();
            MinElevation = reader.ReadDouble();
            MaxElevation = reader.ReadDouble();
        }
    }
}
