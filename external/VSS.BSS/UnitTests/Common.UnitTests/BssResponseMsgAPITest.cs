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
    ///This is a test class for BSSResponseMsgAPITest and is intended
    ///to contain all BSSResponseMsgAPITest Unit Tests
    ///</summary>
  [TestClass()]
  public class BssResponseMsgAPITest : UnitTestBase
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
      long provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.InProgress, insertUTC: DateTime.UtcNow, 
            responseSentUtc: DateTime.UtcNow, updateUTC: DateTime.UtcNow.AddMinutes(10), sequenceNumber: 55);
      CreateBSSResponseMsgUsingAPI(Ctx.OpContext, provID, "TESTMachineName");
      //make sure the row was saved to the table
      List<BSSResponseMsg> msg = (from b in Ctx.OpContext.BSSResponseMsgReadOnly
                                where b.fk_BSSProvisioningMsgID == provID
                                select b).ToList();
      Assert.AreEqual(1, msg.Count, "Incorrect number of messages");
      Assert.AreEqual((int)BSSStatusEnum.Pending, msg[0].fk_BSSStatusID, "incorrect Status ID");
      Assert.AreEqual("TESTMachineName", msg[0].MachineName, "incorrect Machine Name");
      Assert.IsNull(msg[0].ResponseSentUTC, "Response Sent UTC should be null");
    }

    /// <summary>
    ///A test for Update
    ///</summary>
    [TestMethod]
    [DatabaseTest]
    public void UpdateTest()
    {
      long provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.InProgress, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, updateUTC: DateTime.UtcNow.AddMinutes(-10), sequenceNumber: 55);
      BSSResponseMsg bssMessage = CreateBSSResponseMsgUsingAPI(Ctx.OpContext, provID, "TESTMachineName");

      string actualMachineName = bssMessage.MachineName;
      BSSStatusEnum status = BSSStatusEnum.Complete;
      DateTime? sentUTC = DateTime.UtcNow;
      byte? failureCount = 1;
      string machineName = string.Empty;
      bool expected = true;
      bool actual;
      actual = API.BssResponseMsg.Update(Ctx.OpContext, bssMessage, status, sentUTC.Value, failureCount);
      Assert.AreEqual(expected, actual);
      List<BSSResponseMsg> msg = (from b in Ctx.OpContext.BSSResponseMsgReadOnly
                                  where b.ID == bssMessage.ID
                                  select b).ToList();
      Assert.AreEqual(1, msg.Count, "Incorrect number of Messages");
      Assert.AreEqual(sentUTC, msg[0].ResponseSentUTC, "Incorrect SentUTC");
      Assert.AreEqual(failureCount, msg[0].FailedCount, "Incorrect FailedCount");
      Assert.AreEqual(actualMachineName, msg[0].MachineName, "Incorrect MachineName");
    }

    /// <summary>
    ///A test for GetNextMessageToSend
    ///</summary>
    [TestMethod]
    //[DatabaseTest]
    public void GetNextMessageToSendNoneToSendTest()
    {
      DateTime currentTime = DateTime.UtcNow.AddMinutes(1);
      DateTime stuckTimeout = DateTime.UtcNow.Subtract(stuckMsgTimeout);

      long provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 55, updateUTC: DateTime.UtcNow);
      CreateBSSResponseMsg(Ctx.OpContext, 22, provID, "TESTMachineName", BSSStatusEnum.Complete, DateTime.UtcNow, DateTime.UtcNow, 0);

      provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 56, updateUTC: DateTime.UtcNow);
      CreateBSSResponseMsg(Ctx.OpContext, 23, provID, "TESTMachineName", BSSStatusEnum.Complete, DateTime.UtcNow, DateTime.UtcNow, 0);

      provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 57, updateUTC: DateTime.UtcNow);
      CreateBSSResponseMsg(Ctx.OpContext, 24, provID, "TESTMachineName", BSSStatusEnum.Complete, DateTime.UtcNow, DateTime.UtcNow, 0);

      BSSResponseMsg actual = API.BssResponseMsg.GetNextMessageToSend(Ctx.OpContext, currentTime, stuckTimeout, retryDelays, maxFailures);
      Assert.IsNull(actual, "There is nothing to process so should return null");
    }

    /// <summary>
    ///A test for GetNextMessageToProcess
    ///</summary>
    [TestMethod]
    //[DatabaseTest]
    public void GetNextMessageToSendOneCurrentlyBeingSendTest()
    {
      DateTime currentTime = DateTime.UtcNow.AddMinutes(1);
      DateTime stuckTimeout = DateTime.UtcNow.Subtract(stuckMsgTimeout);

      long provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 55, updateUTC: DateTime.UtcNow.AddMinutes(-2));
      CreateBSSResponseMsg(Ctx.OpContext, 47, provID, "TESTMachineName", BSSStatusEnum.Complete, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(-2), 0);

      provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow.AddMinutes(-1), responseSentUtc: null, sequenceNumber: 56, updateUTC: DateTime.UtcNow.AddMinutes(-1));
      CreateBSSResponseMsg(Ctx.OpContext, 48, provID, "TESTMachineName", BSSStatusEnum.InProgress, null, DateTime.UtcNow.AddSeconds(5), 0);

      provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow.AddMinutes(1), responseSentUtc: null, sequenceNumber: 57, updateUTC: DateTime.UtcNow);
      CreateBSSResponseMsg(Ctx.OpContext, 49, provID, "TESTMachineName", BSSStatusEnum.Pending, null, DateTime.UtcNow.AddSeconds(5), 0);

      BSSResponseMsg actual = API.BssResponseMsg.GetNextMessageToSend(Ctx.OpContext, currentTime, stuckTimeout, retryDelays, maxFailures);
      Assert.IsNull(actual, "There is already something being processed so should return null");
    }

    [TestMethod]
    public void GetNextMessageToSend_LowestSequenceNumberTest()
    {
      DateTime currentTime = DateTime.UtcNow.AddMinutes(1);
      DateTime stuckTimeout = DateTime.UtcNow.Subtract(stuckMsgTimeout);

      long provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 55, updateUTC: DateTime.UtcNow.AddMinutes(-3));
      CreateBSSResponseMsg(Ctx.OpContext, 49, provID, "TESTMachineName", BSSStatusEnum.Pending, null, DateTime.UtcNow, 0);
      provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 56, updateUTC: DateTime.UtcNow.AddMinutes(-2));
      CreateBSSResponseMsg(Ctx.OpContext, 50, provID, "TESTMachineName", BSSStatusEnum.Pending, null, DateTime.UtcNow, 0);
      provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 57, updateUTC: DateTime.UtcNow.AddMinutes(-1));
      CreateBSSResponseMsg(Ctx.OpContext, 51, provID, "TESTMachineName", BSSStatusEnum.Pending, null, DateTime.UtcNow, 0);
      provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 58, updateUTC: DateTime.UtcNow);
      CreateBSSResponseMsg(Ctx.OpContext, 52, provID, "TESTMachineName", BSSStatusEnum.Pending, null, DateTime.UtcNow, 0);

      BSSResponseMsg actual = API.BssResponseMsg.GetNextMessageToSend(Ctx.OpContext, currentTime, stuckTimeout, retryDelays, maxFailures);
      Assert.IsNotNull(actual, "A message is expected to sent.");
      Assert.AreEqual(49, actual.ID, "The IDs are expected to match.");
    }

    [TestMethod]
    public void GetNextMessageToSend_NeedToProcessTest()
    {
      DateTime currentTime = DateTime.UtcNow.AddMinutes(1);
      DateTime stuckTimeout = DateTime.UtcNow.Subtract(stuckMsgTimeout);

      long provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 55, updateUTC: DateTime.UtcNow.AddMinutes(-3));
      CreateBSSResponseMsg(Ctx.OpContext, 49, provID, "TESTMachineName", BSSStatusEnum.Complete, DateTime.UtcNow, DateTime.UtcNow, 0);
      provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 56, updateUTC: DateTime.UtcNow.AddMinutes(-2));
      CreateBSSResponseMsg(Ctx.OpContext, 50, provID, "TESTMachineName", BSSStatusEnum.Complete, DateTime.UtcNow, DateTime.UtcNow, 0);
      provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 57, updateUTC: DateTime.UtcNow.AddMinutes(-1));
      CreateBSSResponseMsg(Ctx.OpContext, 51, provID, "TESTMachineName", BSSStatusEnum.Complete, DateTime.UtcNow, DateTime.UtcNow, 0);
      provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 58, updateUTC: DateTime.UtcNow);
      CreateBSSResponseMsg(Ctx.OpContext, 52, provID, "TESTMachineName", BSSStatusEnum.Pending, null, DateTime.UtcNow, 0);

      BSSResponseMsg actual = API.BssResponseMsg.GetNextMessageToSend(Ctx.OpContext, currentTime, stuckTimeout, retryDelays, maxFailures);
      Assert.IsNotNull(actual, "A message is expected to send.");
      Assert.AreEqual(52, actual.ID, "The IDs are expected to match.");
    }

    [TestMethod]
    public void GetNextMessageToSend_RetryPendingTest()
    {
      DateTime currentTime = DateTime.UtcNow.AddMinutes(1);
      DateTime stuckTimeout = DateTime.UtcNow.Subtract(stuckMsgTimeout);

      long provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 55, updateUTC: DateTime.UtcNow.AddMinutes(-3));
      CreateBSSResponseMsg(Ctx.OpContext, 49, provID, "TESTMachineName", BSSStatusEnum.Complete, DateTime.UtcNow, DateTime.UtcNow, 0);
      
      provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 56, updateUTC: DateTime.UtcNow.AddMinutes(-2));
      CreateBSSResponseMsg(Ctx.OpContext, 50, provID, "TESTMachineName", BSSStatusEnum.RetryPending, DateTime.UtcNow.AddSeconds(-15), DateTime.UtcNow, 1);
      
      provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 57, updateUTC: DateTime.UtcNow.AddMinutes(-1));
      CreateBSSResponseMsg(Ctx.OpContext, 51, provID, "TESTMachineName", BSSStatusEnum.Pending, null, DateTime.UtcNow, 0);

      BSSResponseMsg actual = API.BssResponseMsg.GetNextMessageToSend(Ctx.OpContext, currentTime, stuckTimeout, retryDelays, maxFailures);
      Assert.IsNotNull(actual, "A message is expected to send.");
      Assert.AreEqual(50, actual.ID, "The IDs are expected to match.");
    }

    [TestMethod]
    public void GetNextMessageToSend_NoMessageToProcessTest()
    {
      DateTime currentTime = DateTime.UtcNow.AddMinutes(1);
      DateTime stuckTimeout = DateTime.UtcNow.Subtract(stuckMsgTimeout);

      long provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 55, updateUTC: DateTime.UtcNow.AddMinutes(-3));
      CreateBSSResponseMsg(Ctx.OpContext, 49, provID, "TESTMachineName", BSSStatusEnum.Complete, DateTime.UtcNow, DateTime.UtcNow, 0);

      provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 56, updateUTC: DateTime.UtcNow.AddMinutes(-2));
      CreateBSSResponseMsg(Ctx.OpContext, 50, provID, "TESTMachineName", BSSStatusEnum.Complete, DateTime.UtcNow, DateTime.UtcNow, 0);

      provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 57, updateUTC: DateTime.UtcNow.AddMinutes(-1));
      CreateBSSResponseMsg(Ctx.OpContext, 51, provID, "TESTMachineName", BSSStatusEnum.Complete, DateTime.UtcNow, DateTime.UtcNow, 0);

      BSSResponseMsg actual = API.BssResponseMsg.GetNextMessageToSend(Ctx.OpContext, currentTime, stuckTimeout, retryDelays, maxFailures);
      Assert.IsNull(actual, "There is nothing to process so should return null");
    }

    [TestMethod]
    public void GetNextMessageToSend_RetryPending_TakePending_Test()
    {
      DateTime currentTime = DateTime.UtcNow.AddMinutes(1);
      DateTime stuckTimeout = DateTime.UtcNow.Subtract(stuckMsgTimeout);

      long provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 55, updateUTC: DateTime.UtcNow.AddMinutes(-3));
      CreateBSSResponseMsg(Ctx.OpContext, 49, provID, "TESTMachineName", BSSStatusEnum.Complete, DateTime.UtcNow, DateTime.UtcNow, 0);
     
      provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 57, updateUTC: DateTime.UtcNow.AddMinutes(-1));
      CreateBSSResponseMsg(Ctx.OpContext, 50, provID, "TESTMachineName", BSSStatusEnum.Pending, null, DateTime.UtcNow, 0);

      provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 56, updateUTC: DateTime.UtcNow.AddMinutes(-2));
      CreateBSSResponseMsg(Ctx.OpContext, 51, provID, "TESTMachineName", BSSStatusEnum.RetryPending, DateTime.UtcNow.AddSeconds(1), DateTime.UtcNow, 1);

      BSSResponseMsg actual = API.BssResponseMsg.GetNextMessageToSend(Ctx.OpContext, currentTime, stuckTimeout, retryDelays, maxFailures);
      Assert.IsNotNull(actual, "A message is expected to send.");
      Assert.AreEqual(50, actual.ID, "The IDs are expected to match.");
    }

    [TestMethod]
    public void GetNextMessageToSend_InProgressStuckTest()
    {
      DateTime currentTime = DateTime.UtcNow.AddMinutes(1);
      DateTime stuckTimeout = DateTime.UtcNow.Subtract(stuckMsgTimeout);

      long provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 55, updateUTC: DateTime.UtcNow.AddMinutes(-3));
      CreateBSSResponseMsg(Ctx.OpContext, 49, provID, "TESTMachineName", BSSStatusEnum.Complete, DateTime.UtcNow, DateTime.UtcNow, 0);

      provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 56, updateUTC: DateTime.UtcNow.AddMinutes(-2));
      CreateBSSResponseMsg(Ctx.OpContext, 50, provID, "TESTMachineName", BSSStatusEnum.InProgress, null, DateTime.UtcNow.AddMinutes(-6), 1);

      provID = CreateBSSProvisioningMsg(status: BSSStatusEnum.Complete, insertUTC: DateTime.UtcNow, responseSentUtc: DateTime.UtcNow, sequenceNumber: 57, updateUTC: DateTime.UtcNow.AddMinutes(-1));
      CreateBSSResponseMsg(Ctx.OpContext, 51, provID, "TESTMachineName", BSSStatusEnum.Pending, null, DateTime.UtcNow, 0);

      BSSResponseMsg actual = API.BssResponseMsg.GetNextMessageToSend(Ctx.OpContext, currentTime, stuckTimeout, retryDelays, maxFailures);
      Assert.IsNull(actual, "No message is expected to be picked up.");      
    }

    private long CreateBSSProvisioningMsg(DateTime? responseSentUtc, DateTime updateUTC, DateTime insertUTC, long sequenceNumber, 
      string messageType = "AccountHierarchy", string machineName = "Test", byte failedCount = 0, 
      BSSStatusEnum status = BSSStatusEnum.Complete, string messageXML = "<Test><Element1>2</Element1></Test>", string senderIP = "TEST")
    {
      BSSProvisioningMsg msg = new BSSProvisioningMsg();
      msg.MachineName = machineName;
      msg.BSSMessageType = messageType;
      msg.FailedCount = failedCount;
      msg.fk_BSSStatusID = (int)status;
      msg.InsertUTC = insertUTC;
      msg.MessageXML = messageXML;
      msg.ProcessedUTC = responseSentUtc;
      msg.SenderIP = senderIP;
      msg.SequenceNumber = sequenceNumber;
      msg.UpdateUTC = updateUTC;
      Ctx.OpContext.BSSProvisioningMsg.AddObject(msg);
      Ctx.OpContext.SaveChanges();

      return msg.SequenceNumber;
    }

    private BSSResponseMsg CreateBSSResponseMsg(INH_OP ctx, long ID, long bssProvisioningMsgID, string machineName, BSSStatusEnum status, DateTime? sentUTC, DateTime updateUTC, byte failedCount)
    {
      BSSResponseMsg msg = new BSSResponseMsg();
      msg.ID = ID;
      msg.InsertUTC = DateTime.UtcNow;
      msg.MachineName = machineName;
      msg.ResponseSentUTC = sentUTC;
      msg.ResponseXML = "<Test><Element1>2</Element1></Test>";
      msg.fk_BSSResponseEndPointID = Helpers.NHOp.CreateResponseEndpoint();
      msg.UpdateUTC = updateUTC;
      msg.fk_BSSStatusID = (int)status;
      msg.fk_BSSProvisioningMsgID = bssProvisioningMsgID;
      msg.FailedCount = failedCount;
      ctx.BSSResponseMsg.AddObject(msg);
      ctx.SaveChanges();
      return msg;
    }

    private BSSResponseMsg CreateBSSResponseMsgUsingAPI(INH_OP ctx, long bssSequenceNumber, string machineName)
    {
      string responseXML = "<Test><Element1>2</Element1></Test>";
      BSSResponseMsg actual;

      actual = API.BssResponseMsg.Create(ctx, bssSequenceNumber, responseXML, Helpers.NHOp.CreateResponseEndpoint(), machineName);
      Assert.IsNotNull(actual, "Actual should not be null");
      return actual;
    }
  }
}

