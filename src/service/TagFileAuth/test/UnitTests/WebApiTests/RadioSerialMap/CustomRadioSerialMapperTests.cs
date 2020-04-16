using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.RadioSerialMap;
using WebApiTests.Executors;

namespace WebApiTests.RadioSerialMap
{
  [TestClass]
  public class CustomRadioSerialMapTests : ExecutorBaseTests
  {
    /* Text of test json file
{ 
"map" : [{
    "note": "A test element",
    "radioSerial": "123",
    "radioType" : "torch",
    "assetId": "123",
    "assetUid": "B00C62B3-4EEE-472E-9814-C31379E94BD5",
    "projectId": "234",
    "projectUid": "896C7A36-E079-4B67-A79C-B209398F01CA"
}]
}     */

    private string testRadioSerial = "123";
    private int testRadioType = 6; // SNM940/torch;
    private long testAssetId = 123;
    private Guid testAssetUid = new Guid("B00C62B3-4EEE-472E-9814-C31379E94BD5");
    private long testProjectId = 234;
    private Guid testProjectUid = new Guid("896C7A36-E079-4B67-A79C-B209398F01CA");

    [TestMethod]
    public void Creation()
    {
      var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

      var mapper = new CustomRadioSerialProjectMap(loggerFactory);
      Assert.IsTrue(mapper != null);
    }

    [TestMethod]
    public void LocateAsset_Success()
    {
      var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

      var mapper = new CustomRadioSerialProjectMap(loggerFactory);

      var locateResult = mapper.LocateAsset(testRadioSerial, testRadioType, out var id);

      Assert.IsTrue(locateResult);
      Assert.IsTrue(id.assetId == testAssetId);
      Assert.IsTrue(id.assetUid.CompareTo(testAssetUid) == 0);
      Assert.IsTrue(id.projectId == testProjectId);
      Assert.IsTrue(id.projectUid.CompareTo(testProjectUid) == 0);
    }

    [TestMethod]
    public void LocateAsset_Failure_BadRadioSerial()
    {
      var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

      var mapper = new CustomRadioSerialProjectMap(loggerFactory);

      var locateResult = mapper.LocateAsset("xyz", testRadioType, out var id);

      Assert.IsFalse(locateResult);
    }

    [TestMethod]
    public void LocateAsset_Failure_BadRadioType()
    {
      var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

      var mapper = new CustomRadioSerialProjectMap(loggerFactory);

      var locateResult = mapper.LocateAsset(testRadioSerial, -1, out var id);

      Assert.IsFalse(locateResult);
    }
  }
}
