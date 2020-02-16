using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace VSS.Hosted.VLCommon.Bss
{
  public interface IBssPLOTAService
  {
      bool SendPLOTACommand(INH_OP opCtx1, string gpsDeviceID);
  }
}
