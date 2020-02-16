using System;
using System.Collections.Generic;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon
{
  public interface IFirmwareAPI
  {
    long Create(INH_OP ctx, long serviceProviderID, string versionName, string sourceFolder, string notes);
    //unused
		//bool Update(INH_OP ctx, long versionID, List<Param> modifiedProperties);
  }
}
