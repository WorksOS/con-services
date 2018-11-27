using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Exceptions;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees;

namespace VSS.TRex.GridFabric.Affinity
{
  /// <summary>
  /// The key type used to drive spatial affinity key mapping for elements stored in the Ignite cache. This controls
  /// which nodes in the PSNode layer the data for this key should reside. 
  /// </summary>
  public struct SubGridSpatialAffinityKey : ISubGridSpatialAffinityKey, IBinarizable, IFromToBinary, IEquatable<SubGridSpatialAffinityKey>
    {
        private const byte VERSION_NUMBER = 1;
       
        /// <summary>
        /// The GUID for the project the subgrid data belongs to.
        /// </summary>
        public Guid ProjectUID { get; set; }

        /// <summary>
        /// The X ordinate cell address of the origin cell for the subgrid
        /// </summary>
        public uint SubGridX { get; set; }

        /// <summary>
        /// The Y ordinate cell address of the origin cell for the subgrid
        /// </summary>
        public uint SubGridY { get; set; }

        /// <summary>
        /// The segment identifier for the subgrid data. If the segment identifier is empty then the element represents
        /// the subgrid directory (or SGL file). Otherwise, the segment identifier is a string representation of the start
        /// and end times of the segment and the time duration the segment contains data for.
        /// </summary>
        public string SegmentIdentifier { get; set; }

        /// <summary>
        /// A constructor for the subgrid spatial affinity key that accepts the project and subgrid origin location
        /// and returns an instance of the spatial affinity key
        /// </summary>
        /// <param name="projectID"></param>
        /// <param name="subGridX"></param>
        /// <param name="subGridY"></param>
        /// <param name="segmentIdentifier"></param>
        public SubGridSpatialAffinityKey(Guid projectID, uint subGridX, uint subGridY, string segmentIdentifier)
        {
            ProjectUID = projectID;
            SubGridX = subGridX;
            SubGridY = subGridY;
            SegmentIdentifier = segmentIdentifier;
        }

        /// <summary>
        /// A constructor for the subgrid spatial affinity key that accepts the project and a cell address structure for
        /// the subgrid origin location and returns an instance of the spatial affinity key
        /// </summary>
        /// <param name="projectID"></param>
        /// <param name="address"></param>
        /// <param name="segmentIdentifier"></param>
        public SubGridSpatialAffinityKey(Guid projectID, SubGridCellAddress address, string segmentIdentifier)
        {
            ProjectUID = projectID;
            SubGridX = address.X;
            SubGridY = address.Y;
            SegmentIdentifier = segmentIdentifier;
        }

        /// <summary>
        /// A constructor supplying a null segment identifier semantic for contexts that do not require a segment such
        /// as the subgrid directory element
        /// </summary>
        /// <param name="projectID"></param>
        /// <param name="subGridX"></param>
        /// <param name="subGridY"></param>
        public SubGridSpatialAffinityKey(Guid projectID, uint subGridX, uint subGridY) : this(projectID, subGridX, subGridY, "")
        {
        }

        /// <summary>
        /// A constructor supplying a null segment identifier semantic for contexts that do not require a segment such
        /// as the subgrid directory element
        /// </summary>
        /// <param name="projectID"></param>
        /// <param name="address"></param>
        public SubGridSpatialAffinityKey(Guid projectID, SubGridCellAddress address) : this(projectID, address.X, address.Y, "")
        {
        }

        /// <summary>
        /// Converts the spatial segment affinity key into a string representation suitable for use as a unique string
        /// identifying this data element in the cache.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SegmentIdentifier == string.Empty
                ? $"{ProjectUID}-{SubGridX}-{SubGridY}"
                : $"{ProjectUID}-{SubGridX}-{SubGridY}-{SegmentIdentifier}";
        }

    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());

    public void ToBinary(IBinaryRawWriter writer)
    {
      writer.WriteByte(VERSION_NUMBER);
      writer.WriteGuid(ProjectUID);
      writer.WriteInt((int)SubGridX);
      writer.WriteInt((int)SubGridY);
      writer.WriteString(SegmentIdentifier);
    }

    public void FromBinary(IBinaryRawReader reader)
    {
      int version = reader.ReadByte();

      if (version != VERSION_NUMBER)
        throw new TRexSerializationVersionException(VERSION_NUMBER, version);

      ProjectUID = reader.ReadGuid() ?? Guid.Empty;
      SubGridX = (uint)reader.ReadInt();
      SubGridY = (uint)reader.ReadInt();
      SegmentIdentifier = reader.ReadString();
    }

      public bool Equals(SubGridSpatialAffinityKey other)
      {
        return ProjectUID.Equals(other.ProjectUID) && 
               SubGridX == other.SubGridX && 
               SubGridY == other.SubGridY && 
               string.Equals(SegmentIdentifier, other.SegmentIdentifier);
      }

      public override bool Equals(object obj)
      {
        if (ReferenceEquals(null, obj)) return false;
        return obj is SubGridSpatialAffinityKey other && Equals(other);
      }

      public override int GetHashCode()
      {
        unchecked
        {
          var hashCode = ProjectUID.GetHashCode();
          hashCode = (hashCode * 397) ^ (int) SubGridX;
          hashCode = (hashCode * 397) ^ (int) SubGridY;
          hashCode = (hashCode * 397) ^ (SegmentIdentifier != null ? SegmentIdentifier.GetHashCode() : 0);
          return hashCode;
        }
      }
    }
}
