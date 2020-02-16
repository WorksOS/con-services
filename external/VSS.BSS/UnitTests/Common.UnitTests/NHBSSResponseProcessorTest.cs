using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Nighthawk.NHBssSvc;
using VSS.UnitTest.Common;
using VSS.UnitTest.Common.Accessor;
using VSS.Hosted.VLCommon;

namespace UnitTests
{
    
    
    /// <summary>
    ///This is a test class for NHBSSResponseProcessorTest and is intended
    ///to contain all NHBSSResponseProcessorTest Unit Tests
    ///</summary>
  [TestClass()]
  public class NHBSSResponseProcessorTest : UnitTestBase
  {
    /// <summary>
    ///A test for UpdateBssResponseMsg
    ///</summary>
    [TestMethod()]
    //[DatabaseTest]
    public void UpdateBssResponseMsgSuccessTest()
    {
      dynamic tester = ExposedClass.From(typeof(NHBSSResponseProcessor));
      long provID = CreateBSSProvisioningMsg(Ctx.OpContext, "AccountHierarchy", "Test", 0, BSSStatusEnum.Complete, 47, DateTime.UtcNow, "<Test><Element1>2</Element1></Test>", DateTime.UtcNow, "TEST", 55, DateTime.UtcNow.AddMinutes(-2));
      BSSResponseMsg response = CreateBSSResponseMsg(Ctx.OpContext, 47, provID, "TESTMachineName", BSSStatusEnum.Complete, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(-2), 0);

      tester.UpdateBssResponseMsg(Ctx.OpContext, response, false);

      BSSResponseMsg actual = (from b in Ctx.OpContext.BSSResponseMsgReadOnly
                               where b.fk_BSSProvisioningMsgID == provID
                               select b).SingleOrDefault();
      Assert.IsNotNull(actual, "Should return a BSSResponse");
      Assert.AreEqual((int)BSSStatusEnum.Complete, actual.fk_BSSStatusID, "Incorrect StatusID");
      Assert.IsNotNull(actual.ResponseSentUTC, "SentUTC should have a value");
      Assert.AreEqual(0, actual.FailedCount, "Failed count is incorrect");
    }

    /// <summary>
    ///A test for UpdateBssResponseMsg
    ///</summary>
    [TestMethod()]
    //[DatabaseTest]
    public void UpdateBssResponseMsgRetryPendingTest()
    {
      dynamic tester = ExposedClass.From(typeof(NHBSSResponseProcessor));
      long provID = CreateBSSProvisioningMsg(Ctx.OpContext, "AccountHierarchy", "Test", 0, BSSStatusEnum.Complete, 47, DateTime.UtcNow, "<Test><Element1>2</Element1></Test>", DateTime.UtcNow, "TEST", 55, DateTime.UtcNow.AddMinutes(-2));
      BSSResponseMsg response = CreateBSSResponseMsg(Ctx.OpContext, 47, provID, "TESTMachineName", BSSStatusEnum.Complete, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(-2), 0);

      tester.UpdateBssResponseMsg(Ctx.OpContext, response, true);

      BSSResponseMsg actual = (from b in Ctx.OpContext.BSSResponseMsgReadOnly
                               where b.fk_BSSProvisioningMsgID == provID
                               select b).SingleOrDefault();
      Assert.IsNotNull(actual, "Should return a BSSResponse");
      Assert.AreEqual((int)BSSStatusEnum.RetryPending, actual.fk_BSSStatusID, "Incorrect StatusID");
      Assert.IsNotNull(actual.ResponseSentUTC, "SentUTC should have a value");
      Assert.AreEqual(1, actual.FailedCount, "Failed count is incorrect");
    }

    /// <summary>
    ///A test for UpdateBssResponseMsg
    ///</summary>
    [TestMethod()]
    //[DatabaseTest]
    public void UpdateBssResponseMsgFailedTest()
    {
      dynamic tester = ExposedClass.From(typeof(NHBSSResponseProcessor));
      long provID = CreateBSSProvisioningMsg(Ctx.OpContext, "AccountHierarchy", "Test", 0, BSSStatusEnum.Complete, 47, DateTime.UtcNow, "<Test><Element1>2</Element1></Test>", DateTime.UtcNow, "TEST", 55, DateTime.UtcNow.AddMinutes(-2));
      BSSResponseMsg response = CreateBSSResponseMsg(Ctx.OpContext, 47, provID, "TESTMachineName", BSSStatusEnum.Complete, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(-2), 4);

      tester.UpdateBssResponseMsg(Ctx.OpContext, response, true);

      BSSResponseMsg actual = (from b in Ctx.OpContext.BSSResponseMsgReadOnly
                               where b.fk_BSSProvisioningMsgID == provID
                               select b).SingleOrDefault();
      Assert.IsNotNull(actual, "Should return a BSSResponse");
      Assert.AreEqual((int)BSSStatusEnum.Failed, actual.fk_BSSStatusID, "Incorrect StatusID");
      Assert.IsNotNull(actual.ResponseSentUTC, "SentUTC should have a value");
      Assert.AreEqual(5, actual.FailedCount, "Failed count is incorrect");
    }

    private long CreateBSSProvisioningMsg(INH_OP ctx, string messageType, string machineName, byte failedCount, BSSStatusEnum status, long id, DateTime insertUTC, string messageXML, DateTime? processedUTC,
     string senderIP, long sequenceNumber, DateTime updateUTC)
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
      ctx.BSSProvisioningMsg.AddObject(msg);
      ctx.SaveChanges();

      return msg.SequenceNumber;
    }

    private BSSResponseMsg CreateBSSResponseMsg(INH_OP ctx, long ID, long bssSequenceNumber, string machineName, BSSStatusEnum status, DateTime? sentUTC, DateTime updateUTC, byte failedCount)
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
      msg.fk_BSSProvisioningMsgID = bssSequenceNumber;
      msg.FailedCount = failedCount;
      ctx.BSSResponseMsg.AddObject(msg);
      ctx.SaveChanges();
      return msg;
    }

  }
}
