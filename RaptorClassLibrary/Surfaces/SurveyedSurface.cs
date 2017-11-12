using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Geometry;

namespace VSS.VisionLink.Raptor.Surfaces
{
    public class SurveyedSurface : IEquatable<SurveyedSurface>
    {
        long FID = long.MinValue;
        DesignDescriptor FDesignDescriptor;
        DateTime FAsAtDate = DateTime.MinValue;
        BoundingWorldExtent3D FExtents;

        public void Write(BinaryWriter writer)
        {
            writer.Write(FID);
            FDesignDescriptor.Write(writer);
            writer.Write(FAsAtDate.ToBinary());
            FExtents.Write(writer);
        }

        public void Read(BinaryReader reader)
        {
            FID = reader.ReadInt64();
            FDesignDescriptor.Read(reader);
            FAsAtDate = DateTime.FromBinary(reader.ReadInt64());
            FExtents.Read(reader);
        }


        public long ID { get { return FID; } }
        public DesignDescriptor DesignDescriptor { get { return FDesignDescriptor; } }
        public DateTime AsAtDate { get { return FAsAtDate; } }
        public BoundingWorldExtent3D Extents { get { return FExtents; } }

        public SurveyedSurface()
        {
        }

        public SurveyedSurface(long AID,
                               DesignDescriptor ADesignDescriptor,
                               DateTime AAsAtDate) : base()
        {
            FID = AID;
            FDesignDescriptor = ADesignDescriptor;
            FAsAtDate = AAsAtDate;
        }

        public SurveyedSurface Clone() => new SurveyedSurface(FID, FDesignDescriptor, FAsAtDate);

        public override string ToString()
        {
            return String.Format("ID:{0}, DesignID:{1} {2}; {4};{5};{6} {7:F3} [{8}]",
                            FID,
                             FDesignDescriptor.DesignID,
                             FAsAtDate,
                             FDesignDescriptor.FileSpace, FDesignDescriptor.Folder, FDesignDescriptor.FileName,
                             FDesignDescriptor.Offset,
                             FExtents);
        }

        public bool Equals(SurveyedSurface other)
        {
            return (ID == other.ID) &&
                   FDesignDescriptor.Equals(other.DesignDescriptor) &&
                   (FAsAtDate == other.AsAtDate) &&
                   (FExtents.Equals(other.Extents));

        }
    }
}
