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
    private const byte kMajorVersion = 1;
    private const byte kMinorVersion = 3;

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
      writer.Write(kMajorVersion);
      writer.Write(kMinorVersion);
      writer.Write(Count);

      foreach (IAlignment alignment in this)
      {
        alignment.Write(writer);
      }
    }

    public void Read(BinaryReader reader)
    {
      ReadVersionFromStream(reader, out byte MajorVersion, out byte MinorVersion);

      if (MajorVersion != kMajorVersion)
      {
        throw new FormatException("Major version incorrect");
      }

      if (MinorVersion != kMinorVersion)
      {
        throw new FormatException("Minor version incorrect");
      }

      int theCount = reader.ReadInt32();
      for (int i = 0; i < theCount; i++)
      {
        Alignment alignment = new Alignment();
        alignment.Read(reader);
        Add(alignment);
      }
    }

    private void ReadVersionFromStream(BinaryReader reader, out byte MajorVersion, out byte MinorVersion)
    {
      // Load file version info
      MajorVersion = reader.ReadByte();
      MinorVersion = reader.ReadByte();
    }

    public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);


    public void Assign(IAlignments source)
    {
      Clear();

      foreach (IAlignment alignment in source)
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
      IAlignment match = Find(x => x.ID == alignmentUid);

      if (match != null)
      {
        return match;
      }

      IAlignment alignment = new Alignment(alignmentUid, designDescriptor, extents);
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
      IAlignment match = Find(x => x.ID == alignmentUid);

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
      for (int i = 0; i < Count; i++)
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

      for (int I = 0; I < Count; I++)
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
