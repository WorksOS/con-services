using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.TagFileAuth.Service.WebApiModels.Enums;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VSS.TagFileAuth.Service.WebApiTests.Models
{
  [TestClass]
  public class TagFileProcessingErrorRequestTests
  {
    [TestMethod]
    public void TagFileProcessingErrorRequest_ValidatorCase_1()
    {
      TagFileProcessingErrorRequest request = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(-1, "", 0);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(String.Format("Must have assetId {0}", -1)));
    }

    [TestMethod]
    public void TagFileProcessingErrorRequest_ValidatorCase_2()
    {
      TagFileProcessingErrorRequest request = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(34345, "", 0);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Must have filename"));
    }

    [TestMethod]
    public void TagFileProcessingErrorRequest_ValidatorCase_3()
    {
      TagFileProcessingErrorRequest request = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(34345, "aFilename", 0);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Must have valid error number"));
    }

    [TestMethod]
    public void TagFileProcessingErrorRequest_ValidatorCase_4()
    {
      TagFileProcessingErrorRequest request = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(34345, "aFilename", -3);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Must have valid error number"));
    }

    [TestMethod]
    public void TagFileProcessingErrorRequest_ValidatorCase_5()
    {
      TagFileProcessingErrorRequest request = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(34345, "aFilename", 10);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Must have valid error number"));
    }

    [TestMethod]
    public void TagFileProcessingErrorRequest_ValidatorCase_6()
    {
      TagFileProcessingErrorRequest request = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(34345, "aFilename", 6);
      request.Validate();
    }

  }
}
