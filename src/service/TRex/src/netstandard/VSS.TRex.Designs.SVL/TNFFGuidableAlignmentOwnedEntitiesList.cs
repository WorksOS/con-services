using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VSS.TRex.Common;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.SVL
{
  public class TNFFGuidableAlignmentOwnedEntitiesList : List<TNFFStationedLineworkEntity>
  {
    public TNFFGuidableAlignmentOwnedEntitiesList()
    {
    }

    public TNFFGuidableAlignmentOwnedEntitiesList(TNFFGuidableAlignmentEntity owner) : this()
    {
      Owner = owner;
    }

    public TNFFGuidableAlignmentEntity Owner;

    //    function GetExpectedObjectType: TClass; override;
    //    procedure DoItemAdded(AObject: TObject); override;

    //    Function Get(Index : Integer) : TNFFStationedLineWorkEntity;
    //    procedure Put(Index: Integer; const AEntity: TNFFStationedLineWorkEntity);
    //    public
    //      type
    //      TNFFGuidableAlignmentEntityListEnum = class
    //    private
    //      FCurrentIndex: Integer;
    //    FList: TNFFGuidableAlignmentOwnedEntitiesList;
    //    function GetCurrent: TNFFStationedLineWorkEntity;
    //    public
    //      constructor Create(AList: TNFFGuidableAlignmentOwnedEntitiesList);
    //    function MoveNext: Boolean;
    //    property Current: TNFFStationedLineWorkEntity read GetCurrent;
    //    end;

    //    property Items[Index : Integer] : TNFFStationedLineWorkEntity read Get write Put; default;

    //    function Extract(AEntity: TNFFStationedLineWorkEntity): TNFFStationedLineWorkEntity;
    //    Function First : TNFFStationedLineWorkEntity;
    //    Function Last : TNFFStationedLineWorkEntity;

    //    function GetEnumerator: TNFFGuidableAlignmentEntityListEnum;

    //    Function InMemorySize : Longint;

    //    procedure DumpToText(Stream: TTextDumpStream;
    //    const OriginX, OriginY : Double);
    //    Procedure SaveToNFFStream(Stream : TStream;
    //    const OriginX, OriginY : Double;
    //    FileVersion : TNFFFileVersion);

    public void LoadFromNFFStream(BinaryReader reader,
      double OriginX, double OriginY,
      bool HasGuidanceID,
      TNFFFileVersion FileVersion)
    {
      throw new NotImplementedException();
    }

//    Procedure SaveToStream(Stream : TStream);
//    Procedure LoadFromStream(Stream : TStream);

    public BoundingWorldExtent3D BoundingBox()
    {
      if (Count == 0)
        return new BoundingWorldExtent3D(Consts.NullDouble, Consts.NullDouble, Consts.NullDouble, Consts.NullDouble);

      var Result = this.First().BoundingBox();

      for (int I = 1; I < Count; I++)
        Result.Include(this[I].BoundingBox());

      return Result;
    }
  }
}
