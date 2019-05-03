using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.Map3D.Common
{
  public struct TileOptions
  {
  public Guid ProjectUid { get; set; }
  public Guid FilterUid { get; set; }
  public TileOptions(Guid projectUid, Guid filterUid)
  {
    ProjectUid = projectUid;
    FilterUid = filterUid;
  }
}
}
