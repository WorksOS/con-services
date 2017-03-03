using Microsoft.VisualStudio.TestTools.UnitTesting;
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
      TagFileProcessingErrorRequest request = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(0, "", 0);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }

    [TestMethod]
    public void TagFileProcessingErrorRequest_ValidatorCase_2()
    {
      TagFileProcessingErrorRequest request = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(1, "", 0);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }

    [TestMethod]
    public void TagFileProcessingErrorRequest_ValidatorCase_3()
    {
      TagFileProcessingErrorRequest request = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(1, "Data from my dozer", 0);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }

    [TestMethod]
    public void TagFileProcessingErrorRequest_ValidatorCase_4()
    {
      TagFileProcessingErrorRequest request = TagFileProcessingErrorRequest.CreateTagFileProcessingErrorRequest(1, "", TagFileErrorsEnum.ProjectID_MultipleProjects);
      Assert.ThrowsException<ServiceException>(() => request.Validate());
    }

  }
}
