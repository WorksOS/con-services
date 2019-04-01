using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Common.Types;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using VSS.TRex.Common.Utilities.Interfaces;

namespace VSS.TRex.Alignments
{
  /// <summary>
  /// Defines all the state information describing an alignment based on a design descriptor
  /// </summary>
  public class Alignment : IEquatable<IAlignment>, IBinaryReaderWriter, IAlignment
  {
    /// <summary>
    /// 3D extents bounding box enclosing the underlying design represented by the design descriptor (excluding any vertical offset(
    /// </summary>
    private readonly BoundingWorldExtent3D extents = new BoundingWorldExtent3D();

    /// <summary>
    /// Serialises state to a binary writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(ID.ToByteArray());
      DesignDescriptor.Write(writer);
      extents.Write(writer);
    }

    /// <summary>
    /// Serialises state to a binary writer with a supplied intermediary buffer
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="buffer"></param>
    public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);

    /// <summary>
    /// Serialises state in from a binary reader
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      ID = reader.ReadGuid();
      DesignDescriptor.Read(reader);
      extents.Read(reader);
    }

    /// <summary>
    /// Readonly property exposing the Alignment ID
    /// </summary>
    public Guid ID { get; set; }

    /// <summary>
    /// Readonly property exposing the design descriptor for the underlying topology surface
    /// </summary>
    public DesignDescriptor DesignDescriptor { get; }

    /// <summary>
    /// Returns the real world 3D enclosing extents for the Alignment topology, including any configured vertical offset
    /// </summary>
    public BoundingWorldExtent3D Extents
    {
      get
      {
        BoundingWorldExtent3D result = new BoundingWorldExtent3D(extents);

        // Incorporate any vertical offset from the underlying design the Alignment is based on
        result.Offset(DesignDescriptor.Offset);

        return result;
      }
    }

    /// <summary>
    /// No-arg constructor
    /// </summary>
    public Alignment()
    {
      extents = new BoundingWorldExtent3D();
      DesignDescriptor = new DesignDescriptor();
    }

    /// <summary>
    /// Constructor accepting a Binary Reader instance from which to instantiate itself
    /// </summary>
    /// <param name="reader"></param>
    public Alignment(BinaryReader reader)
    {
      Read(reader);
    }

    /// <summary>
    /// Constructor accepting full Alignment state
    /// </summary>
    /// <param name="iD">The unque identifier for the Alignment in this site model</param>
    /// <param name="designDescriptor"></param>
    /// <param name="extents_"></param>
    public Alignment(Guid iD,
      DesignDescriptor designDescriptor,
      BoundingWorldExtent3D extents_)
    {
      ID = iD;
      DesignDescriptor = designDescriptor;
      extents = extents_;
    }

    /// <summary>
    /// Produces a deep clone of the Alignment
    /// </summary>
    /// <returns></returns>
    public IAlignment Clone() => new Alignment(ID, DesignDescriptor, new BoundingWorldExtent3D(Extents));

    /// <summary>
    /// ToString() for Alignment
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return
        $"ID:{ID}, DesignID:{DesignDescriptor.DesignID}; {DesignDescriptor.Folder};{DesignDescriptor.FileName} {DesignDescriptor.Offset:F3} [{Extents}]";
    }

    /// <summary>
    /// Resolves each station/offset to a NEE value
    /// </summary>
    /// <param name="crossSectionInterval"></param>
    /// <param name="startStation"></param>
    /// <param name="endStation"></param>
    /// <param name="offsets"></param>
    /// <returns></returns>
    public List<StationOffsetPoint> GetOffsetPointsInNEE(double crossSectionInterval, double startStation, double endStation, double[] offsets)
    {
      // todo when SDK available
      return new List<StationOffsetPoint>();
    }

    /// <summary>
    /// Determine if two Alignments are equal
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(IAlignment other)
    {
      return other != null && 
             ID == other.ID &&
             DesignDescriptor.Equals(other.DesignDescriptor) &&
             Extents.Equals(other.Extents);
    }

  }
}
