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
    private string testRadioSerial = "123";
    private int testRadioType = 6; // SNM940/torch;
    private long testAssetId = 123;
    private Guid testAssetUid = new Guid("b00c62b3-4eee-472e-9814-c31379e94bd5");
    private long testProjectId = 234;
    private Guid testProjectUid = new Guid("896c7a36-e079-4b67-a79c-b209398f01ca");

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
      Assert.IsTrue(id.AssetId == testAssetId);
      Assert.IsTrue(id.AssetUid.CompareTo(testAssetUid) == 0);
      Assert.IsTrue(id.ProjectId == testProjectId);
      Assert.IsTrue(id.ProjectUid.CompareTo(testProjectUid) == 0);
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
