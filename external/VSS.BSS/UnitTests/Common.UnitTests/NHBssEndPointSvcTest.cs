using System;
using System.Linq;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Nighthawk.NHBssSvc.BSSEndPoints;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;
using Microsoft.Web.Services;
using Microsoft.ServiceModel.Web;
using System.Xml.Linq;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace UnitTests
{
    
    
    /// <summary>
    ///This is a test class for NHBssEndPointSvcTest and is intended
    ///to contain all NHBssEndPointSvcTest Unit Tests
    ///</summary>
  [TestClass()]
  public class NHBssEndPointSvcTest : UnitTestBase
  {
    
    static int ValidationErrors = 0;

    /// <summary>
    ///A test for AccountHierarchiesV2 with valid target stack
    ///</summary>
    [TestMethod()]
    public void AccountHierarchiesV2ValidTargetStackTest()
    {
      string targetStack = "US01";
      NHBssEndPointSvc target = new NHBssEndPointSvc();
      AccountHierarchy ah = CreateAccountHierarchy(targetStack);

      target.AccountHierarchiesV2(ah);

      BSSProvisioningMsg msg = (from b in Ctx.OpContext.BSSProvisioningMsgReadOnly
                                where b.SequenceNumber == ah.SequenceNumber
                                select b).SingleOrDefault();
      Assert.IsNotNull(msg, "message should not be null");
      Assert.AreEqual("AccountHierarchy", msg.BSSMessageType, "MessageType should be AccountHierarchy");
      Assert.AreEqual((int)BSSStatusEnum.Pending, msg.fk_BSSStatusID, "incorrect Status");
      Assert.AreEqual(0, msg.FailedCount, "incorrect failed Count");
    }

    /// <summary>
    ///A test for AccountHierarchiesV2 with invalid target stack
    ///</summary>
    [TestMethod()]
    [ExpectedException(typeof(HttpException))]
    public void AccountHierarchiesV2InvalidValidTargetStackTest()
    {
      string targetStack = "US02";
      NHBssEndPointSvc target = new NHBssEndPointSvc();
      AccountHierarchy ah = CreateAccountHierarchy(targetStack);

      target.AccountHierarchiesV2(ah);

      BSSProvisioningMsg msg = (from b in Ctx.OpContext.BSSProvisioningMsgReadOnly
                                where b.SequenceNumber == ah.SequenceNumber
                                select b).SingleOrDefault();
      Assert.IsNull(msg, "message should be null");
    }

    /// <summary>
    ///A test for InstallBasesV2
    ///</summary>
    [TestMethod()]
    public void InstallBasesV2ValidTargetStackTest()
    {
      string targetStack = "US01";
      NHBssEndPointSvc target = new NHBssEndPointSvc();
      InstallBase installBasePayload = CreateInstallBase(targetStack);
      target.InstallBasesV2(installBasePayload);

      BSSProvisioningMsg msg = (from b in Ctx.OpContext.BSSProvisioningMsgReadOnly
                                where b.SequenceNumber == installBasePayload.SequenceNumber
                                select b).SingleOrDefault();
      Assert.IsNotNull(msg, "message should not be null");
      Assert.AreEqual("InstallBase", msg.BSSMessageType, "MessageType should be AccountHierarchy");
      Assert.AreEqual((int)BSSStatusEnum.Pending, msg.fk_BSSStatusID, "incorrect Status");
      Assert.AreEqual(0, msg.FailedCount, "incorrect failed Count");
    }

    /// <summary>
    ///A test for InstallBasesV2
    ///</summary>
    [TestMethod()]
    [ExpectedException(typeof(HttpException))]
    public void InstallBasesV2InvalidValidTargetStackTest()
    {
      string targetStack = "US02";
      NHBssEndPointSvc target = new NHBssEndPointSvc();
      InstallBase installBasePayload = CreateInstallBase(targetStack);
      target.InstallBasesV2(installBasePayload);

      BSSProvisioningMsg msg = (from b in Ctx.OpContext.BSSProvisioningMsgReadOnly
                                where b.SequenceNumber == installBasePayload.SequenceNumber
                                select b).SingleOrDefault();
      Assert.IsNull(msg, "message should be null");
    }

    /// <summary>
    ///A test for ServicePlansV2
    ///</summary>
    [TestMethod()]
    public void ServicePlansV2ValidTargetStackTest()
    {
      NHBssEndPointSvc target = new NHBssEndPointSvc();
      string targetStack = "US01";
      ServicePlan servicePlan = CreateServicePlan(targetStack);

      target.ServicePlansV2(servicePlan);
      BSSProvisioningMsg msg = (from b in Ctx.OpContext.BSSProvisioningMsgReadOnly
                                where b.SequenceNumber == servicePlan.SequenceNumber
                                select b).SingleOrDefault();
      Assert.IsNotNull(msg, "message should not be null");
      Assert.AreEqual("ServicePlan", msg.BSSMessageType, "MessageType should be AccountHierarchy");
      Assert.AreEqual((int)BSSStatusEnum.Pending, msg.fk_BSSStatusID, "incorrect Status");
      Assert.AreEqual(0, msg.FailedCount, "incorrect failed Count");
    }

    /// <summary>
    ///A test for ServicePlansV2
    ///</summary>
    [TestMethod()]
    [ExpectedException(typeof(HttpException))]
    public void ServicePlansV2InValidTargetStackTest()
    {
      NHBssEndPointSvc target = new NHBssEndPointSvc();
      string targetStack = "US02";
      ServicePlan servicePlan = CreateServicePlan(targetStack);

      target.ServicePlansV2(servicePlan);
      BSSProvisioningMsg msg = (from b in Ctx.OpContext.BSSProvisioningMsgReadOnly
                                where b.SequenceNumber == servicePlan.SequenceNumber
                                select b).SingleOrDefault();
      Assert.IsNull(msg, "message should be null");
    }

    /// <summary>
    ///A test for DeviceReplacementV2
    ///</summary>
    [TestMethod()]
    public void DeviceReplacementV2ValidTargetStackTest()
    {
      string targetStack = "US01";
      NHBssEndPointSvc target = new NHBssEndPointSvc();
      DeviceReplacement deviceReplacement = CreateDeviceReplacement(targetStack);
      target.DeviceReplacementV2(deviceReplacement);
      BSSProvisioningMsg msg = (from b in Ctx.OpContext.BSSProvisioningMsgReadOnly
                                where b.SequenceNumber == deviceReplacement.SequenceNumber
                                select b).SingleOrDefault();
      Assert.IsNotNull(msg, "message should not be null");
      Assert.AreEqual("DeviceReplacement", msg.BSSMessageType, "MessageType should be AccountHierarchy");
      Assert.AreEqual((int)BSSStatusEnum.Pending, msg.fk_BSSStatusID, "incorrect Status");
      Assert.AreEqual(0, msg.FailedCount, "incorrect failed Count");
    }

    /// <summary>
    ///A test for DeviceReplacementV2
    ///</summary>
    [TestMethod()]
    [ExpectedException(typeof(HttpException))]
    public void DeviceReplacementV2InvalidTargetStackTest()
    {
      string targetStack = "US02";
      NHBssEndPointSvc target = new NHBssEndPointSvc();
      DeviceReplacement deviceReplacement = CreateDeviceReplacement(targetStack);
      target.DeviceReplacementV2(deviceReplacement);
      BSSProvisioningMsg msg = (from b in Ctx.OpContext.BSSProvisioningMsgReadOnly
                                where b.SequenceNumber == deviceReplacement.SequenceNumber
                                select b).SingleOrDefault();
      Assert.IsNull(msg, "message should not be null");
    }

    /// <summary>
    ///A test for DeviceReplacementV2
    ///</summary>
    [TestMethod()]
    public void DeviceRegisterdV2ValidTargetStackTest()
    {
      string targetStack = "US01";
      NHBssEndPointSvc target = new NHBssEndPointSvc();
      var deviceRegistration = CreateDeviceRegistration(targetStack);
      target.DeviceRegistrationV2(deviceRegistration);
      BSSProvisioningMsg msg = (from b in Ctx.OpContext.BSSProvisioningMsgReadOnly
                                where b.SequenceNumber == deviceRegistration.SequenceNumber
                                select b).SingleOrDefault();
      Assert.IsNotNull(msg, "message should not be null");
      Assert.AreEqual("DeviceRegistration", msg.BSSMessageType, "MessageType should be DeviceRegistration");
      Assert.AreEqual((int)BSSStatusEnum.Pending, msg.fk_BSSStatusID, "incorrect Status");
      Assert.AreEqual(0, msg.FailedCount, "incorrect failed Count");
    }

    /// <summary>
    ///A test for DeviceReplacementV2
    ///</summary>
    [TestMethod()]
    [ExpectedException(typeof(HttpException))]
    public void DeviceRegisteredV2InvalidTargetStackTest()
    {
      string targetStack = "US02";
      NHBssEndPointSvc target = new NHBssEndPointSvc();
      var deviceRegistration = CreateDeviceRegistration(targetStack);
      target.DeviceRegistrationV2(deviceRegistration);
      BSSProvisioningMsg msg = (from b in Ctx.OpContext.BSSProvisioningMsgReadOnly
                                where b.SequenceNumber == deviceRegistration.SequenceNumber
                                select b).SingleOrDefault();
      Assert.IsNull(msg, "message should not be null");
    }

    private static DeviceReplacement CreateDeviceReplacement(string targetStack)
    {
      DeviceReplacement deviceReplacement = new DeviceReplacement();
      deviceReplacement.Action = ActionEnum.Activated.ToString();
      deviceReplacement.ActionUTC = DateTime.UtcNow.ToString();
      deviceReplacement.ControlNumber = "1";
      deviceReplacement.SequenceNumber = 2;
      deviceReplacement.OldIBKey = "1";
      deviceReplacement.NewIBKey = "2";
      deviceReplacement.TargetStack = targetStack;
      return deviceReplacement;
    }

    private static ServicePlan CreateServicePlan(string targetStack)
    {
      ServicePlan servicePlan = new ServicePlan();
      servicePlan.Action = ActionEnum.Activated.ToString();
      servicePlan.ActionUTC = DateTime.UtcNow.ToString();
      servicePlan.ControlNumber = "1";
      servicePlan.IBKey = "2";
      servicePlan.OwnerVisibilityDate = DateTime.UtcNow.ToString();
      servicePlan.SequenceNumber = 2;
      servicePlan.ServicePlanlineID = "2";
      servicePlan.ServicePlanName = "Test";
      servicePlan.ServiceTerminationDate = DateTime.UtcNow.ToString();
      servicePlan.TargetStack = targetStack;
      return servicePlan;
    }

    private static InstallBase CreateInstallBase(string targetStack)
    {
      InstallBase installBasePayload = new InstallBase();
      installBasePayload.Action = ActionEnum.Activated.ToString();
      installBasePayload.ActionUTC = DateTime.UtcNow.ToShortDateString();
      installBasePayload.CellularModemIMEA = "1";
      installBasePayload.ControlNumber = 2.ToString();
      installBasePayload.EquipmentLabel = "3";
      installBasePayload.FirmwareVersionID = 4.ToString();
      installBasePayload.GPSDeviceID = "test";
      installBasePayload.IBKey = 4.ToString();
      installBasePayload.MakeCode = "5";
      installBasePayload.Model = "6";
      installBasePayload.ModelYear = 2011.ToString();
      installBasePayload.OwnerBSSID = 7.ToString();
      installBasePayload.PartNumber = 7.ToString();
      installBasePayload.SequenceNumber = 2;
      installBasePayload.EquipmentSN = "12";
      installBasePayload.SIMSerialNumber = "13";
      installBasePayload.SIMState = "14";
      installBasePayload.TargetStack = targetStack;
      installBasePayload.EquipmentVIN = "12";
      return installBasePayload;
    }

    private static AccountHierarchy CreateAccountHierarchy(string targetStack)
    {
      AccountHierarchy ah = new AccountHierarchy();
      ah.TargetStack = targetStack;
      ah.SequenceNumber = 2;
      ah.RelationshipID = "3";
      ah.ParentBSSID = "4";
      ah.NetworkDealerCode = "network";
      ah.NetworkCustomerCode = "networkCustomer";
      ah.HierarchyType = "Customer";
      ah.DealerNetwork = "DealerNetwork";
      ah.DealerAccountCode = "accountCode";
      ah.CustomerType = AccountHierarchy.BSSCustomerTypeEnum.ACCOUNT.ToString();
      ah.CustomerName = "Customer";
      ah.ControlNumber = "5";
      ah.contact = new PrimaryContact();
      ah.contact.FirstName = "Test";
      ah.contact.LastName = "Test";
      ah.contact.Email = "test@test";
      ah.BSSID = IdGen.GetId().ToString();
      ah.ActionUTC = DateTime.UtcNow.ToString();
      ah.Action = ActionEnum.Activated.ToString();
      return ah;
    }

    public static DeviceRegistration CreateDeviceRegistration(string targetStack)
    {
      return new DeviceRegistration
      {
        TargetStack = targetStack,
        SequenceNumber = 2,
        ControlNumber = "2",
        Action = ActionEnum.Reactivated.ToString(),
        ActionUTC = DateTime.UtcNow.ToString(),
        IBKey = IdGen.StringId(),
        Status = DeviceRegistrationStatusEnum.DEREG_STORE.ToString(),
      };
    }
    
    [TestMethod()]
    [DatabaseTest()]
    [ExpectedException(typeof(WebProtocolException))]
    public void AssetIDChangesTest_InvalidDate()
    {
      Asset testAsset = TestData.TestAssetPL121;
      Customer testCust = TestData.TestCustomer;
      User testUser = TestData.TestCustomerAdminUser;
      AssetAlias alias = new AssetAlias();
      alias.DealerAccountCode = "TestAccountCode";
      alias.fk_AssetID = testAsset.AssetID;
      alias.fk_CustomerID = testCust.ID;
      alias.fk_UserID = testUser.ID;
      alias.IBKey = "TestKey";
      alias.InsertUTC = DateTime.Parse("2013-01-16 12:00:00");
      alias.Name = testAsset.Name;
      alias.NetworkCustomerCode = "TestCustCode";
      alias.NetworkDealerCode = "TestDealerCode";
      alias.OwnerBSSID = testCust.BSSID;
      Ctx.OpContext.AssetAlias.AddObject(alias);
      Ctx.OpContext.SaveChanges();
      NHBssEndPointSvc target = new NHBssEndPointSvc();
      AssetIDChanges result = target.AssetIDChanges("2013-01-15 12:00");            
    }
    [TestMethod()]
    [DatabaseTest()]
    [ExpectedException(typeof(WebProtocolException))]
    public void AssetIDChangesTest_EmptyDate()
    {
      Asset testAsset = TestData.TestAssetPL121;
      Customer testCust = TestData.TestCustomer;
      User testUser = TestData.TestCustomerAdminUser;
      AssetAlias alias = new AssetAlias();
      alias.DealerAccountCode = "TestAccountCode";
      alias.fk_AssetID = testAsset.AssetID;
      alias.fk_CustomerID = testCust.ID;
      alias.fk_UserID = testUser.ID;
      alias.IBKey = "TestKey";
      alias.InsertUTC = DateTime.Parse("2013-01-16 12:00:00");
      alias.Name = testAsset.Name;
      alias.NetworkCustomerCode = "TestCustCode";
      alias.NetworkDealerCode = "TestDealerCode";
      alias.OwnerBSSID = testCust.BSSID;
      Ctx.OpContext.AssetAlias.AddObject(alias);
      Ctx.OpContext.SaveChanges();
      NHBssEndPointSvc target = new NHBssEndPointSvc();
      AssetIDChanges result = target.AssetIDChanges("");
    }
    [TestMethod()]
    [DatabaseTest()]
    public void AssetIDChangesTest_SchemaValidation()
    {
      Asset testAsset = TestData.TestAssetPL121;
      Customer testCust = TestData.TestCustomer;
      User testUser = TestData.TestCustomerAdminUser;
      AssetAlias alias = new AssetAlias();
      alias.DealerAccountCode = "TestAccountCode";
      alias.fk_AssetID = testAsset.AssetID;
      alias.fk_CustomerID = testCust.ID;
      alias.fk_UserID = testUser.ID;
      alias.IBKey = "TestKey";
      alias.InsertUTC = DateTime.Parse("2013-01-16 12:00:00");
      alias.Name = testAsset.Name;
      alias.NetworkCustomerCode = "TestCustCode";
      alias.NetworkDealerCode = "TestDealerCode";
      alias.OwnerBSSID = testCust.BSSID;
      Ctx.OpContext.AssetAlias.AddObject(alias);
      Ctx.OpContext.SaveChanges();
      NHBssEndPointSvc target = new NHBssEndPointSvc();
      AssetIDChanges result = target.AssetIDChanges("2013-01-15T12:00:00Z");
      string xmlResponse = BssCommon.WriteXML(result);
      ValidateXML(xmlResponse, "AssetIDChanges.xsd");
      Assert.AreEqual(0, ValidationErrors, "XML Schema validation throws Validation Errors");     
    }

    private void ValidateXML(string xml, string schemafile)
    {
      ValidationErrors = 0;
      XmlReader xsd = new XmlTextReader(GetFullTestFilePathAndVerifyExistence(schemafile));
      XmlSchemaSet schema = new XmlSchemaSet();
      schema.Add(null, xsd);

      XmlReaderSettings xmlReadeSettings = new XmlReaderSettings();
      xmlReadeSettings.ValidationType = ValidationType.Schema;
      xmlReadeSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
      xmlReadeSettings.Schemas.Add(schema);
      xmlReadeSettings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);

      StringReader stream = new StringReader(xml);
      XmlTextReader xmlTextReader = new XmlTextReader(stream);
      XmlReader xmlReader = XmlReader.Create(xmlTextReader, xmlReadeSettings);
      using (XmlReader validatingReader = XmlReader.Create(new StringReader(xml), xmlReadeSettings))
      {

        while (!validatingReader.EOF)
        {
          try
          {
            validatingReader.Read();
          }
          catch (InvalidOperationException opError)
          {
            if (!validatingReader.Name.Equals("schema"))
            {
              throw opError;
            }
          }
        }
      }
    }

    private void ValidationCallBack(object sender, ValidationEventArgs e)
    {
      ValidationErrors++;
      throw e.Exception;
    }

    private string GetFullTestFilePathAndVerifyExistence(string filename)
    {
      string testFilePath = Path.Combine(Environment.CurrentDirectory + "\\Schema", filename);
      Assert.IsTrue(File.Exists(testFilePath), testFilePath);
      return testFilePath;
    }
  }
}
