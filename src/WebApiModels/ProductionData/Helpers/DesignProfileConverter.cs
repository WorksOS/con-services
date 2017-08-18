using SVOICProfileCell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VLPDDecls;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Common;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using VSS.Velociraptor.PDSInterface;
using ProfileCell = VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling.ProfileCell;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Helpers
{
  public class DesignProfileConverter : ProfileConverterBase
  {
    public static CompactionProfileResult ConvertDesignProfileResult(MemoryStream ms, Guid callId)
    {
      throw new NotImplementedException();
    }
  }
}