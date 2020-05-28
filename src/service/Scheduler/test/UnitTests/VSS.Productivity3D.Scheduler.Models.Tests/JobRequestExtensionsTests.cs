using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Scheduler.Models;

namespace VSS.Productivity3D.Scheduler.Jobs.Tests
{
  [TestClass]
  public class JobRequestExtensionsTests
  {
    private readonly ScheduleJobRequest _scheduleJobRequest;
    private readonly string _serializedRequest;

    public JobRequestExtensionsTests()
    {
      _scheduleJobRequest = new ScheduleJobRequest();
      _scheduleJobRequest.Headers = new HeaderDictionary
      {
        { "custom-header", "some value" },
        { "Content-Type", "application/json" },
        { "Cache-Control", "none" }
      };
      _scheduleJobRequest.Url = "url";
      _scheduleJobRequest.Filename = "filename";

      _serializedRequest = JsonConvert.SerializeObject(_scheduleJobRequest);
    }

    [TestMethod]
    public void GetConvertedObject_Should_throw_When_receiver_type_isnt_supported()
    {
      var exObj = Assert.ThrowsException<ServiceException>(() => _serializedRequest.GetConvertedObject<HeaderDictionary>());
    }

    [TestMethod]
    public void GetConvertedObject_Should_convert_When_receiver_is_JObject()
    {
      var jObject = JsonConvert.DeserializeObject<JObject>(_serializedRequest);

      var result = jObject.GetConvertedObject<JObject>();

      Assert.IsTrue(result.HasValues);
    }

    [TestMethod]
    public void GetConvertedObject_Should_deserialize_HeaderDictionary_type()
    {
      var jObject = JsonConvert.DeserializeObject<JObject>(_serializedRequest);
      var jToken = jObject.GetValue("headers");

      Assert.IsNotNull(jToken);

      var result = jToken.GetConvertedObject<HeaderDictionary>();

      foreach (var header in _scheduleJobRequest.Headers)
      {
        var customHeader = result.TryGetValue(header.Key, out var value);

        Assert.IsTrue(customHeader);
        Assert.AreEqual(header.Value, value[0]);
      }
    }
  }
}
