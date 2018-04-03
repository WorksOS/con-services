using System;
using System.IO;
using VSS.VisionLink.Raptor.Designs;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.Utilities.Interfaces;

namespace VSS.VisionLink.Raptor.Surfaces
{
    /// <summary>
    /// Defines all the state information describing a surveyed surface based on a design descriptor
    /// </summary>
    [Serializable]
    public class SurveyedSurface : IEquatable<SurveyedSurface>, IBinaryReaderWriter
    {
        /// <summary>
        /// Unique identified for the surveyd surfaced
        /// </summary>
        long FID = long.MinValue;

        /// <summary>
        /// Underlying design the surveyed surface is based on
        /// </summary>
        DesignDescriptor FDesignDescriptor;

        /// <summary>
        /// The effective data the surveyed surface represents a snapshot of the ground topology recorded in the design descriptor+
        /// </summary>
        DateTime FAsAtDate = DateTime.MinValue;

        /// <summary>
        /// 3D extents bounding box enclosing the underlying design represented by the design descriptor (excluding any vertical offset(
        /// </summary>
        BoundingWorldExtent3D FExtents;

        /// <summary>
        /// Serialises state to a binary writer
        /// </summary>
        /// <param name="writer"></param>
        public void Write(BinaryWriter writer)
        {
            writer.Write(FID);
            FDesignDescriptor.Write(writer);
            writer.Write(FAsAtDate.ToBinary());
            FExtents.Write(writer);
        }

        /// <summary>
        /// Serialises state to a binary writer with a supplied intermediary buffer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="buffer"></param>
        public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);

        /// <summary>
        /// Serialises state in from a binary reader
        /// </summary>
        /// <param name="reader"></param>
        public void Read(BinaryReader reader)
        {
            FID = reader.ReadInt64();
            FDesignDescriptor.Read(reader);
            FAsAtDate = DateTime.FromBinary(reader.ReadInt64());
            FExtents.Read(reader);
        }

        /// <summary>
        /// Readonly property exposing the surveyed surface ID
        /// </summary>
        public long ID { get { return FID; } }

        /// <summary>
        /// Readonlhy property exposing the design decriptor for the underlying topology surface
        /// </summary>
        public DesignDescriptor DesignDescriptor { get { return FDesignDescriptor; } }

        /// <summary>
        /// Readonly attribute for AsAtData
        /// </summary>
        public DateTime AsAtDate { get { return FAsAtDate; } }

        /// <summary>
        /// Returns the real world 3D enclosing extents for the surveyed surface topology, including any configured vertical offset
        /// </summary>
        public BoundingWorldExtent3D Extents
        {
            get
            {
                BoundingWorldExtent3D result = FExtents;

                // Incorporate any vertical offset from the underlying design the surveyed surface is based on
                result.Offset(DesignDescriptor.Offset);

                return result;
            }
        }

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
            return string.Format("ID:{0}, DesignID:{1} {2}; {3};{4};{5} {6:F3} [{7}]",
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
