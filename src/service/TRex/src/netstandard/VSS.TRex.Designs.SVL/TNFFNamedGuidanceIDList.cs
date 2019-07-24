using System;
using System.Collections.Generic;
using System.IO;

namespace VSS.TRex.Designs.SVL
{
  public class TNFFNamedGuidanceIDList : List<TNFFNamedGuidanceID>
  {
// TODO Might need -->    procedure DoItemAdded(AObject: TObject); override;

//    Procedure SaveToStream(Stream : TStream);
    public void LoadFromStream(BinaryReader reader)
    {
      int Num = reader.ReadUInt16();
      for (int I = 0; I < Num; I++)
      {
        var item = new TNFFNamedGuidanceID();

        //this.Add(new TNFFNamedGuidanceID);
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

    // Used by owner TNFFFile.ProcessGuidanceAlignments only
    // public void SortByOffset();


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

    public TNFFNamedGuidanceID Locate(string AName)
    {
      for (int I = 0; I < Count; I++)
        if (string.Compare(this[I].Name, AName, StringComparison.OrdinalIgnoreCase) == 0)
        {
          return this[I];
        }

      return null;
    }

    public TNFFNamedGuidanceID Locate(int AnID)
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
