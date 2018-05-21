using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.Utilities.Interfaces;

namespace VSS.TRex.Designs.Storage
{
    [Serializable]
    public class Designs : List<Design>, IBinaryReaderWriter
    {
        private const byte kMajorVersion = 1;
        private const byte kMinorVersion = 0;

        /// <summary>
        /// No-arg constructor
        /// </summary>
        public Designs()
        {
        }

        /// <summary>
        /// Constructor accepting a Binary Reader instance from which to instantiate itself
        /// </summary>
        /// <param name="reader"></param>
        public Designs(BinaryReader reader)
        {
            Read(reader);
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(kMajorVersion);
            writer.Write(kMinorVersion);
            writer.Write((int)Count);

            foreach (Design design in this)
            {
                design.Write(writer);
            }
        }

        public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);

        public void Read(BinaryReader reader)
        {
            ReadVersionFromStream(reader, out byte MajorVersion, out byte MinorVersion);

            if (MajorVersion != kMajorVersion)
            {
                throw new FormatException("Major version incorrect");
            }

            if (MinorVersion != kMinorVersion)
            {
                throw new FormatException("Minor version incorrect");
            }

            int TheCount = reader.ReadInt32();
            for (int i = 0; i < TheCount; i++)
            {
                Design design = new Design();
                design.Read(reader);
                Add(design);
            }
        }

        public void ReadVersionFromStream(BinaryReader reader, out byte MajorVersion, out byte MinorVersion)
        {
            // Load file version info
            MajorVersion = reader.ReadByte();
            MinorVersion = reader.ReadByte();
        }

        /// <summary>
        /// Create a new design in the list based on the provided details
        /// </summary>
        /// <param name="ADesignID"></param>
        /// <param name="ADesignDescriptor"></param>
        /// <param name="AExtents"></param>
        /// <returns></returns>
        public Design AddDesignDetails(Guid ADesignID,
                                       DesignDescriptor ADesignDescriptor,
                                       BoundingWorldExtent3D AExtents)
        {
            Design match = Find(x => x.ID == ADesignID);

            if (match != null)
            {
                return match;
            }

            Design design = new Design(ADesignID, ADesignDescriptor, AExtents);
            Add(design);

            return design;
        }

        /// <summary>
        /// Remove a given design from the list of designs for a site model
        /// </summary>
        /// <param name="ADesignID"></param>
        /// <returns></returns>
        public bool RemoveDesign(Guid ADesignID)
        {
            Design match = Find(x => x.ID == ADesignID);

            return match != null && Remove(match);
        }

        public Design Locate(Guid AID) => Find(x => x.ID == AID);

        public void Assign(Designs source)
        {
            Clear();

            foreach (Design design in source)
            {
                Add(design.Clone());
            }
        }

        /// <summary>
        /// Determine if the designs in this list are the same as the designs in the other list, based on ID comparison
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsSameAs(Designs other)
        {
            if (Count != other.Count)
            {
                return false;
            }

            for (int I = 0; I < Count; I++)
            {
                if (this[I].ID != other[I].ID)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Calculate the cache key for the design list
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <returns></returns>
        public static NonSpatialAffinityKey CacheKey(Guid SiteModelID) => new NonSpatialAffinityKey(SiteModelID, "Designs");
    }
}
