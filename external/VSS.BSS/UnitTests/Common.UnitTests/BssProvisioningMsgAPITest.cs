using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;


using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests
{
  /// <summary>
  ///This is a test class for BssProvisioningMsgAPITest and is intended
  ///to contain all BssProvisioningMsgAPITest Unit Tests
  ///</summary>
  [TestClass()]
  public class BssProvisioningMsgAPITest : UnitTestBase
  {
    Dictionary<int, double> retryDelays;
    int maxFailures = 5;
    TimeSpan stuckMsgTimeout = TimeSpan.FromMinutes(5.0);

    [TestInitialize]
    public void TestInitialize()
    {
      retryDelays = (from delay in ConfigurationManager.AppSettings["BSSMessageRetryDelay"].Split('|')
                     let keyvalue = delay.Split(',')
                     select new
                     {
                       key = Convert.ToInt32(keyvalue[0]),
                       value = Convert.ToDouble(keyvalue[1])
                     }).ToDictionary(t => t.key, t => t.value);
    }

    /// <summary>
    ///A test for Create
    ///</summary>
    [DatabaseTest]
    [TestMethod]
    public void CreateTest()
    {
      string messageType;
      long sequenceNumber;
      string senderIP;
      CreateBssProvisioningMsg(Ctx.OpContext, out messageType, out sequenceNumber, out senderIP);
      //make sure the row was saved to the table
      List<BSSProvisioningMsg> msg = (from b in Ctx.OpContext.BSSProvisioningMsgReadOnly
                                      where b.BSSMessageType == messageType && b.SenderIP == senderIP
                                      select b).ToList();
      Assert.AreEqual(1, msg.Count, "Incorrect number of messages");
      Assert.AreEqual(sequenceNumber, msg[0].SequenceNumber, "incorrect sequence number");

    }

    /// <summary>
    ///A test for Update
    ///</summary>
    [TestMethod]
    [DatabaseTest]
    public void UpdateTest()
    {
      string messageType;
      long sequenceNumber;
      string senderIP;
      BSSProvisioningMsg bssMessage = CreateBssProvisioningMsg(Ctx.OpContext, out messageType, out sequenceNumber, out senderIP);
      string actualMachineName = bssMessage.MachineName;
      BSSStatusEnum status = BSSStatusEnum.Complete;
      DateTime? processedUTC = DateTime.UtcNow;
      byte? failureCount = 1;
      string machineName = string.Empty;
      bool expected = true;
      bool actual;
      actual = API.BssProvisioningMsg.Update(Ctx.OpContext, bssMessage, status, processedUTC, failureCount, machineName);
      Assert.AreEqual(expected, actual);
      List<BSSProvisioningMsg> msg = (from b in Ctx.OpContext.BSSProvisioningMsgReadOnly
                                      where b.BSSMessageType == messageType && b.SenderIP == senderIP
                                      select b).ToList();
      Assert.AreEqual(1, msg.Count, "Incorrect number of Messages");
      Assert.AreEqual(processedUTC, msg[0].ProcessedUTC, "Incorrect ProcessedUTC");
      Assert.AreEqual(failureCount, msg[0].FailedCount, "Incorrect FailedCount");
      Assert.AreEqual(actualMachineName, msg[0].MachineName, "Incorrect MachineName");
    }

    /// <summary>
    ///A test for GetNextMessageToProcess
    ///</summary>
    [TestMethod]
    public void GetNextMessageToProcess_NoneToProcessTest()
    {
      DateTime currentTime = DateTime.UtcNow.AddMinutes(1);
      DateTime retryTime = DateTime.UtcNow.AddMinutes(3);
      DateTime stuckTimeout = DateTime.UtcNow.Subtract(stuckMsgTimeout);

      CreateBSSProvisioningMsg(DateTime.UtcNow, DateTime.UtcNow, IdGen.GetId(), DateTime.UtcNow);
      CreateBSSProvisioningMsg(DateTime.UtcNow, DateTime.UtcNow, IdGen.GetId(), DateTime.UtcNow);

      BSSProvisioningMsg actual = API.BssProvisioningMsg.GetNextMessageToProcess(Ctx.OpContext, currentTime, stuckTimeout, retryDelays, maxFailures);
      Assert.IsNull(actual, "There is nothing to process so should return null.");
    }

    /// <summary>
    ///A test for GetNextMessageToProcess
    ///</summary>
    [TestMethod]
    public void GetNextMessageToProcess_LowestSequenceNumberTest()
    {
      DateTime currentTime = DateTime.UtcNow.AddMinutes(1);
      DateTime stuckTimeout = DateTime.UtcNow.Subtract(stuckMsgTimeout);

      CreateBSSProvisioningMsg(null, DateTime.UtcNow, 1000, DateTime.UtcNow, status: BSSStatusEnum.Pending);
      CreateBSSProvisioningMsg(null, DateTime.UtcNow, 1001, DateTime.UtcNow, status: BSSStatusEnum.Pending);
      CreateBSSProvisioningMsg(null, DateTime.UtcNow, 1002, DateTime.UtcNow, status: BSSStatusEnum.Pending);
      CreateBSSProvisioningMsg(null, DateTime.UtcNow, 1003, DateTime.UtcNow, status: BSSStatusEnum.Pending);

      BSSProvisioningMsg actual = API.BssProvisioningMsg.GetNextMessageToProcess(Ctx.OpContext, currentTime, stuckTimeout, retryDelays, maxFailures);
      Assert.IsNotNull(actual, "A message is expected to process.");
      Assert.AreEqual(1000, actual.SequenceNumber, "SequenceNumber is incorrect.");
    }

    /// <summary>
    ///A test for GetNextMessageToProcess
    ///</summary>
    [TestMethod]
    public void GetNextMessageToProcess_OneCurrentlyBeingProcessedTest()
    {
      DateTime currentTime = DateTime.UtcNow.AddMinutes(1);
      DateTime stuckTimeout = DateTime.UtcNow.Subtract(stuckMsgTimeout);

      CreateBSSProvisioningMsg(DateTime.UtcNow, DateTime.UtcNow, 1000, DateTime.UtcNow, status: BSSStatusEnum.Complete);
      CreateBSSProvisioningMsg(DateTime.UtcNow, DateTime.UtcNow, 1001, DateTime.UtcNow, status: BSSStatusEnum.InProgress);
      CreateBSSProvisioningMsg(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow, 1002, DateTime.UtcNow, status: BSSStatusEnum.Pending);

      BSSProvisioningMsg actual = API.BssProvisioningMsg.GetNextMessageToProcess(Ctx.OpContext, currentTime, stuckTimeout, retryDelays, maxFailures);
      Assert.IsNull(actual, "There is already something being processed so should return null.");
    }

    [TestMethod]
    public void GetNextMessageToProcess_NeedToProcessTest()
    {
      DateTime currentTime = DateTime.UtcNow.AddSeconds(5);
      DateTime stuckTimeout = DateTime.UtcNow.Subtract(stuckMsgTimeout);

      var id = IdGen.GetId();
      CreateBSSProvisioningMsg(DateTime.UtcNow, DateTime.UtcNow, IdGen.GetId(), DateTime.UtcNow);
      CreateBSSProvisioningMsg(DateTime.UtcNow, DateTime.UtcNow, IdGen.GetId(), DateTime.UtcNow);
      CreateBSSProvisioningMsg(DateTime.UtcNow, DateTime.UtcNow, IdGen.GetId(), DateTime.UtcNow);
      CreateBSSProvisioningMsg(null, DateTime.UtcNow, id, DateTime.UtcNow, status: BSSStatusEnum.Pending);

      BSSProvisioningMsg actual = API.BssProvisioningMsg.GetNextMessageToProcess(Ctx.OpContext, currentTime,stuckTimeout, retryDelays, maxFailures);
      Assert.IsNotNull(actual, "A message is expected to process.");
      Assert.AreEqual(id, actual.SequenceNumber, "SequenceNumber is incorrect.");
    }

    [TestMethod]
    public void GetNextMessageToProcess_RetryPendingTest()
    {
      DateTime currentTime = DateTime.UtcNow.AddMinutes(1);
      DateTime stuckTimeout = DateTime.UtcNow.Subtract(stuckMsgTimeout);

      CreateBSSProvisioningMsg(processedUTC: DateTime.UtcNow, insertUTC: DateTime.UtcNow, sequenceNumber: 1000, updateUTC: DateTime.UtcNow); //completed
      CreateBSSProvisioningMsg(processedUTC: DateTime.UtcNow.AddSeconds(-20), insertUTC: DateTime.UtcNow, sequenceNumber: 1001, updateUTC: DateTime.UtcNow, status: BSSStatusEnum.RetryPending, failedCount: 1); //retry pending
      CreateBSSProvisioningMsg(processedUTC: null, insertUTC: DateTime.UtcNow, sequenceNumber: 1002, updateUTC: DateTime.UtcNow, status: BSSStatusEnum.Pending); //pending

      BSSProvisioningMsg actual = API.BssProvisioningMsg.GetNextMessageToProcess(Ctx.OpContext, currentTime, stuckTimeout, retryDelays, maxFailures);
      Assert.IsNotNull(actual, "A message is expected to process.");
      Assert.AreEqual(1001, actual.SequenceNumber, "SequenceNumber is incorrect.");
    }

    [TestMethod]
    public void GetNextMessageToProcess_RetryPending_WithinRetryLimitTest()
    {
      DateTime currentTime = DateTime.UtcNow.AddMinutes(1);
      DateTime stuckTimeout = DateTime.UtcNow.Subtract(stuckMsgTimeout);

      CreateBSSProvisioningMsg(processedUTC: DateTime.UtcNow, insertUTC: DateTime.UtcNow, sequenceNumber: 1000, updateUTC: DateTime.UtcNow); //completed
      CreateBSSProvisioningMsg(processedUTC: DateTime.UtcNow, insertUTC: DateTime.UtcNow, sequenceNumber: 1001, updateUTC: DateTime.UtcNow, status: BSSStatusEnum.RetryPending, failedCount: 2); //retry pending      

      BSSProvisioningMsg actual = API.BssProvisioningMsg.GetNextMessageToProcess(Ctx.OpContext, currentTime, stuckTimeout, retryDelays, maxFailures);
      Assert.IsNull(actual, "No message should be returned.");      
    }

    [TestMethod]
    public void GetNextMessageToProcess_RetryPending_TakePending_Test()
    {
      DateTime currentTime = DateTime.UtcNow.AddMinutes(1);
      DateTime stuckTimeout = DateTime.UtcNow.Subtract(stuckMsgTimeout);

      var id = IdGen.GetId();
      CreateBSSProvisioningMsg(DateTime.UtcNow, DateTime.UtcNow, 1000, DateTime.UtcNow); //completed
      CreateBSSProvisioningMsg(null, DateTime.UtcNow, 1001, DateTime.UtcNow.AddMinutes(-1), status: BSSStatusEnum.Pending); //pending
      CreateBSSProvisioningMsg(DateTime.UtcNow, DateTime.UtcNow, 1002, DateTime.UtcNow, status: BSSStatusEnum.RetryPending, failedCount: 1); //retry pending      

      BSSProvisioningMsg actual = API.BssProvisioningMsg.GetNextMessageToProcess(Ctx.OpContext, currentTime, stuckTimeout, retryDelays, maxFailures);
      Assert.IsNotNull(actual, "A message is expected to process.");
      Assert.AreEqual(1001, actual.SequenceNumber, "SequenceNumber is incorrect.");
    }

    [TestMethod]
    public void GetNextMessageToProcess_AllFailedMessage_NoMessageToProcessTest()
    {
      DateTime currentTime = DateTime.UtcNow.AddMinutes(1);
      DateTime stuckTimeout = DateTime.UtcNow.Subtract(stuckMsgTimeout);

      CreateBSSProvisioningMsg(DateTime.UtcNow, DateTime.UtcNow, IdGen.GetId(), DateTime.UtcNow, status: BSSStatusEnum.Failed, failedCount: 5);
      CreateBSSProvisioningMsg(DateTime.UtcNow, DateTime.UtcNow, IdGen.GetId(), DateTime.UtcNow, status: BSSStatusEnum.Failed, failedCount: 5);
      CreateBSSProvisioningMsg(DateTime.UtcNow, DateTime.UtcNow, IdGen.GetId(), DateTime.UtcNow, status: BSSStatusEnum.Failed, failedCount: 5);

      BSSProvisioningMsg actual = API.BssProvisioningMsg.GetNextMessageToProcess(Ctx.OpContext, currentTime, stuckTimeout, retryDelays, maxFailures);
      Assert.IsNull(actual, "No message should be left in queue unprocessed and hence expecting a null.");
    }

    [TestMethod]
    public void GetNextMessageToProcessInProgressStuckTest()
    {
      DateTime currentTime = DateTime.UtcNow.AddMinutes(1);
      DateTime stuckTimeout = DateTime.UtcNow.Subtract(stuckMsgTimeout);

      CreateBSSProvisioningMsg(null, DateTime.UtcNow, 1000, DateTime.UtcNow.AddMinutes(-6), status: BSSStatusEnum.InProgress);
      CreateBSSProvisioningMsg(null, DateTime.UtcNow, 1001, DateTime.UtcNow, status: BSSStatusEnum.Pending);
      CreateBSSProvisioningMsg(null, DateTime.UtcNow, 1002, DateTime.UtcNow, status: BSSStatusEnum.Pending);

      BSSProvisioningMsg actual = API.BssProvisioningMsg.GetNextMessageToProcess(Ctx.OpContext, currentTime, stuckTimeout, retryDelays, maxFailures);
      Assert.IsNull(actual, "A message is not expected to process.");      
    }

    private void CreateBSSProvisioningMsg(DateTime? processedUTC, DateTime insertUTC,
      long sequenceNumber, DateTime updateUTC, string messageType = "AccountHierarchy",
      string machineName = "Test", byte failedCount = 0, BSSStatusEnum status = BSSStatusEnum.Complete,
      string messageXML = "<Test><Element1>2</Element1></Test>", string senderIP = "Test")
    {
      BSSProvisioningMsg msg = new BSSProvisioningMsg();
      msg.MachineName = machineName;
      msg.BSSMessageType = messageType;
      msg.FailedCount = failedCount;
      msg.fk_BSSStatusID = (int)status;
      msg.InsertUTC = insertUTC;
      msg.MessageXML = messageXML;
      msg.ProcessedUTC = processedUTC;
      msg.SenderIP = senderIP;
      msg.SequenceNumber = sequenceNumber;
      msg.UpdateUTC = updateUTC;
      Ctx.OpContext.BSSProvisioningMsg.AddObject(msg);
      Ctx.OpContext.SaveChanges();
    }

    private static BSSProvisioningMsg CreateBssProvisioningMsg(INH_OP ctx, out string messageType, out long sequenceNumber, out string senderIP)
    {
      messageType = "AccountHierarchy";
      sequenceNumber = 50;
      senderIP = "10.20.15.66";
      string xml = "<Test><Element1>2</Element1></Test>";
      BSSProvisioningMsg actual;
      actual = API.BssProvisioningMsg.Create(ctx, messageType, sequenceNumber, senderIP, xml);
      Assert.IsNotNull(actual, "Actual should not be null");
      return actual;
    }
  }
}
