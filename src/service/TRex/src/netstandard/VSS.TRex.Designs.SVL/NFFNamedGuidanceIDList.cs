using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.Common;
using VSS.TRex.Designs.SVL.Comparers;
using VSS.TRex.Designs.SVL.Utilities;

namespace VSS.TRex.Designs.SVL
{
  public class NFFNamedGuidanceIDList : List<NFFNamedGuidanceID>
  {
// TODO Might need -->    procedure DoItemAdded(AObject: TObject); override;

//    Procedure SaveToStream(Stream : TStream);
    public void LoadFromStream(BinaryReader reader)
    {
      int Num = reader.ReadUInt16();
      for (int I = 0; I < Num; I++)
      {
        var item = new NFFNamedGuidanceID();

        //this.Add(new NFFNamedGuidanceID);
        item.ID = reader.ReadInt16();
        item.Flags = reader.ReadByte();
        item.Name = NFFUtils.ReadWideStringFromStream(reader);

        if ((item.Flags & NFFConsts.kNFFGuidanceIDHasStationRange) != 0)
        {
          item.StartStation = reader.ReadDouble();
          item.EndStation = reader.ReadDouble();
        }

        Add(item);
      }
    }

    //   procedure DumpToText(Stream: TTextDumpStream);

    // Used by owner NFFFile.ProcessGuidanceAlignments only
    public void SortByOffset()
    {
      // NFFNamedGuidanceID.StartOffset values are maintained solely to perform initial
      // sort and are NOT streamed to file, thus when this function is called in context
      // of file load all StartOffset values will be initialised to NullReal.  In this
      // case the sort should be skipped
      if (Count < 2 || this[0].StartOffset == Consts.NullDouble)
        return;

      Sort(new NamedGuidanceIDComparer());
    }

    public double MinStartStation()
    {
      double Result = 1.0e308; // Very big positive

      for (int I = 0; I < Count; I++)
        Result = Math.Min(this[I].StartStation, Result);

      return Result;
    }

    public double MaxEndStation()
    {
      double Result = 1.0e308; // Very big positive

      for (int I = 0; I < Count; I++)
        Result = Math.Max(this[I].EndStation, Result);

      return Result;
    }

    public NFFNamedGuidanceID Locate(string AName)
    {
      for (int I = 0; I < Count; I++)
        if (string.Compare(this[I].Name, AName, StringComparison.OrdinalIgnoreCase) == 0)
        {
          return this[I];
        }

      return null;
    }

    public NFFNamedGuidanceID Locate(int AnID)
    {
      for (int I = 0; I < Count; I++)
        if (this[I].ID == AnID)
        {
          return this[I];
        }

      return null;
    }

    // Procedure SortName; // Sort the guidance ID list based on name
    // Procedure SortIDs; // Sort the guidance ID list based on ID
  }
}
