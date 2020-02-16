using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests
{
  [TestClass]
  public class FaultAccessTest : UnitTestBase
  {
    [DatabaseTest]
    [TestMethod]
    public void SaveFault_Success()
    {
      string faultsXml = @"
        <root>
        <faults>
        <Fault SignatureID=""123456"" fk_FaultTypeID=""2"" fk_DatalinkID=""1"" CodedDescription=""2"" />
        <Fault SignatureID=""654321"" fk_FaultTypeID=""1"" fk_DatalinkID=""2"" CodedDescription=""3"" />
        </faults>
        </root>
        ";

      bool success = FaultAccess.SaveFaults(faultsXml, 2);
      Assert.IsTrue(success, "Failed to save Fault data.");
      var faults = (from f in Ctx.OpContext.FaultReadOnly
                    where f.SignatureID == 123456
                          || f.SignatureID == 654321
                    select f).ToList();
      Assert.AreEqual(2, faults.Count, "Fault count doesn't match.");
      Assert.AreEqual(2, faults.Where(f=> f.SignatureID == 123456).FirstOrDefault().fk_FaultTypeID, "FaultType doesn't match.");
      Assert.AreEqual(1, faults.Where(f => f.SignatureID == 123456).FirstOrDefault().fk_DatalinkID, "Datalink doesn't match.");
      Assert.AreEqual("3", faults.Where(f => f.SignatureID == 654321).FirstOrDefault().CodedDescription, "CodedDescription doesn't match.");
    }

    [DatabaseTest]
    [TestMethod]
    public void SaveFaultParameter_Success()
    {
      string faultsXml = @"
        <root>
        <faults>
        <Fault SignatureID=""123456"" fk_FaultTypeID=""2"" fk_DatalinkID=""1"" CodedDescription=""2"" />
        <Fault SignatureID=""654321"" fk_FaultTypeID=""1"" fk_DatalinkID=""2"" CodedDescription=""3"" />
        </faults>
        </root>
        ";

      bool success = FaultAccess.SaveFaults(faultsXml, 2);
      Assert.IsTrue(success, "Failed to save Fault data.");

      string faultParametersXml = string.Format(@"
        <root>
        <parameters>
        <FaultParameter SignatureID=""{0}"" fk_FaultParameterTypeID=""2"" Value=""4"" />
        <FaultParameter SignatureID=""{1}"" fk_FaultParameterTypeID=""5"" Value=""8"" />
        </parameters>
        </root>
        ", 123456, 654321);

      success = FaultAccess.SaveFaultParameters(faultParametersXml, 2);
      Assert.IsTrue(success, "Failed to save FaultParameter data.");

      var faults = (from f in Ctx.OpContext.FaultReadOnly
                    where f.SignatureID == 123456
                          || f.SignatureID == 654321
                    select f).ToList();
      Assert.AreEqual(2, faults.Count, "Fault count doesn't match.");
      Assert.AreEqual(2, faults.Where(f => f.SignatureID == 123456).FirstOrDefault().fk_FaultTypeID, "FaultType doesn't match.");
      Assert.AreEqual(1, faults.Where(f => f.SignatureID == 123456).FirstOrDefault().fk_DatalinkID, "Datalink doesn't match.");
      Assert.AreEqual("3", faults.Where(f => f.SignatureID == 654321).FirstOrDefault().CodedDescription, "CodedDescription doesn't match.");
    }

    [DatabaseTest]
    [TestMethod]
    public void SaveFaultDescription_Success()
    {
      string faultsXml = @"
        <root>
        <faults>
        <Fault SignatureID=""123456"" fk_FaultTypeID=""2"" fk_DatalinkID=""1"" CodedDescription=""2"" />
        <Fault SignatureID=""654321"" fk_FaultTypeID=""1"" fk_DatalinkID=""2"" CodedDescription=""3"" />
        </faults>
        </root>
        ";

      bool success = FaultAccess.SaveFaults(faultsXml, 2);
      Assert.IsTrue(success, "Failed to save Fault data.");

      string faultDescriptionsXml = string.Format(@"
        <root>
        <descriptions>
        <FaultDescription SignatureID=""{0}"" fk_LanguageID=""8"" Description=""9"" />
        <FaultDescription SignatureID=""{1}"" fk_LanguageID=""9"" Description=""10"" />
        </descriptions>
        </root>
        ", 123456, 654321);

      success = FaultAccess.SaveFaultDescriptions(faultDescriptionsXml, 2);
      Assert.IsTrue(success, "Failed to save FaultDescription data.");

      var faults = (from f in Ctx.OpContext.FaultReadOnly
                    where f.SignatureID == 123456
                          || f.SignatureID == 654321
                    select f).ToList();
      Assert.AreEqual(2, faults.Count, "Fault count doesn't match.");
      Assert.AreEqual(2, faults.Where(f => f.SignatureID == 123456).FirstOrDefault().fk_FaultTypeID, "FaultType doesn't match.");
      Assert.AreEqual(1, faults.Where(f => f.SignatureID == 123456).FirstOrDefault().fk_DatalinkID, "Datalink doesn't match.");
      Assert.AreEqual("3", faults.Where(f => f.SignatureID == 654321).FirstOrDefault().CodedDescription, "CodedDescription doesn't match.");
    }

    [DatabaseTest]
    [TestMethod]
    public void UpdateFaultDescription_Success()
    {
      string faultsXml = @"
        <root>
        <faults>
        <Fault SignatureID=""123456"" fk_FaultTypeID=""2"" fk_DatalinkID=""1"" CodedDescription=""2"" />
        <Fault SignatureID=""654321"" fk_FaultTypeID=""1"" fk_DatalinkID=""2"" CodedDescription=""3"" />
        </faults>
        </root>
        ";

      bool success = FaultAccess.SaveFaults(faultsXml, 2);
      Assert.IsTrue(success, "Failed to save Fault data.");

      string faultDescriptionsXml = string.Format(@"
        <root>
        <descriptions>
        <FaultDescription SignatureID=""{0}"" fk_LanguageID=""8"" Description=""9"" />
        <FaultDescription SignatureID=""{1}"" fk_LanguageID=""9"" Description=""10"" />
        </descriptions>
        </root>
        ", 123456, 654321);

      success = FaultAccess.SaveFaultDescriptions(faultDescriptionsXml, 2);
      Assert.IsTrue(success, "Failed to save FaultDescription data.");

      string updatedDescription = "updated description";
      faultDescriptionsXml = string.Format(@"
        <root>
        <descriptions>
        <FaultDescription SignatureID=""{0}"" fk_LanguageID=""8"" Description=""{1}"" />
        </descriptions>
        </root>
        ", 123456, updatedDescription);

      success = FaultAccess.SaveFaultDescriptions(faultDescriptionsXml, 1);
      Assert.IsTrue(success, "Failed to update FaultDescription data.");
      long faultID = (from f in Ctx.OpContext.Fault where f.SignatureID == 123456 select f.ID).First();
      var actual = (from fd in Ctx.OpContext.FaultDescriptionReadOnly
                    where fd.fk_FaultID == faultID
                    && fd.fk_LanguageID == 8
                    select fd.Description).FirstOrDefault();
      Assert.AreEqual(updatedDescription, actual, "Fault description doesn't match.");
    }

    [DatabaseTest]
    [TestMethod]
    public void CalculateFaultSignatureID_Success()
    {
      string codedDescription = "FMI:99999 CID:99999 DL:SAEJ1939";
      long actualSignatureID = FaultAccess.GenerateSignatureID(codedDescription);
      long expectedSignatureID = 3107738837569546055;
      Assert.AreEqual(expectedSignatureID, actualSignatureID, "SignatureID didn't calculate properly.");
    }

    [DatabaseTest]
    [TestMethod]
    public void CalculateFaultSignatureID_IsUnique_Success()
    {
      string codedDescription2 = "EID:664 DL:CDL";
      long actualSignatureID2 = FaultAccess.GenerateSignatureID(codedDescription2);
      string codedDescription3 = "EID:657 DL:CDL";
      long actualSignatureID3 = FaultAccess.GenerateSignatureID(codedDescription3);
      Assert.AreNotEqual(actualSignatureID2, actualSignatureID3, "SignatureIDs should be unique.");
    }

    [DatabaseTest]
    [TestMethod]
    public void CalculateFaultSignatureID_IsUniqueForMaxCodedDescription_Success()
    {
      string codedDescription2 = "FMI:99999 CID:99999 DL:SAEJ1939";
      long actualSignatureID2 = FaultAccess.GenerateSignatureID(codedDescription2);
      string codedDescription3 = "FMI:99998 CID:99999 DL:SAEJ1939";
      long actualSignatureID3 = FaultAccess.GenerateSignatureID(codedDescription3);
      Assert.AreNotEqual(actualSignatureID2, actualSignatureID3, "SignatureIDs should be unique.");
    }
  }
}
