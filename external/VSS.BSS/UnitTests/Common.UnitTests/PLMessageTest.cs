using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;


using VSS.Hosted.VLCommon.PLMessages;
using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests
{
  /// <summary>
  /// Summary description for PLMessageTest
  /// </summary>
  [TestClass]
  public class PLMessageTest : UnitTestBase
  {
    public PLMessageTest()
    {

    }

    private TestContext testContextInstance;

    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext
    {
      get
      {
        return testContextInstance;
      }
      set
      {
        testContextInstance = value;
      }
    }

    #region Additional test attributes
    //
    // You can use the following additional attributes as you write your tests:
    //
    // Use ClassInitialize to run code before running the first test in the class
    // [ClassInitialize()]
    // public static void MyClassInitialize(TestContext testContext) { }
    //
    // Use ClassCleanup to run code after all tests in a class have run
    // [ClassCleanup()]
    // public static void MyClassCleanup() { }
    //
    // Use TestInitialize to run code before running each test 
    // [TestInitialize()]
    // public void MyTestInitialize() { }
    //
    // Use TestCleanup to run code after each test has run
    // [TestCleanup()]
    // public void MyTestCleanup() { }
    //
    #endregion

    [TestMethod]
    public void PlMessageBase_VerifyMessageTypeIsFenceConfig()
    {
      // create fence config message - get body as string to be stored
      // then use the storage string to determine which type it is - should be a fence config
      byte[] formattedFenceConfigMessage = PLOutboundFormatter.FormatFenceConfig(
        true,
        true,
        true,
        1,
        1,
        1,
        1,
        DateTime.Now,
        DateTime.UtcNow.AddHours(1),
        1,
        new List<decimal>{1, 2, 3, 4}, 
        new List<decimal>{1, 2, 3, 4},
        new List<decimal>{1, 2, 3, 4},
        1,
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        0,
        24);

      string fenceConfigMessageBody = PLMessageBase.BytesToBinaryString(formattedFenceConfigMessage);

      var transactiontYpe = PLMessageBase.GetMessageType(fenceConfigMessageBody, false);

      Assert.IsTrue(transactiontYpe.Equals(PLTransactionTypeEnum.FenceConfig));
    }

    [TestMethod]
    public void PlMessageBase_VerifyMessageTypeIsNotFenceConfig()
    {
      Device device = Entity.Device.PL321.Save();
      DevicePersonality dp = Entity.DevicePersonality.PL321SRModuleType.ForDevice(device).Save();
      // create a non-fence config message - get body as string to be stored
      // then use the storage string to determine which type it is - should not be a fence config

      byte[] formattedMessage = PLOutboundFormatter.FormatRuntimeAdjustment(new TimeSpan(1, 1, 1, 1), device.GpsDeviceID, DeviceTypeEnum.PL321);

      string messageBody = PLMessageBase.BytesToBinaryString(formattedMessage);

      var transactionType = PLMessageBase.GetMessageType(messageBody, false);
      
      Assert.IsFalse(transactionType.Equals(PLTransactionTypeEnum.Unknown), "The message was found to be unknown but should be an OTA Message.");
      Assert.IsFalse(transactionType.Equals(PLTransactionTypeEnum.FenceConfig), "The message was found to be a fence config message but should not be.");
      Assert.IsTrue(transactionType.Equals(PLTransactionTypeEnum.OTAConfigMessages), "The message was not an OTA config message.");
    }

    [TestMethod]
    public void PlMessageBase_VerifyMessageTypeIsUnknown()
    {
      string notAValidMessageString = "Hello world, I am not a valid fence config formatted message string.";

      var transactionType = PLMessageBase.GetMessageType(notAValidMessageString, false);

      Assert.IsTrue(transactionType.Equals(PLTransactionTypeEnum.Unknown));
    }
  }
}
