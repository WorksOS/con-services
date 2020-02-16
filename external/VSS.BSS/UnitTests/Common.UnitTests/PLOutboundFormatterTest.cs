using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.MTSMessages;
using VSS.Hosted.VLCommon.PLMessages;
using VSS.Hosted.VLCommon;

namespace UnitTests
{
  /// <summary>
  ///This is a test class for PLOutboundFormatterTest and is intended
  ///to contain all PLOutboundFormatterTest Unit Tests
  ///</summary>
  [TestClass()]
  public class PLOutboundFormatterTest
  {
    /// <summary>
    ///A test for FormatFenceConfig
    ///</summary>
    [TestMethod()]
    public void FormatFenceConfigTest()
    {
      bool inclusiveProductWatch = true;
      bool exclusiveProductWatch = true;
      bool timeBasedProductWatch = true;
      byte inclusiveMessageID = 1;
      decimal inclusiveLatitude = 44M + ((30M + (20M / 60M)) / 60M);
      decimal inclusiveLongitude = -(73M + ((11M + (3M / 60M)) / 60M));
      decimal radius = 1;
      byte exclusiveMessageID = 1;
      List<decimal> exclusiveLat = new List<decimal>();
      exclusiveLat.Add(44M + ((30M + (21M / 60M)) / 60M));
      exclusiveLat.Add(44M + ((30M + (57M / 60M)) / 60M));
      exclusiveLat.Add(44M + ((30M + (43M / 60M)) / 60M));
      exclusiveLat.Add(44M + ((30M + (7M / 60M)) / 60M));
      exclusiveLat.Add(44M + ((29M + (26M / 60M)) / 60M));

      List<decimal> exclusiveLon = new List<decimal>();
      exclusiveLon.Add(-(73M + ((11M + (46M / 60M)) / 60M)));
      exclusiveLon.Add(-(73M + ((10M + (58M / 60M)) / 60M)));
      exclusiveLon.Add(-(73M + ((9M + (43M / 60M)) / 60M)));
      exclusiveLon.Add(-(73M + ((9M + (55M / 60M)) / 60M)));
      exclusiveLon.Add(-(73M + ((11M + (24M / 60M)) / 60M)));

      List<decimal> exRadius = new List<decimal> { 1, 1, 1, 1, 1 };

      bool workSun = false;
      bool workMon = true;
      bool workTue = true;
      bool workWed = true;
      bool workThur = true;
      bool workFri = true;
      bool workSat = false;

      byte startTime = 9;
      byte endTime = 18;

      string expected = "02070140B405CBF53A000100000000FFFFFFFF010540B3EBCBF30D000140B047CBF57B000140B1B2CBF945000140B556CBF8AA000140B97BCBF42A0001017C0912";
      string actual = HexDump.BytesToHexString(PLOutboundFormatter.FormatFenceConfig(inclusiveProductWatch, exclusiveProductWatch, timeBasedProductWatch, inclusiveMessageID,
        inclusiveLatitude, inclusiveLongitude, radius, null, null, exclusiveMessageID, exclusiveLat, exclusiveLon, exRadius, 1, workSun, workMon, workTue, workWed, workThur, workFri, workSat, startTime, endTime));
      Assert.AreEqual(expected, actual, "The Strings are not equal");

    }

    /// <summary>
    ///A test for FormatProductWatchActivation
    ///</summary>
    [TestMethod()]
    public void FormatProductWatchActivationTest()
    {
      bool? inclusiveWatchActive = true;
      bool? exclusiveWatchActive = true;
      bool? timeBasedWatchActive = true;
      
      byte[] actual = PLOutboundFormatter.FormatProductWatchActivation(inclusiveWatchActive, exclusiveWatchActive, timeBasedWatchActive);
      ProductWatchActivation product = new ProductWatchActivation();
      uint bitPosition = 0;
      product.Serialize(SerializationAction.Hydrate, actual, ref bitPosition);
      Assert.IsNotNull(product, "Actual should be a ProductWatchActivation message");
      Assert.AreEqual(inclusiveWatchActive, product.inclusiveWatchActivate, "inclusiveWatchActivate should be true");
      Assert.AreEqual(exclusiveWatchActive, product.exclusiveWatchActivate, "exclusiveWatchActivate should be true");
      Assert.AreEqual(timeBasedWatchActive, product.timeBasedWatchActivate, "timeBasedWatchActivate should be true");
    }
  }
}
