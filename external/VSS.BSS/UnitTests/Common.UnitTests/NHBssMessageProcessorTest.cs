using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Nighthawk.NHBssSvc;
using VSS.UnitTest.Common;
using VSS.UnitTest.Common.Accessor;

namespace UnitTests
{
  /// <summary>
  ///This is a test class for NHBssMessageProcessorTest and is intended
  ///to contain all NHBssMessageProcessorTest Unit Tests
  ///</summary>
  [TestClass]
  public class NHBssMessageProcessorTest : UnitTestBase
  {
    /// <summary>
    ///A test for GetMessage
    ///</summary>
    [TestMethod]
    //[DatabaseTest]
    public void GetMessageInvalidSenderTest()
    {
      Helpers.NHOp.CreateResponseEndpoint();

      CreateBssProvisioningMsg("3.2.0.1", 0);
      var tester = ExposedClass.From(typeof(NHBssMessageProcessor));
      tester._sendToEndpoints = null;

      BSSProvisioningMsg msg = tester.GetMessage();
      Assert.IsNull(msg, "Message should be null");
      msg = (from b in Ctx.OpContext.BSSProvisioningMsgReadOnly
             where b.SequenceNumber == 2
             select b).SingleOrDefault();
      Assert.IsNotNull(msg);
      Assert.AreEqual((int)BSSStatusEnum.Blocked, msg.fk_BSSStatusID, "status should be blocked");
      Assert.IsNotNull(msg.ProcessedUTC, "ProcessedUTC should not be null");
    }

    /// <summary>
    ///A test for GetMessage
    ///</summary>
    [TestMethod]
    //[DatabaseTest]
    public void GetMessageValidSenderTest()
    {
      Helpers.NHOp.CreateResponseEndpoint();

      CreateBssProvisioningMsg("127.0.0.1", 0);
      var tester = ExposedClass.From(typeof(NHBssMessageProcessor));
      tester._sendToEndpoints = null;

      BSSProvisioningMsg msg = tester.GetMessage();
      Assert.IsNotNull(msg, "Message should not be null");

      msg = (from b in Ctx.OpContext.BSSProvisioningMsgReadOnly
             where b.SequenceNumber == 2
             select b).SingleOrDefault();
      Assert.IsNotNull(msg);
      Assert.AreEqual((int)BSSStatusEnum.InProgress, msg.fk_BSSStatusID, "status should be inProgress");
      Assert.IsNull(msg.ProcessedUTC, "ProcessedUTC should be null");
    }

    [TestMethod]
    //[DatabaseTest]
    public void UpdateRowTooManyFailuresTest()
    {
      Helpers.NHOp.CreateResponseEndpoint();
      var bssMsg = CreateBssProvisioningMsg("127.0.0.1", 4);
      var tester = ExposedClass.From(typeof(NHBssMessageProcessor));
      tester._sendToEndpoints = null;

      tester.UpdateRow(bssMsg, null);

      var msg = (from b in Ctx.OpContext.BSSProvisioningMsgReadOnly
                                where b.SequenceNumber == 2
                                select b).SingleOrDefault();
      Assert.IsNotNull(msg);
      Assert.AreEqual((int)BSSStatusEnum.Failed, msg.fk_BSSStatusID, "status should be Failed");
      Assert.IsNotNull(msg.ProcessedUTC, "ProcessedUTC should not be null");
    }

    [TestMethod]
    //[DatabaseTest]
    public void UpdateRowSuccessfullTest()
    {
      var id = Helpers.NHOp.CreateResponseEndpoint();
      var bssMsg = CreateBssProvisioningMsg("127.0.0.1", 0);
      var tester = ExposedClass.From(typeof(NHBssMessageProcessor));
      tester._sendToEndpoints = null;

      var response = new Response
      {
        ControlNumber = "12",
        EndPointName = Response.EndpointEnum.AccountHierarchy,
        SequenceNumber = 2,
        Success = true.ToString().ToUpper(),
        TargetStack = "US01"
      };

      tester._endPointId = id;
      tester.UpdateRow(bssMsg, response);

      var msg = (from b in Ctx.OpContext.BSSProvisioningMsgReadOnly
                                where b.SequenceNumber == 2
                                select b).SingleOrDefault();
      Assert.IsNotNull(msg);
      Assert.AreEqual((int)BSSStatusEnum.Complete, msg.fk_BSSStatusID, "status should be Complete");
      Assert.IsNotNull(msg.ProcessedUTC, "ProcessedUTC should not be null");

      var responseMsg = (from b in Ctx.OpContext.BSSResponseMsgReadOnly
                                    where b.fk_BSSProvisioningMsgID == msg.SequenceNumber
                                    select b).SingleOrDefault();
      Assert.IsNotNull(responseMsg, "Response message should not be null");
      Assert.AreEqual(id, responseMsg.fk_BSSResponseEndPointID, "Incorrect Destination ID");
      Assert.AreEqual(0, responseMsg.FailedCount, "Incorrect Failed Count");
    }

    [TestMethod]
    [DatabaseTest]
    //[Ignore]
    public void UnknownMessageType_MessageProcess_Failure()
    {
      ProcessMessage(string.Empty, BSSStatusEnum.RetryPending);
    }

    [TestMethod]
    [DatabaseTest]
    //[Ignore]
    public void AccountHierarchy_MessageProcess_Success()
    {
      ProcessMessage("AccountHierarchy");
    }

    [TestMethod]
    [DatabaseTest]
    //[Ignore]
    public void InstallBase_MessageProcess_Success()
    {
      ProcessMessage("InstallBase");
    }

    [TestMethod]
    [DatabaseTest]
    //[Ignore]
    public void ServicePlan_MessageProcess_Success()
    {
      ProcessMessage("ServicePlan");
    }

    [TestMethod]
    [DatabaseTest]
    //[Ignore]
    public void DeviceReplacement_MessageProcess_Success()
    {
      ProcessMessage("DeviceReplacement");
    }

    [TestMethod]
    [DatabaseTest]
    //[Ignore]
    public void DeviceRegistration_MessageProcess_Success()
    {
      ProcessMessage("DeviceRegistration");
    }

    private void ProcessMessage(string messageName, BSSStatusEnum expectedstatus = BSSStatusEnum.Complete)
    {
      // In case the function has been overwritten by another test. It happens...
      Services.ServiceViews = () => new BssServiceViewService();

      Helpers.NHOp.CreateResponseEndpoint();
      var message = CreateBssProvisioningMsg("127.0.0.1", 0, messageName);
      var tester = ExposedClass.From(typeof(NHBssMessageProcessor));
      tester._sendToEndpoints = null;

      tester.ProcessRow(new object());
      var msg = (from b in Ctx.OpContext.BSSProvisioningMsgReadOnly
                 where b.SequenceNumber == message.SequenceNumber
                 select b).SingleOrDefault();
      //make sure the message is processed.
      Assert.IsNotNull(msg, "Response message should not be null");
      Assert.AreEqual((int)expectedstatus, msg.fk_BSSStatusID, "Response message should not be null");
    }

    private BSSProvisioningMsg CreateBssProvisioningMsg(string senderIp, byte failedCount, string messageName = "")
    {
      var msg = new BSSProvisioningMsg
      {
        MachineName = "TESTMachineName",
        BSSMessageType = messageName,
        FailedCount = failedCount,
        fk_BSSStatusID = (int) BSSStatusEnum.Pending,
        InsertUTC = DateTime.UtcNow.AddMinutes(-10),
        MessageXML = GetMessage(messageName),
        ProcessedUTC = null,
        SenderIP = senderIp,
        SequenceNumber = 2,
        UpdateUTC = DateTime.UtcNow
      };
      Ctx.OpContext.BSSProvisioningMsg.AddObject(msg);
      Ctx.OpContext.SaveChanges();

      return msg;
    }

    private string GetMessage(string messageName = "AccountHierarchy")
    {
      string message;
      switch (messageName)
      {
        case "AccountHierarchy":
          message = BssCommon.WriteXML(new AccountHierarchy
          {
            Action = "Created",
            ActionUTC = DateTime.UtcNow.ToString(),
            BSSID = IdGen.StringId(),
            contact = new PrimaryContact
            {
              Email = "test@test.com",
              FirstName = "FirstName",
              LastName = "LastName"
            },
            ControlNumber = IdGen.StringId(),
            CustomerName = IdGen.StringId(),
            CustomerType = "Dealer",
            SequenceNumber = 2,
            TargetStack = "US01"
          });
          break;
        case "InstallBase":
          message = BssCommon.WriteXML(new InstallBase
          {
            Action = "Created",
            ActionUTC = DateTime.UtcNow.ToString(),
            ControlNumber = IdGen.StringId(),
            DeviceState = "Registered",
            GPSDeviceID = IdGen.StringId(),
            IBKey = IdGen.StringId(),
            MakeCode = "A01",
            OwnerBSSID = IdGen.StringId(),
            SequenceNumber = 2,
            PartNumber = IdGen.StringId(),
            TargetStack = "US01"
          });
          break;
        case "ServicePlan":
          message = BssCommon.WriteXML(new ServicePlan
          {
            Action = "Activated",
            ActionUTC = DateTime.UtcNow.ToString(),
            ControlNumber = IdGen.StringId(),
            IBKey = IdGen.StringId(),
            SequenceNumber = 2,
            ServicePlanlineID = IdGen.StringId(),
            ServicePlanName = IdGen.StringId(),
            TargetStack = "US01"
          });
          break;
        case "DeviceReplacement":
          message = BssCommon.WriteXML(new DeviceReplacement
          {
            Action = "Replaced",
            ActionUTC = DateTime.UtcNow.ToString(),
            ControlNumber = IdGen.StringId(),
            NewIBKey = IdGen.StringId(),
            OldIBKey = IdGen.StringId(),
            SequenceNumber = 2,
            TargetStack = "US01"
          });
          break;
        case "DeviceRegistration":
          message = BssCommon.WriteXML(new DeviceRegistration
          {
            Action = "Registered",
            ActionUTC = DateTime.UtcNow.ToString(),
            ControlNumber = IdGen.StringId(),
            IBKey = IdGen.StringId(),
            SequenceNumber = 2,
            Status = "Dereg_Tech",
            TargetStack = "US01"
          });
          break;
        default:
          message = "<Test><Element1>2</Element1></Test>";
          break;
      }
      return message;
    }
  }
}
