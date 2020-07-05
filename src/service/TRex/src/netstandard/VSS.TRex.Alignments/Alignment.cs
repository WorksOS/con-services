using System;
using System.IO;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;

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
    private readonly BoundingWorldExtent3D _extents = new BoundingWorldExtent3D();

    /// <summary>
    /// Serializes state to a binary writer
    /// </summary>
    public void Write(BinaryWriter writer)
    {
      writer.Write(ID.ToByteArray());
      DesignDescriptor.Write(writer);
      _extents.Write(writer);
    }

    /// <summary>
    /// Serializes state in from a binary reader
    /// </summary>
    public void Read(BinaryReader reader)
    {
      ID = reader.ReadGuid();
      DesignDescriptor.Read(reader);
      _extents.Read(reader);
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
    public BoundingWorldExtent3D Extents => new BoundingWorldExtent3D(_extents);

    /// <summary>
    /// No-arg constructor
    /// </summary>
    public Alignment()
    {
      _extents = new BoundingWorldExtent3D();
      DesignDescriptor = new DesignDescriptor();
    }

    /// <summary>
    /// Constructor accepting a Binary Reader instance from which to instantiate itself
    /// </summary>
    public Alignment(BinaryReader reader)
    {
      Read(reader);
    }

    /// <summary>
    /// Constructor accepting full Alignment state
    /// </summary>
    /// <param name="iD">The unique identifier for the Alignment in this site model</param>
    /// <param name="designDescriptor"></param>
    /// <param name="extents"></param>
    public Alignment(Guid iD,
      DesignDescriptor designDescriptor,
      BoundingWorldExtent3D extents)
    {
      ID = iD;
      DesignDescriptor = designDescriptor;
      _extents = extents;
    }

    /// <summary>
    /// Produces a deep clone of the Alignment
    /// </summary>
    public IAlignment Clone() => new Alignment(ID, DesignDescriptor, new BoundingWorldExtent3D(Extents));

    /// <summary>
    /// ToString() for Alignment
    /// </summary>
    public override string ToString()
    {
      return
        $"ID:{ID}, DesignID:{DesignDescriptor.DesignID}; {DesignDescriptor.Folder};{DesignDescriptor.FileName} [{Extents}]";
    }

    /// <summary>
    /// Determine if two Alignments are equal
    /// </summary>
    public bool Equals(IAlignment other)
    {
      return other != null &&
             ID == other.ID &&
             DesignDescriptor.Equals(other.DesignDescriptor) &&
             Extents.Equals(other.Extents);
    }
  }
}
