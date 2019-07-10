using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.WebApi.Models.Report.Executors;

namespace VSS.Productivity3D.WebApiTests.Report.Executors
{
  [TestClass]
  public class ExportReportExecutorTests
  {
    [TestMethod]
    public void ExportReportExecutor_Should_throw_When_T_conversion_to_ExportReport_fails()
    {
      Assert.ThrowsExceptionAsync<ServiceException>(async () => await new ExportReportExecutor().ProcessAsync(new { Id = 1 }));
    }
  }
}
