using System;
using System.IO;
using VSS.TRex.Common.Utilities.ExtensionMethods;

namespace VSS.TRex.SubGridTrees.Server
{
    /// <summary>
    /// Describes a relationship between a machine represented by a GUID, and an internal numeric identifier used
    /// to reduce the storage required in the cell passes to identify the machine
    /// </summary>
    public struct SubgridCellSegmentMachineReference
    {
        /// <summary>
        /// Internal numeric identifier for the machin in a segment
        /// </summary>
        public short _SiteModelMachineIndex;

        /// <summary>
        /// Globally unique Guid identifier for the machine
        /// </summary>
        public Guid _MachineID;

        /// <summary>
        /// Read the mapping of Guid to numeric identifier usign the supplied reader
        /// </summary>
        /// <param name="reader"></param>
        public void Read(BinaryReader reader)
        {
            _SiteModelMachineIndex = reader.ReadInt16();
            _MachineID = reader.ReadGuid();
        }

        /// <summary>
        /// Write the mapping of Guid to numeric identifier usign the supplied writer
        /// </summary>
        /// <param name="writer"></param>
        public void Write(BinaryWriter writer)
        {
            writer.Write(_SiteModelMachineIndex);
            writer.Write(_MachineID.ToByteArray());
        }
    }
}
