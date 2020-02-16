using System;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;

namespace UnitTests.BSS_Tests
{
  public class BssPLOTAServiceExceptionFake : IBssPLOTAService
  {
    public bool WasExecuted = false;

    public bool SendPLOTACommand(INH_OP opCtx1, string gpsDeviceID)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }
  }
}
