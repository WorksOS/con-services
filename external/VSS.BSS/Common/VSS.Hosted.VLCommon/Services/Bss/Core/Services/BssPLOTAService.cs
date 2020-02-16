using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class BssPLOTAService : IBssPLOTAService
  {
      public bool SendPLOTACommand(INH_OP opCtx1, string gpsDeviceID)
    {
      return API.PLOutbound.SendQueryCommand(opCtx1, gpsDeviceID, PLQueryCommandEnum.Deregistration);
    }
  }
}
