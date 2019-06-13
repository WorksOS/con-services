using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Common.Types;

namespace VSS.TRex.Alignments.Interfaces
{
  public interface IAlignment
  {
    /// <summary>
    /// Serializes state to a binary writer
    /// </summary>
    /// <param name="writer"></param>
    void Write(BinaryWriter writer);

    /// <summary>
    /// Serializes state in from a binary reader
    /// </summary>
    /// <param name="reader"></param>
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

    List<StationOffsetPoint> GetOffsetPointsInNEE(double crossSectionInterval, double startStation, double endStation, double[] offsets);
  }
}
