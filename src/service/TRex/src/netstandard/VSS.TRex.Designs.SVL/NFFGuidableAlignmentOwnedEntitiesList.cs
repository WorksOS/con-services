using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VSS.TRex.Common;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.SVL
{
  public class NFFGuidableAlignmentOwnedEntitiesList : List<NFFStationedLineworkEntity>
  {
    public NFFGuidableAlignmentOwnedEntitiesList()
    {
    }

    public NFFGuidableAlignmentOwnedEntitiesList(NFFGuidableAlignmentEntity owner) : this()
    {
      Owner = owner;
    }

    public NFFGuidableAlignmentEntity Owner;

    //    procedure DumpToText(Stream: TTextDumpStream;    const OriginX, OriginY : Double);
    //    Procedure SaveToNFFStream(Stream : TStream;     const OriginX, OriginY : Double;    FileVersion : NFFFileVersion);

    public void LoadFromNFFStream(BinaryReader reader,
      double OriginX, double OriginY,
      bool HasGuidanceID,
      NFFFileVersion FileVersion)
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
