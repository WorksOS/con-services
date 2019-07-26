using System.Collections.Generic;

namespace VSS.TRex.Designs.SVL
{
  public class NFFGuidableAlignmentEntityList : List<NFFGuidableAlignmentEntity>
  {
    public NFFGuidableAlignmentEntity Locate(int AnID)
    {
      for (int I = 0; I < Count; I++)
        if (this[I].GuidanceID == AnID)
          return this[I];

      return null;
    }

    //   Procedure SaveToStream(Stream : TStream);

/*
 public void LoadFromStream(BinaryReader reader)
    {
      var SchemaID = reader.ReadInt32(); //Schema ID for guidance alignment list (currently = 1)
      Debug.Assert(SchemaID == 1, $"Unknown SchemaID: {SchemaID}");

      var ItemCount = reader.ReadInt32();

      for (int I = 0; I < ItemCount; I++)
      {
        Add(new NFFGuidableAlignmentEntity());
        this.Last().LoadFromStream(reader);
      }
    }
*/

    //  procedure DumpToText(Stream: TTextDumpStream; const OriginX, OriginY: Integer);
    //   Function InMemorySize : Longint;
  }
}
