using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace VSS.TRex.Designs.SVL
{
  public class TNFFGuidableAlignmentEntityList : List<TNFFGuidableAlignmentEntity>
  {
    //  protected
//      FOwner : TObject; // Actually a TNFFFile, but have usual circular reference problem

    //  function GetExpectedObjectType: TClass; override;
    //  procedure DoItemAdded(AObject: TObject); override;

    ///  Function Get(Index : Integer) : TNFFGuidableAlignmentEntity;
    //  procedure Put(Index: Integer; const Value: TNFFGuidableAlignmentEntity);
    //  public
    //  constructor Create(AOwner: TObject); // Hides inherited no-args constructor

//    property Items[Index : Integer] : TNFFGuidableAlignmentEntity read Get write Put; default;

    //  Function First : TNFFGuidableAlignmentEntity;
//    Function Last : TNFFGuidableAlignmentEntity;

    public TNFFGuidableAlignmentEntity Locate(int AnID)
    {
      for (int I = 0; I < Count - 1; I++)
        if (this[I].GuidanceID == AnID)
          return this[I];

      return null;
    }


    //   Procedure SaveToStream(Stream : TStream);
    public void LoadFromStream(BinaryReader reader)
    {
      var SchemaID = reader.ReadInt32(); //Schema ID for guidance alignment list (currently = 1)
      Debug.Assert(SchemaID == 1, $"Unknown SchemaID: {SchemaID}");

      var ItemCount = reader.ReadInt32();

      for (int I = 0; I < ItemCount; I++)
      {
        Add(new TNFFGuidableAlignmentEntity());
        this.Last().LoadFromStream(reader);
      }
    }


    //  procedure DumpToText(Stream: TTextDumpStream; const OriginX, OriginY: Integer);

    //   Function InMemorySize : Longint;
  }
}
