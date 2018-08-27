using System;
using System.IO;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;

namespace VSS.TRex.SurveyedSurfaces.Interfaces
{
  public interface ISurveyedSurface
  {
    /// <summary>
    /// Readonly attribute for AsAtData
    /// </summary>
    DateTime AsAtDate { get; set; }

    /// <summary>
    /// Readonly property exposing the surveyed surface ID
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
    /// Returns the real world 3D enclosing extents for the surveyed surface topology, including any configured vertical offset
    /// </summary>
    BoundingWorldExtent3D Extents { get; }

    /// <summary>
    /// Produces a deep clone of the surveyed surface
    /// </summary>
    /// <returns></returns>
    ISurveyedSurface Clone();

    /// <summary>
    /// ToString() for SurveyedSurface
    /// </summary>
    /// <returns></returns>
    string ToString();

    /// <summary>
    /// Determine if two surveyed surfaces are equal
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    bool Equals(ISurveyedSurface other);
  }
}
