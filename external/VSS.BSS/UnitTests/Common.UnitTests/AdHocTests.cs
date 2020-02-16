using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.MTSMessages;

namespace UnitTests
{
  [TestClass]
  public class AdHocTests
  {



    [TestMethod()]
    public void ToTraceString()
    {
      var sql = string.Empty;//((ObjectQuery)()).ToTraceString();

      System.Diagnostics.Debug.WriteLine(sql);  // Visible in the Output window so long as you run the test as Debug.
    }

    [TestMethod()]
    public void MTSMessageSerialize()
    {
      // SELECT TOP 1 Payload FROM [NH_RAW].[dbo].[PR3Message] where PacketID = 7
      string payload = "02290036d0de40302000006402cc01000003cdcd6464210000430020000000610022630300331900703ec82077";
      byte[] bytes = HexDump.HexStringToBytes(payload);  // Omit the leading '0x'.

      bool isIncoming = true; // true if it's an incoming msg, false if it's an outgoing one.
      PlatformMessage msg = PlatformMessage.HydratePlatformMessage(bytes, true, isIncoming);
    }

    [TestMethod]
    public void METHODNAME()
    {
      TestEnum en1 = TestEnum.Val1;
      TestEnum en2 = (TestEnum)3;

      Debug.WriteLine(en1.ToString());
      Debug.WriteLine(en2.ToString());
    }
  }

  public enum TestEnum
  {
    Val1
  }
}



