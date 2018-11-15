using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Designs.GridFabric.Arguments
{
  public class CalculateDesignProfileArgument : BaseApplicationServiceRequestArgument, IEquatable<CalculateDesignProfileArgument>
  {
    /// <summary>
    /// The path along which the profile will be calculated
    /// </summary>
    public XYZ[] ProfilePath { get; }

    /// <summary>
    /// The cell stepping size to move between points in the patch being interpolated
    /// </summary>
    public double CellSize { get; set; }

    /// <summary>
    /// The descriptor of the design file the elevations are to be interpolated from
    /// </summary>
    public DesignDescriptor DesignDescriptor { get; set; }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public CalculateDesignProfileArgument()
    {
    }

    /// <summary>
    /// Constructor taking the full state of the elevation patch computation operation
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="cellSize"></param>
    /// <param name="designDescriptor"></param>
    /// <param name="profilePath"></param>
    // /// <param name="processingMap"></param>
    public CalculateDesignProfileArgument(Guid siteModelID,
                                          double cellSize,
                                          DesignDescriptor designDescriptor,
                                          XYZ[] profilePath) : this()
    {
      ProjectID = siteModelID;
      CellSize = cellSize;
      DesignDescriptor = designDescriptor;
      ProfilePath = profilePath;
    }

    /// <summary>
    /// Overloaded ToString to add argument properties
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return base.ToString() + $" -> ProjectUID:{ProjectID}, CellSize:{CellSize}, Design:{DesignDescriptor}";
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteDouble(CellSize);

      DesignDescriptor.ToBinary(writer);

      // todo: Add profile path
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      CellSize = reader.ReadDouble();

      DesignDescriptor.FromBinary(reader);

      // todo: Add profile path
    }

    public bool Equals(CalculateDesignProfileArgument other)
    {
      // todo: Add profile path

      return base.Equals(other) && 
             CellSize.Equals(other.CellSize) && 
             DesignDescriptor.Equals(other.DesignDescriptor);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((CalculateDesignProfileArgument) obj);
    }

    public override int GetHashCode()
    {
      // todo: Add profile path

      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ CellSize.GetHashCode();
        hashCode = (hashCode * 397) ^ DesignDescriptor.GetHashCode();
        return hashCode;
      }
    }
  }
}
