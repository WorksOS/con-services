using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.Hosted.VLCommon.Bss;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class ActivityResultTests : BssUnitTestBase
  {
    [TestMethod]
    public void ActivityResult_Defaults()
    {
      var result = new ActivityResult();

      Assert.AreNotEqual(DateTime.MinValue, result.DateTimeUtc);
      Assert.AreEqual(ResultType.Information, result.Type);
    }

    [TestMethod]
    public void DebugResult_Defaults()
    {
      var result = new DebugResult();

      Assert.AreNotEqual(DateTime.MinValue, result.DateTimeUtc);
      Assert.AreEqual(ResultType.Debug, result.Type);
    }

    [TestMethod]
    public void WarningResult_Defaults()
    {
      var result = new WarningResult();

      Assert.AreNotEqual(DateTime.MinValue, result.DateTimeUtc);
      Assert.AreEqual(ResultType.Warning, result.Type);
    }

    [TestMethod]
    public void ErrorResult_Defaults()
    {
      var result = new ErrorResult();

      Assert.AreNotEqual(DateTime.MinValue, result.DateTimeUtc);
      Assert.AreEqual(ResultType.Error, result.Type);
    }

    [TestMethod]
    public void ExceptionResult_Defaults()
    {
      var exception = new Exception("Exception Message");
      var result = new ExceptionResult {Exception = exception};

      Assert.AreEqual(exception.Message, result.Summary);
      Assert.AreEqual(result.Exception.Message, result.Summary);
      Assert.AreNotEqual(DateTime.MinValue, result.DateTimeUtc);
      Assert.AreEqual(ResultType.Exception, result.Type);
    }

    [TestMethod]
    public void NotifyResult_Defaults()
    {
      var exception = new Exception("Exception Message");
      var result = new NotifyResult { Exception = exception };

      Assert.AreEqual(exception.Message, result.Summary);
      Assert.AreEqual(result.Exception.Message, result.Summary);
      Assert.AreNotEqual(DateTime.MinValue, result.DateTimeUtc);
      Assert.AreEqual(ResultType.Notify, result.Type);
    }
  }
}
