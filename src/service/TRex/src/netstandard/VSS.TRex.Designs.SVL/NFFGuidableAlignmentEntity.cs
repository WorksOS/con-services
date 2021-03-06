﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using VSS.TRex.Common;
using VSS.TRex.Designs.SVL.Utilities;
using VSS.TRex.Geometry;
using Range = VSS.TRex.Common.Utilities.Range;

namespace VSS.TRex.Designs.SVL
{
  public class NFFGuidableAlignmentEntity : NFFStationedLineworkEntity
  {
    /*---------------------------------------------------------------------------
   Name: NFFGuidableAlignmentEntity

   Comments: A compound entity containing a collection of NFFStationedLineworkEntity
   objects forming a Guidance Alignment.

   Within NFFFile object each Guidance Alignment instance is associated with a
   NFFNamedGuidanceID instance.

   NOTE: In case of Guidance Alignment in NFF file being written for Site Vision
   guidance platform, only the Master guidance alignment should have it's (and
   contained entities) kNFFElementHeaderHasStationing flag set and and Station values
   written to file.However within SVO Station values are written to
   NFFStationedLineworkEntity instances and subsequently refered to even when the
   kNFFElementHasStationing flag is NOT set.
  ---------------------------------------------------------------------------*/

    public NFFGuidableAlignmentOwnedEntitiesList Entities;

    public NFFGuidableAlignmentEntity()
    {
      _headerFlags = NFFConsts.kNFFElementHeaderHasGuidanceID;

      Entities = new NFFGuidableAlignmentOwnedEntitiesList();
    }

    protected override void SetHeaderFlags(byte Value)
    {
      // NFFGuidableAlignmentEntity class MUST have GuidanceID
      Debug.Assert((Value & NFFConsts.kNFFElementHeaderHasGuidanceID) != 0x00, "NFFGuidableAlignmentEntity class MUST have GuidanceID");

      _headerFlags = Value;

      // Map through to contained entities
      for (int I = 0; I < Entities.Count; I++)
        Entities[I].HeaderFlags = HeaderFlags;
    }

    protected override void SetGuidanceID(int Value)
    {
      base.SetGuidanceID(Value);

      // Map through to contained entities
      for (int I = 0; I < Entities.Count; I++)
        Entities[I].GuidanceID = GuidanceID;
    }

    protected override double GetStartStation()
    {
      return Entities.Count > 0 ? Entities.First().StartStation : Consts.NullDouble;
    }

    protected override void SetStartStation(double Value)
    {
      // Illegal to set StartStation as it is determined by the StartStation of the first Element

      // Could possibly write new <StartStation> value through to first element and
      // bubble change in offset through to all subsequent entities, however this functionality
      // hasn't been required at this point.
      // This call is ignored in TRex

      //if not fSuppressAssertions then
      //Assert(False);
    }

    protected override double GetEndStation()
    {
      return Entities.Count > 0 ? Entities.Last().EndStation : Consts.NullDouble;
    }

    // NormaliseArcs scans the elements in the guidance alignment and swaps the
    // 'WasClockWise' flag in them to make then consistent with the sense of
    // direction along the guidance alignment. This is necessary due to the
    // way arcs are stored in clockwise direction only in SVL files, and do
    // not retain the original sense of direction from the arc that was originally
    // used to define it.
    // procedure NormaliseArcs;

    //      procedure Assign(Entity: NFFLineworkEntity); override;
    //      property Entities : NFFGuidableAlignmentOwnedEntitiesList read FEntities;

    //      procedure DumpToText(Stream: TTextDumpStream; const OriginX, OriginY : Double); override;
    //      Procedure SaveToNFFStream(Stream : TStream;
    //  const OriginX, OriginY : Double;
    //                                FileVersion : NFFFileVersion); override;

    public override void LoadFromNFFStream(BinaryReader reader,
      double OriginX, double OriginY,
      bool HasGuidanceID,
      NFFFileVersion FileVersion)
    {
      NFFLineWorkElementType EntityType;

      // The file version passed into here is ignored in favour of the file version contained in the header information

      Debug.Assert(FileVersion == NFFFileVersion.Version_Undefined,
        "Specific file version sent to NFFGuidableAlignmentEntity.LoadFromNFFStream");

      var Header = new NFFLineworkGridFileHeader();

      var b = reader.ReadBytes(Marshal.SizeOf(Header));

      var handle = GCHandle.Alloc(b, GCHandleType.Pinned);
      Header = (NFFLineworkGridFileHeader) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(NFFLineworkGridFileHeader));

      if (NFFUtils.MagicNumberToANSIString(Header.MagicNumber) != NFFConsts.kNFFLineworkFileMagicNumber)
        throw new IOException($"Expected {NFFConsts.kNFFLineworkFileMagicNumber} as the magic number, not: {Header.MagicNumber}");

      if (!NFFUtils.SetFileVersionFromMinorMajorVersionNumbers(Header.MajorVer, Header.MinorVer, out FileVersion))
        throw new IOException($"Unexpected file version, Major = {Header.MajorVer}, Minor - {Header.MinorVer}");

      if (FileVersion < NFFFileVersion.Version1_2)
        throw new IOException("File version less than 1.2 - unsupported");

      var factory = new NFFElementFactory();

      // Read the list of items from the stream...
      while (reader.BaseStream.Position < reader.BaseStream.Length)
      {
        byte EntityTypeVal = reader.ReadByte();

        // The flags moved into a following byte after the element type in v1.5

        if (FileVersion >= NFFFileVersion.Version1_5)
          EntityType = (NFFLineWorkElementType) EntityTypeVal;
        else
          EntityType = (NFFLineWorkElementType) (EntityTypeVal & 0x0f);

        if (EntityType == NFFLineWorkElementType.kNFFLineWorkLineElement)
          return;

        byte FlagsByte = 0;
        if (FileVersion >= NFFFileVersion.Version1_5)
        {
          // v1.5 and later store the flags as a separate byte. However, the flags
          // that were in the most significant 4 bits were being thrown away in favour
          // or a separate flags byte stored in each entity. So we will preserve this
          // behaviour for post v1.4 too
          FlagsByte = reader.ReadByte();
        }
        else
        {
          if ((EntityTypeVal & NFFConsts.kNFFHasGuidanceID) != 0)
            FlagsByte |= NFFConsts.kNFFElementHeaderHasGuidanceID;

          if ((EntityTypeVal & NFFConsts.kNFFHasStationing) != 0)
            FlagsByte |= NFFConsts.kNFFElementHeaderHasStationing;

          if ((EntityTypeVal & NFFConsts.kNFFHasHeight) != 0)
            FlagsByte |= NFFConsts.kNFFElementHeaderHasElevation;
        }

        if (EntityType == NFFLineWorkElementType.kNFFLineworkEndElement)
          return;

        var Entity = factory.NewElement(EntityType);

        if (!(Entity is NFFStationedLineworkEntity))
        {
          // Inappropriate entity type in file
          throw new Exception("Non stationed element created from guidable alignment geometry");
        }

        Entity.HeaderFlags = FlagsByte;
        Entity.LoadFromNFFStream(reader,
          Header.Origin.X, Header.Origin.Y,
          HasGuidanceID, FileVersion);

        if (Entities.Count == 0)
        {
          // Loading first contained entity, write entities <Flags> and <GuidanceID>
          // to self BEFORE adding it
          GuidanceID = (Entity as NFFStationedLineworkEntity).GuidanceID;
          HeaderFlags = Entity.HeaderFlags;
        }

        Entities.Add(Entity as NFFStationedLineworkEntity);
      }
    }

//      Procedure SaveToStream(Stream : TStream); override;
//      Procedure LoadFromStream(Stream : TStream); override;

    public override BoundingWorldExtent3D BoundingBox()
    {
      if (Entities.Count == 0)
        return new BoundingWorldExtent3D(Consts.NullDouble, Consts.NullDouble, Consts.NullDouble, Consts.NullDouble);

      var Result = Entities.FirstOrDefault().BoundingBox();

      for (int I = 1; I < Entities.Count; I++)
        Result.Include(Entities[I].BoundingBox());

      return Result;
    }

    public bool GetHeightRange(out double MinZ, out double MaxZ)
    {
      var Bound = BoundingWorldExtent3D.Inverted();

      for (int I = 0; I < Entities.Count; I++)
      {
        Bound.Include(Entities[I].GetStartPoint().Z);
        Bound.Include(Entities[I].GetEndPoint().Z);
      }

      MinZ = Bound.IsValidHeightExtent ? Bound.MinZ : Consts.NullDouble;
      MaxZ = Bound.IsValidHeightExtent ? Bound.MaxZ : Consts.NullDouble;

      return Bound.IsValidHeightExtent;
    }

    //      Procedure SaveToCompoundDoc(const FileName : TFileName;
    //  const OriginX, OriginY : Integer);

    public void LoadFromCompoundDoc(NFFFile owner, BinaryReader reader, string Filename)
    {
      reader.BaseStream.Position = owner.StreamInfoList.Locate(Filename).Offset;

      //try
      //fSuppressAssertions:= True;
      LoadFromNFFStream(reader, Consts.NullDouble, Consts.NullDouble, true, NFFFileVersion.Version_Undefined);
      //  finally
      //fSuppressAssertions:= False;
      //end;
    }

    //  Procedure Sort;

    public override bool HasValidHeight()
    {
      if (ControlFlag_NullHeightAllowed)
        return true;

      foreach (var entity in Entities)
        if (!entity.HasValidHeight())
          return false;

      return true;
    }
    //      Function HasCrossSlopes : Boolean; Override;

    public bool IsMasterAlignment() => ((HeaderFlags & NFFConsts.kNFFElementHeaderHasStationing) != 0x00);

    // ComputeStnOfs takes a plan position and finds the closest element in the
    // alignment for which the plan position converts to a valid station and offset
    // coordinate. The station and offset value for that station with respect to
    // that element is then returned to the caller.
    public override void ComputeStnOfs(double X, double Y, out double Stn, out double Ofs)
    {
      NFFStationedLineworkEntity element = null;
      ComputeStnOfs(X, Y, out Stn, out Ofs, ref element);
    }

    public void ComputeStnOfs(double X, double Y, out double Stn, out double Ofs, ref NFFStationedLineworkEntity Element)
    {
      double TestStn, TestOfs;

      Stn = Consts.NullDouble;
      Ofs = Consts.NullDouble;

      // Check the given element to see if it matches. If so then return the answer
      if (Element != null)
      {
        Element.ComputeStnOfs(X, Y, out TestStn, out TestOfs);

        if (TestStn != Consts.NullDouble && TestOfs != Consts.NullDouble)
        {
          Ofs = TestOfs;
          Stn = TestStn;
          return;
        }
      }

      Element = null;
      for (int I = 0; I < Entities.Count; I++)
      {
        Entities[I].ComputeStnOfs(X, Y, out TestStn, out TestOfs);

        if (TestStn != Consts.NullDouble && TestOfs != Consts.NullDouble &&
            (Ofs == Consts.NullDouble || Math.Abs(TestOfs) < Math.Abs(Ofs)))
        {
          Ofs = TestOfs;
          Stn = TestStn;
          Element = Entities[I];
        }
      }
    }

    // ComputeXY takes a station/offset position and finds the element in the
    // alignment for which the station value corresponds and then determines the
    // plan position of the point at the given offset distance from the position
    // of the element corresponeding to the given station. This plan position
    // is then returned to the caller.
    public override void ComputeXY(double Stn, double Ofs, out double X, out double Y)
    {
      X = Consts.NullDouble;
      Y = Consts.NullDouble;

      LocateEntityAtStation(Stn, out var Entity);
      Entity?.ComputeXY(Stn, Ofs, out X, out Y);
    }

    //      Procedure Reverse; Overload; Override;
    //      procedure Reverse(const StartIdx, EndIdx : Integer); Overload; Override;

    // LocateEntityAtStation takes a station value and locates the element
    // within which the station value lies.
    public void LocateEntityAtStation(double Stn, out NFFStationedLineworkEntity Element)
    {
      Element = null;

      for (int I = 0; I < Entities.Count; I++)
      {
        double Eps1;
        if (I == 0)
          Eps1 = 1E-4;
        else if (Entities[I - 1].EndStation < Entities[I].StartStation)
          Eps1 = 1E-4;
        else
          Eps1 = 0;

        double Eps2;
        if (I == Entities.Count - 1)
          Eps2 = 1E-4;
        else if (Entities[I + 1].StartStation > Entities[I].EndStation)
          Eps2 = 1E-4;
        else
          Eps2 = 0;

        if (Range.InRange(Stn, Entities[I].StartStation - Eps1, Entities[I].EndStation + Eps2))
        {
          Element = Entities[I];
        }
      }
    }

    // NumberElements assigns the ordinal index of each entity in the list to
    // the ElementIndex member in each entity
    // procedure NumberElements;

    //  Function InMemorySize : Longint; Override;
  }
}

