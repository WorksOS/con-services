using System;
using System.IO;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;

namespace VSS.TRex.Alignments.Interfaces
{
  public interface IAlignment
  {
    /// <summary>
    /// Readonly property exposing the Alignment ID
    /// </summary>`
    Guid ID { get; set; }

    DesignDescriptor Get_DesignDescriptor();

    /// <summary>
    /// Serialises state to a binary writer
    /// </summary>
    /// <param name="writer"></param>
    void Write(BinaryWriter writer);

    /// <summary>
    /// Serialises state to a binary writer with a supplied intermediary buffer
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="buffer"></param>
    void Write(BinaryWriter writer, byte[] buffer);

    /// <summary>
    /// Serialises state in from a binary reader
    /// </summary>
    /// <param name="reader"></param>
    void Read(BinaryReader reader);

    /// <summary>
    /// Returns the real world 3D enclosing extents for the Alignment topology, including any configured vertical offset
    /// </summary>
    BoundingWorldExtent3D Extents { get; }

    /// <summary>
    /// Produces a deep clone of the Alignment
    /// </summary>
    /// <returns></returns>
    IAlignment Clone();

    /// <summary>
    /// ToString() for Alignment
    /// </summary>
    /// <returns></returns>
    string ToString();

    /// <summary>
    /// Determine if two Alignments are equal
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    bool Equals(IAlignment other);
  }
}
