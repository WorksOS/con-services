using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.WebApi.Compaction.Controllers.Filters;

namespace VSS.Productivity3D.WebApiTests.Compaction.Filters
{
  // MSTest TestClass doesn't work if made generic with <T> where T : ValidateWidthAndHeightAttribute, new()
  [TestClass]
  public class ValidateTileParametersAttributeTests : ActionFilterAttributeTestsBase
  {
    private const string QUERY_BASE = "?ProjectUid=7925f179-013d-4aaf-aff4-7b9833bb06d6&mode=0&bbox=36.207437%2C+-115.019999%2C+36.207473%2C+-115.019959&width=256&height=256";

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("?")]
    public void Should_return_ServiceException_When_bbox_parameter_is_not_found(string bbox)
    {
      var queryString = $"?ProjectUid=ff91dd40-1569-4765-a2bc-014321f76ace&mode=height&width=256&height=256{bbox}";
      Assert.ThrowsException<ServiceException>(() => CallOnActionExecuting<ValidateTileParametersAttribute>(queryString));
    }

    [TestMethod]
    public void Should_return_successfully_When_bbox_parameter_is_found()
    {
      const string queryString = "?ProjectUid=ff91dd40-1569-4765-a2bc-014321f76ace&mode=height&width=256&height=256&bbox=36.207437%2C+-115.019999%2C+36.207473%2C+-115.019959";
      var context = CallOnActionExecuting<ValidateTileParametersAttribute>(queryString);

      Assert.IsNull(context.Result);
    }

    [TestMethod]
    public void Should_return_successfully_When_all_WMS_parameters_are_provided()
    {
      const string queryString = "?ProjectUid=7925f179-013d-4aaf-aff4-7b9833bb06d6&mode=0&bbox=36.207437%2C+-115.019999%2C+36.207473%2C+-115.019959&width=256&height=256&service=WMS&version=1.3.0&request=GETMAP&format=IMAGE/PNG&transparent=true&layers=LAYERS&crs=EPSG:4326";

      var context = CallOnActionExecuting<ValidateTileParametersAttribute>(queryString);

      Assert.IsNull(context.Result);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("WMS")]
    [DataRow("wms")]
    public void Should_pass_validation_When_Service_parameter_is_valid(string parameter)
    {
      var queryString = $"{QUERY_BASE}&service={parameter}&version=1.3.0&request=GETMAP&format=IMAGE/PNG&transparent=true&layers=LAYERS&crs=EPSG:4326";

      var context = CallOnActionExecuting<ValidateTileParametersAttribute>(queryString);

      Assert.IsNull(context.Result);
    }

    [TestMethod]
    [DataRow("invalid")]
    public void Should_throw_When_Service_parameter_fails_validation(string parameter)
    {
      var queryString = $"{QUERY_BASE}&service={parameter}&version=1.3.0&request=GETMAP&format=IMAGE/PNG&transparent=true&layers=LAYERS&crs=EPSG:4326";

      Assert.ThrowsException<ServiceException>(() => CallOnActionExecuting<ValidateTileParametersAttribute>(queryString));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("1.3.0")]
    public void Should_pass_validation_When_Version_parameter_is_valid(string parameter)
    {
      var queryString = $"{QUERY_BASE}&service=WMS&version={parameter}&request=GETMAP&format=IMAGE/PNG&transparent=true&layers=LAYERS&crs=EPSG:4326";

      var context = CallOnActionExecuting<ValidateTileParametersAttribute>(queryString);

      Assert.IsNull(context.Result);
    }

    [TestMethod]
    [DataRow("invalid")]
    [DataRow("1.3.")]
    [DataRow("1.3")]
    public void Should_throw_When_Version_parameter_fails_validation(string parameter)
    {
      var queryString = $"{QUERY_BASE}&service=WMS&version={parameter}&request=GETMAP&format=IMAGE/PNG&transparent=true&layers=LAYERS&crs=EPSG:4326";

      Assert.ThrowsException<ServiceException>(() => CallOnActionExecuting<ValidateTileParametersAttribute>(queryString));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("GETMAP")]
    [DataRow("getmap")]
    public void Should_pass_validation_When_Request_parameter_is_valid(string parameter)
    {
      var queryString = $"{QUERY_BASE}&service=WMS&version=1.3.0&request={parameter}&format=IMAGE/PNG&transparent=true&layers=LAYERS&crs=EPSG:4326";

      var context = CallOnActionExecuting<ValidateTileParametersAttribute>(queryString);

      Assert.IsNull(context.Result);
    }

    [TestMethod]
    [DataRow("invalid")]
    public void Should_throw_When_Request_parameter_fails_validation(string parameter)
    {
      var queryString = $"{QUERY_BASE}&service=WMS&version=1.3.0&request={parameter}&format=IMAGE/PNG&transparent=true&layers=LAYERS&crs=EPSG:4326";

      Assert.ThrowsException<ServiceException>(() => CallOnActionExecuting<ValidateTileParametersAttribute>(queryString));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("IMAGE/PNG")]
    [DataRow("image/png")]
    public void Should_pass_validation_When_Format_parameter_is_valid(string parameter)
    {
      var queryString = $"{QUERY_BASE}&service=WMS&version=1.3.0&request=GETMAP&format={parameter}&transparent=true&layers=LAYERS&crs=EPSG:4326";

      var context = CallOnActionExecuting<ValidateTileParametersAttribute>(queryString);

      Assert.IsNull(context.Result);
    }

    [TestMethod]
    [DataRow("invalid")]
    [DataRow(@"IMAGE\PNG")]
    public void Should_throw_When_Format_parameter_fails_validation(string parameter)
    {
      var queryString = $"{QUERY_BASE}&service=WMS&version=1.3.0&request=GETMAP&format={parameter}&transparent=true&layers=LAYERS&crs=EPSG:4326";

      Assert.ThrowsException<ServiceException>(() => CallOnActionExecuting<ValidateTileParametersAttribute>(queryString));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("TRUE")]
    [DataRow("true")]
    public void Should_pass_validation_When_Transparent_parameter_is_valid(string parameter)
    {
      var queryString = $"{QUERY_BASE}&service=WMS&version=1.3.0&request=GETMAP&format=IMAGE/PNG&transparent={parameter}&layers=LAYERS&crs=EPSG:4326";

      var context = CallOnActionExecuting<ValidateTileParametersAttribute>(queryString);

      Assert.IsNull(context.Result);
    }

    [TestMethod]
    [DataRow("invalid")]
    [DataRow(@"IMAGE\PNG")]
    public void Should_throw_When_Transparent_parameter_fails_validation(string parameter)
    {
      var queryString = $"{QUERY_BASE}&service=WMS&version=1.3.0&request=GETMAP&format=IMAGE/PNG&transparent={parameter}&layers=LAYERS&crs=EPSG:4326";

      Assert.ThrowsException<ServiceException>(() => CallOnActionExecuting<ValidateTileParametersAttribute>(queryString));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("LAYERS")]
    [DataRow("layers")]
    public void Should_pass_validation_When_Layers_parameter_is_valid(string parameter)
    {
      var queryString = $"{QUERY_BASE}&service=WMS&version=1.3.0&request=GETMAP&format=IMAGE/PNG&transparent=true&layers={parameter}&crs=EPSG:4326";

      var context = CallOnActionExecuting<ValidateTileParametersAttribute>(queryString);

      Assert.IsNull(context.Result);
    }

    [TestMethod]
    [DataRow("invalid")]
    public void Should_throw_When_Layers_parameter_fails_validation(string parameter)
    {
      var queryString = $"{QUERY_BASE}&service=WMS&version=1.3.0&request=GETMAP&format=IMAGE/PNG&transparent=true&layers={parameter}&crs=EPSG:4326";

      Assert.ThrowsException<ServiceException>(() => CallOnActionExecuting<ValidateTileParametersAttribute>(queryString));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("EPSG:4326")]
    [DataRow("epsg:4326")]
    public void Should_pass_validation_When_CRS_parameter_is_valid(string parameter)
    {
      var queryString = $"{QUERY_BASE}&service=WMS&version=1.3.0&request=GETMAP&format=IMAGE/PNG&transparent=true&layers=LAYERS&crs={parameter}";

      var context = CallOnActionExecuting<ValidateTileParametersAttribute>(queryString);

      Assert.IsNull(context.Result);
    }

    [TestMethod]
    [DataRow("invalid")]
    public void Should_throw_When_CRS_parameter_fails_validation(string parameter)
    {
      var queryString = $"{QUERY_BASE}&service=WMS&version=1.3.0&request=GETMAP&format=IMAGE/PNG&transparent=true&layers=LAYERS&crs={parameter}";

      Assert.ThrowsException<ServiceException>(() => CallOnActionExecuting<ValidateTileParametersAttribute>(queryString));
    }

    [TestMethod]
    public void Should_throw_When_Stylse_parameter_fails_validation()
    {
      var queryString = $"{QUERY_BASE}&service=WMS&version=1.3.0&request=GETMAP&format=IMAGE/PNG&transparent=true&layers=LAYERS&crs=EPSG:426&styles=something";

      Assert.ThrowsException<ServiceException>(() => CallOnActionExecuting<ValidateTileParametersAttribute>(queryString));
    }
  }
}