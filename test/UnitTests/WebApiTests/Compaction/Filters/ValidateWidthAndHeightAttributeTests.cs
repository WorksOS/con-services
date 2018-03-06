using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.WebApi.Compaction.Controllers.Filters;

namespace VSS.Productivity3D.WebApiTests.Compaction.Filters
{
  [TestClass]
  public class ValidateBoundingBoxAttributeTests : ActionFilterAttributeTestsBase
  {
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("?")]
    public void Should_return_ServiceException_When_bbox_parameter_is_not_found(string bbox)
    {
      var context = CreateActionExecutingContext(new DefaultHttpContext
      {
        Request = {
          QueryString = new QueryString($"?ProjectUid=ff91dd40-1569-4765-a2bc-014321f76ace&mode=height&width=256&height=256{bbox}")
        }
      });

      var validationAttribute = new ValidateBoundingBoxAttribute();

      Assert.ThrowsException<ServiceException>(() => validationAttribute.OnActionExecuting(context));
    }

    [TestMethod]
    [DataRow("&bbox=36.207437%2C+-115.019999%2C+36.207473%2C+-115.019959")]
    public void Should_return_successfully_When_bbox_parameter_is_found(string bbox)
    {
      var context = CreateActionExecutingContext(new DefaultHttpContext
      {
        Request = {
          QueryString = new QueryString($"?ProjectUid=ff91dd40-1569-4765-a2bc-014321f76ace&mode=height&width=256&height=256{bbox}")
        }
      });

      var validationAttribute = new ValidateBoundingBoxAttribute();

      validationAttribute.OnActionExecuting(context);

      Assert.IsNull(context.Result);
    }
  }
}