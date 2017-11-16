using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.Utilities.Interfaces;

namespace VSS.VisionLink.Raptor.Surfaces
{
    [Serializable]
    public class SurveyedSurface : IEquatable<SurveyedSurface>, IBinaryReaderWriter
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

        public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);

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

        /// <summary>
        /// No-arg constructor
        /// </summary>
        public SurveyedSurface()
        {
        }

        /// <summary>
        /// Constructor accepting a Binary Reader instance from which to instantiate itself
        /// </summary>
        /// <param name="reader"></param>
        public SurveyedSurface(BinaryReader reader) : base()
        {
            Read(reader);
        }
        
        /// <summary>
        /// Constructor accepting full surveyed surface state
        /// </summary>
        /// <param name="AID"></param>
        /// <param name="ADesignDescriptor"></param>
        /// <param name="AAsAtDate"></param>
        public SurveyedSurface(long AID,
                               DesignDescriptor ADesignDescriptor,
                               DateTime AAsAtDate,
                               BoundingWorldExtent3D AExtents) : base()
        {
            FID = AID;
            FDesignDescriptor = ADesignDescriptor;
            FAsAtDate = AAsAtDate;
            FExtents = AExtents;
        }

        /// <summary>
        /// Produces a deep clone of the surveyed surface
        /// </summary>
        /// <returns></returns>
        public SurveyedSurface Clone() => new SurveyedSurface(FID, FDesignDescriptor, FAsAtDate, FExtents);

        /// <summary>
        /// ToString() for SurveyedSurface
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("ID:{0}, DesignID:{1} {2}; {3};{4};{5} {6:F3} [{7}]",
                            FID,
                             FDesignDescriptor.DesignID,
                             FAsAtDate,
                             FDesignDescriptor.FileSpace, FDesignDescriptor.Folder, FDesignDescriptor.FileName,
                             FDesignDescriptor.Offset,
                             FExtents);
        }

        /// <summary>
        /// Determine if two surveyed surfaces are equal
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(SurveyedSurface other)
        {
            return (ID == other.ID) &&
                   FDesignDescriptor.Equals(other.DesignDescriptor) &&
                   (FAsAtDate == other.AsAtDate) &&
                   (FExtents.Equals(other.Extents));
        }
    }
}
