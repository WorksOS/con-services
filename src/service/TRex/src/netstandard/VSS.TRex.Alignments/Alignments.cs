using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Alignments.Interfaces;

namespace VSS.TRex.Alignments
{
  public class Alignments : List<IAlignment>, IComparable<IAlignment>, IAlignments
  {
    private const byte MAJOR_VERSION = 1;
    private const byte MINOR_VERSION = 3;

    /// <summary>
    /// No-arg constructor
    /// </summary>
    public Alignments()
    {
    }

    /// <summary>
    /// Constructor accepting a Binary Reader instance from which to instantiate itself
    /// </summary>
    /// <param name="reader"></param>
    public Alignments(BinaryReader reader)
    {
      Read(reader);
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(MAJOR_VERSION);
      writer.Write(MINOR_VERSION);
      writer.Write(Count);

      foreach (var alignment in this)
      {
        alignment.Write(writer);
      }
    }

    public void Read(BinaryReader reader)
    {
      ReadVersionFromStream(reader, out var majorVersion, out var minorVersion);

      if (majorVersion != MAJOR_VERSION)
      {
        throw new FormatException("Major version incorrect");
      }

      if (minorVersion != MINOR_VERSION)
      {
        throw new FormatException("Minor version incorrect");
      }

      var theCount = reader.ReadInt32();
      for (var i = 0; i < theCount; i++)
      {
        var alignment = new Alignment();
        alignment.Read(reader);
        Add(alignment);
      }
    }

    private void ReadVersionFromStream(BinaryReader reader, out byte majorVersion, out byte minorVersion)
    {
      // Load file version info
      majorVersion = reader.ReadByte();
      minorVersion = reader.ReadByte();
    }

    public void Assign(IAlignments source)
    {
      Clear();

      foreach (var alignment in source)
      {
        Add(alignment);
      }
    }
    /// <summary>
    /// Create a new Alignment in the list based on the provided details
    /// </summary>
    /// <param name="alignmentUid"></param>
    /// <param name="designDescriptor"></param>
    /// <param name="extents"></param>
    /// <returns></returns>
    public IAlignment AddAlignmentDetails(Guid alignmentUid,
      DesignDescriptor designDescriptor,
      BoundingWorldExtent3D extents)
    {
      var match = Find(x => x.ID == alignmentUid);

      if (match != null)
      {
        return match;
      }

      var alignment = new Alignment(alignmentUid, designDescriptor, extents);
      Add(alignment);

      return alignment;
    }

    /// <summary>
    /// Remove a given Alignment from the list of Alignments for a site model
    /// </summary>
    /// <param name="alignmentUid"></param>
    /// <returns></returns>
    public bool RemoveAlignment(Guid alignmentUid)
    {
      var match = Find(x => x.ID == alignmentUid);

      return match != null && Remove(match);
    }

    /// <summary>
    /// Locates a Alignment in the list with the given GUID
    /// </summary>
    /// <param name="alignmentUid"></param>
    /// <returns></returns>
    public IAlignment Locate(Guid alignmentUid)
    {
      // Note: This happens a lot and the for loop is faster than foreach or Find(x => x.ID)
      // If numbers of Alignments become large a Dictionary<Guid, SS> would be good...
      for (var i = 0; i < Count; i++)
        if (this[i].ID == alignmentUid)
          return this[i];

      return null;
    }
    
    /// <summary>
    /// Determine if the Alignments in this list are the same as the Alignments in the other list, based on ID comparison
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsSameAs(Alignments other)
    {
      if (Count != other.Count)
      {
        return false;
      }

      for (var I = 0; I < Count; I++)
      {
        if (this[I].ID != other[I].ID)
        {
          return false;
        }
      }

      return true;
    }

    public int CompareTo(IAlignment other)
    {
      throw new NotImplementedException();
    }
  }
}
