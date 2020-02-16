using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;

namespace UnitTests.BSS_Tests
{
  public class BssPLOTAServiceFake : IBssPLOTAService
  {
    public bool WasExecuted = false;
    private bool _boolValue;

    public BssPLOTAServiceFake(bool boolValue)
    {
      _boolValue = boolValue;
    }
    public bool SendPLOTACommand(INH_OP opCtx1, string gpsDeviceID)
    {
      WasExecuted = true;
      return _boolValue;
    }
  }
}
