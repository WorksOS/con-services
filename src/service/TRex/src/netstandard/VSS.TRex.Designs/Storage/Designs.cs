using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.Common;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.Storage
{
    public class Designs : List<IDesign>, IDesigns
    {
        private const byte VERSION_NUMBER = 1;

        /// <summary>
        /// No-arg constructor
        /// </summary>
        public Designs()
        {
        }

        public void Write(BinaryWriter writer)
        {
            VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

            writer.Write((int)Count);

            foreach (Design design in this)
            {
                design.Write(writer);
            }
        }

        public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);

        public void Read(BinaryReader reader)
        {
            VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

            int TheCount = reader.ReadInt32();
            for (int i = 0; i < TheCount; i++)
            {
                Design design = new Design();
                design.Read(reader);
                Add(design);
            }
        }

        /// <summary>
        /// Create a new design in the list based on the provided details
        /// </summary>
        /// <param name="ADesignID"></param>
        /// <param name="ADesignDescriptor"></param>
        /// <param name="AExtents"></param>
        /// <returns></returns>
        public IDesign AddDesignDetails(Guid ADesignID,
                                       DesignDescriptor ADesignDescriptor,
                                       BoundingWorldExtent3D AExtents)
        {
            IDesign match = Find(x => x.ID == ADesignID);

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
            IDesign match = Find(x => x.ID == ADesignID);

            return match != null && Remove(match);
        }

        public IDesign Locate(Guid AID) => Find(x => x.ID == AID);
    }
}
