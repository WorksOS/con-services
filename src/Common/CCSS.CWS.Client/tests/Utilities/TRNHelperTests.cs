using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Abstractions.Clients.CWS.Utilities;

namespace CCSS.CWS.Client.UnitTests.Utilities
{
  [TestClass]
  public class TRNHelperTests
  {

    [TestMethod]
    [DataRow("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_ACCOUNT, "trn::profilex:us-west-2:account:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [DataRow("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_USER, "trn::profilex:us-west-2:user:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [DataRow("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_PROJECT, "trn::profilex:us-west-2:project:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [DataRow("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_DEVICE, "trn::profilex:us-west-2:device:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    public void ExtractGuidFromTRNTest(string expectedGuidString, string TRNtype, string userTRN)
    {
      var convertedGuid = TRNHelper.ExtractGuid(userTRN);
      Assert.AreEqual(expectedGuidString, convertedGuid.ToString());
    }

    [TestMethod]
    [DataRow("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_ACCOUNT, "trn::profilex:us-west-2:account:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [DataRow("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_USER, "trn::profilex:us-west-2:user:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [DataRow("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_PROJECT, "trn::profilex:us-west-2:project:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [DataRow("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_DEVICE, "trn::profilex:us-west-2:device:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    public void ExtractGuidFromTRNAsStringTest(string expectedGuidString, string TRNtype, string userTRN)
    {
      var convertedGuidString = TRNHelper.ExtractGuidAsString(userTRN);
      Assert.AreEqual(expectedGuidString, convertedGuidString);
    }

    [TestMethod]
    [DataRow("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_ACCOUNT, "trn::profilex:us-west-2:account:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [DataRow("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_USER, "trn::profilex:us-west-2:user:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [DataRow("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_PROJECT, "trn::profilex:us-west-2:project:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [DataRow("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_DEVICE, "trn::profilex:us-west-2:device:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    public void MakeTRNFromGuidStringTest(string guidString, string TRNtype, string expectedUserTRN)
    { 
      var convertedUserTRN = TRNHelper.MakeTRN(guidString, TRNtype);
      Assert.AreEqual(expectedUserTRN, convertedUserTRN);
    }

    [TestMethod]
    [DataRow("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_ACCOUNT, "trn::profilex:us-west-2:account:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [DataRow("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_USER, "trn::profilex:us-west-2:user:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [DataRow("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_PROJECT, "trn::profilex:us-west-2:project:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    [DataRow("d79a392d-6513-46c1-baa1-75c537cf0c32", TRNHelper.TRN_DEVICE, "trn::profilex:us-west-2:device:d79a392d-6513-46c1-baa1-75c537cf0c32")]
    public void MakeTRNFromGuidTest(string guidString, string TRNtype, string expectedUserTRN)
    {
      var guid = new Guid(guidString);
      var convertedUserTRN = TRNHelper.MakeTRN(guid, TRNtype);
      Assert.AreEqual(expectedUserTRN, convertedUserTRN);
    }

  }
}

