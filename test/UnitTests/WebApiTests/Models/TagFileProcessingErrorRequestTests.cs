using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WebApiModels.Models;

namespace WebApiTests.Models
{
  [TestClass]
  public class TagFileProcessingErrorRequestTests
  {
    [TestMethod]
    public void TagFileProcessingErrorRequest_ValidatorCase_1()
    {
      TagFileProcessingErrorRequest request = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(-1, "", 0);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Must have assetId", StringComparison.Ordinal));
    }

    [TestMethod]
    public void TagFileProcessingErrorRequest_ValidatorCase_2()
    {
      TagFileProcessingErrorRequest request = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(34345, "", 0);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Must have filename", StringComparison.Ordinal));
    }

    [TestMethod]
    public void TagFileProcessingErrorRequest_ValidatorCase_3()
    {
      TagFileProcessingErrorRequest request = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(34345, "aFilename", 0);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Must have valid error number", StringComparison.Ordinal));
    }

    [TestMethod]
    public void TagFileProcessingErrorRequest_ValidatorCase_4()
    {
      TagFileProcessingErrorRequest request = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(34345, "aFilename", -3);
      var ex = Assert.ThrowsException<ServiceException>(() => request.Validate());
      Assert.AreNotEqual(-1, ex.GetContent.IndexOf("Must have valid error number", StringComparison.Ordinal));
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
