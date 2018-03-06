using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.WebApi.Compaction.Controllers.Filters;

namespace VSS.Productivity3D.WebApiTests.Compaction.Filters
{
  [TestClass]
  public class ValidateWidthAndHeightAttributeTests : ActionFilterAttributeTestsBase
  {
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("?")]
    [DataRow("&width=256")]
    [DataRow("&width=256&")]
    [DataRow("&height=256")]
    [DataRow("&width=abc&height=256")]
    [DataRow("&width=256&height=abc")]
    public void Should_return_ServiceException_When_dimensions_are_badly_formatted(string widthAndHeight)
    {
      var context = CreateActionExecutingContext(new DefaultHttpContext
      {
        Request = {
          QueryString = new QueryString($"?ProjectUid=ff91dd40-1569-4765-a2bc-014321f76ace&mode=height{widthAndHeight}")
        }
      });

      var validationAttribute = new ValidateWidthAndHeightAttribute();

      Assert.ThrowsException<ServiceException>(() => validationAttribute.OnActionExecuting(context));
    }

    [TestMethod]
    [DataRow("&width=255&height=256")]
    [DataRow("&width=256&height=255")]
    public void Should_throw_When_dimensions_are_invalid(string widthAndHeight)
    {
      var context = CreateActionExecutingContext(new DefaultHttpContext
      {
        Request = {
          QueryString = new QueryString($"?ProjectUid=ff91dd40-1569-4765-a2bc-014321f76ace&mode=height{widthAndHeight}")
        }
      });

      var validationAttribute = new ValidateWidthAndHeightAttribute();

      Assert.ThrowsException<ServiceException>(() => validationAttribute.OnActionExecuting(context));
    }

    [TestMethod]
    [DataRow("&width=256&height=256")]
    public void Should_return_successfully_When_dimensions_are_valid(string widthAndHeight)
    {
      var context = CreateActionExecutingContext(new DefaultHttpContext
      {
        Request = {
          QueryString = new QueryString($"?ProjectUid=ff91dd40-1569-4765-a2bc-014321f76ace&mode=height{widthAndHeight}")
        }
      });

      var validationAttribute = new ValidateWidthAndHeightAttribute();

      validationAttribute.OnActionExecuting(context);

      Assert.IsNull(context.Result);
    }
  }
}