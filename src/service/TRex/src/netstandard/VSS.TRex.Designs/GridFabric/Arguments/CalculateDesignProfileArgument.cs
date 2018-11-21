using System;
using System.Linq;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ExtensionMethods;

namespace VSS.TRex.Designs.GridFabric.Arguments
{
  public class CalculateDesignProfileArgument : BaseApplicationServiceRequestArgument, IEquatable<CalculateDesignProfileArgument>
  {
    private const byte versionNumber = 1;

    /// <summary>
    /// The path along which the profile will be calculated
    /// </summary>
    public XYZ[] ProfilePath { get; set; } = new XYZ[0];

    /// <summary>
    /// The cell stepping size to move between points in the patch being interpolated
    /// </summary>
    public double CellSize { get; set; }

    /// <summary>
    /// The guid identifying the design to compute the profile over
    /// </summary>
    public Guid DesignUid { get; set; }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public CalculateDesignProfileArgument()
    {
    }

    /// <summary>
    /// Constructor taking the full state of the elevation patch computation operation
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="cellSize"></param>
    /// <param name="designUid"></param>
    /// <param name="profilePath"></param>
    // /// <param name="processingMap"></param>
    public CalculateDesignProfileArgument(Guid projectUid,
                                          double cellSize,
                                          Guid designUid,
                                          XYZ[] profilePath) : this()
    {
      ProjectID = projectUid;
      CellSize = cellSize;
      DesignUid = designUid;
      ProfilePath = profilePath;
    }

    /// <summary>
    /// Overloaded ToString to add argument properties
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return base.ToString() + $" -> ProjectUID:{ProjectID}, CellSize:{CellSize}, Design:{DesignUid}, {ProfilePath.Length} vertices";
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteByte(versionNumber);

      writer.WriteDouble(CellSize);

      writer.WriteGuid(DesignUid);

      writer.WriteInt(ProfilePath.Length);
      foreach (var pt in ProfilePath)
      {
        pt.ToBinaryUnversioned(writer);
      }
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      byte version = reader.ReadByte();

      if (version != versionNumber)
        throw new TRexSerializationVersionException(versionNumber, version);

      CellSize = reader.ReadDouble();
      DesignUid = reader.ReadGuid() ?? Guid.Empty;

      var count = reader.ReadInt();

      ProfilePath = new XYZ[count];
      for (int i = 0; i < count; i++)
      {       
        ProfilePath[i] = ProfilePath[i].FromBinaryUnversioned(reader);
      }
    }

    public bool Equals(CalculateDesignProfileArgument other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;

      if (ProfilePath.Length != other.ProfilePath.Length) return false;

      return base.Equals(other) && 
             !ProfilePath.Where((pt, i) => !pt.Equals(other.ProfilePath[i])).Any() &&
             CellSize.Equals(other.CellSize) && 
             DesignUid.Equals(other.DesignUid);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != GetType()) return false;
      return Equals((CalculateDesignProfileArgument) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ (ProfilePath != null ? ProfilePath.GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ CellSize.GetHashCode();
        hashCode = (hashCode * 397) ^ DesignUid.GetHashCode();
        return hashCode;
      }
    }
  }
}
