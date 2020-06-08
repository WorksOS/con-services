using System;
using System.IO;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;

namespace VSS.TRex.Alignments.Interfaces
{
  public interface IAlignment
  {
    /// <summary>
    /// Serializes state to a binary writer
    /// </summary>
    void Write(BinaryWriter writer);

    /// <summary>
    /// Serializes state in from a binary reader
    /// </summary>
    void Read(BinaryReader reader);

    /// <summary>
    /// The internal identifier of the design
    /// </summary>
    Guid ID { get; }

    DesignDescriptor DesignDescriptor { get; }

    /// <summary>
    /// Returns the real world 3D enclosing extents for the Alignment topology, including any configured vertical offset
    /// </summary>
    BoundingWorldExtent3D Extents { get; }

    /// <summary>
    /// Produces a deep clone of the Alignment
    /// </summary>
    IAlignment Clone();

    /// <summary>
    /// ToString() for Alignment
    /// </summary>
    string ToString();

    /// <summary>
    /// Determine if two Alignments are equal
    /// </summary>
    /// <param name="other"></param>
    bool Equals(IAlignment other);
  }
}
