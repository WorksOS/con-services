using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VSS.TRex.Designs.SVL
{
  public class TNFFStreamInfoList : List<TNFFStreamInfo>
  {
//    protected
//      Function Get(Index : Integer) : TNFFStreamInfo;
//    procedure Put(Index: Integer; const Value: TNFFStreamInfo);

    //   Procedure SaveToStream(NFFFileVersion : TNFFFileVersion; Stream : TStream);
    public void LoadFromStream(TNFFFileVersion NFFFileVersion, BinaryReader reader)
    {
      //todo
    }

    //  procedure Sort;
    //  public
    //     property Items[Index : Integer] : TNFFStreamInfo read Get write Put; default;

    //   Function First : TNFFStreamInfo;
    //   Function Last : TNFFStreamInfo;

    public TNFFStreamInfo Locate(string Filename)
    {
      return this.FirstOrDefault(x => string.Compare(x.Name, Filename, StringComparison.OrdinalIgnoreCase) == 0);
    }
  }
}
