using System.Collections.Generic;
using System.Globalization;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.ValueProviders.Providers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.DeviceCapabilityService.Helpers;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;
using IValueProvider = System.Web.Http.ValueProviders.IValueProvider;
using ModelBindingContext = System.Web.Http.ModelBinding.ModelBindingContext;
using ED=VSS.Nighthawk.ExternalDataTypes.Enumerations;
namespace VSS.Nighthawk.DeviceCapabilityService.Tests
{
  [TestClass]
  public class DeviceQueryModelBinderTests
  {
    [TestMethod]
    public void BindModel_ValidIDOnlyTest()
    {
      IEnumerable<KeyValuePair<string, string>> query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("id", "127") };
      IValueProvider valueProvider = new NameValuePairsValueProvider(query, new CultureInfo(1));
      
      var bindingContext = new ModelBindingContext() { ModelName = "foo", ValueProvider = valueProvider };
      bindingContext.ModelMetadata = new ModelMetadata(new EmptyModelMetadataProvider(), null, null, typeof(IDeviceQuery), "IDeviceQuery");
      DeviceQueryModelBinder binder = new DeviceQueryModelBinder();
      var value = binder.BindModel(null, bindingContext);
      Assert.IsTrue(value);
      Assert.IsNull(((IDeviceQuery)bindingContext.Model).AssetID);
      Assert.AreEqual(127, ((IDeviceQuery)bindingContext.Model).ID);
      Assert.AreEqual(default(string), ((IDeviceQuery)bindingContext.Model).GPSDeviceID);
      Assert.IsNull(((IDeviceQuery)bindingContext.Model).DeviceType);
    }

    [TestMethod]
    public void BindModel_ValidGpsDeviceIDDeviceTypeTest()
    {
      IEnumerable<KeyValuePair<string, string>> query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("gpsdeviceid", "127"), new KeyValuePair<string, string>("devicetype", "PL121") };
      IValueProvider valueProvider = new NameValuePairsValueProvider(query, new CultureInfo(1));

      var bindingContext = new ModelBindingContext() { ModelName = "foo", ValueProvider = valueProvider };
      bindingContext.ModelMetadata = new ModelMetadata(new EmptyModelMetadataProvider(), null, null, typeof(IDeviceQuery), "IDeviceQuery");
      DeviceQueryModelBinder binder = new DeviceQueryModelBinder();
      var value = binder.BindModel(null, bindingContext);
      Assert.IsTrue(value);
      Assert.IsNull(((IDeviceQuery)bindingContext.Model).AssetID);
      Assert.IsNull(((IDeviceQuery)bindingContext.Model).ID);
      Assert.AreEqual("127", ((IDeviceQuery)bindingContext.Model).GPSDeviceID);
      Assert.AreEqual(ED.DeviceTypeEnum.PL121, ((IDeviceQuery)bindingContext.Model).DeviceType);
    }

    [TestMethod]
    public void BindModel_ValidIDGpsDeviceIDDeviceTypeTest()
    {
      IEnumerable<KeyValuePair<string, string>> query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("id", "111"), new KeyValuePair<string, string>("gpsdeviceid", "127"), new KeyValuePair<string, string>("devicetype", "PL121") };
      IValueProvider valueProvider = new NameValuePairsValueProvider(query, new CultureInfo(1));

      var bindingContext = new ModelBindingContext() { ModelName = "foo", ValueProvider = valueProvider };
      bindingContext.ModelMetadata = new ModelMetadata(new EmptyModelMetadataProvider(), null, null, typeof(IDeviceQuery), "IDeviceQuery");
      DeviceQueryModelBinder binder = new DeviceQueryModelBinder();
      var value = binder.BindModel(null, bindingContext);
      Assert.IsTrue(value);
      Assert.IsNull(((IDeviceQuery)bindingContext.Model).AssetID);
      Assert.AreEqual(111, ((IDeviceQuery)bindingContext.Model).ID);
      Assert.AreEqual("127", ((IDeviceQuery)bindingContext.Model).GPSDeviceID);
      Assert.AreEqual(ED.DeviceTypeEnum.PL121, ((IDeviceQuery)bindingContext.Model).DeviceType);
    }

    [TestMethod]
    public void BindModel_ValidAssetIDOnlyTest()
    {
      IEnumerable<KeyValuePair<string, string>> query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("assetid", "127") };
      IValueProvider valueProvider = new NameValuePairsValueProvider(query, new CultureInfo(1));

      var bindingContext = new ModelBindingContext() { ModelName = "foo", ValueProvider = valueProvider };
      bindingContext.ModelMetadata = new ModelMetadata(new EmptyModelMetadataProvider(), null, null, typeof(IDeviceQuery), "IDeviceQuery");
      DeviceQueryModelBinder binder = new DeviceQueryModelBinder();
      var value = binder.BindModel(null, bindingContext);
      Assert.IsTrue(value);
      Assert.AreEqual(127, ((IDeviceQuery)bindingContext.Model).AssetID);
      Assert.IsNull(((IDeviceQuery)bindingContext.Model).ID);
      Assert.AreEqual(default(string), ((IDeviceQuery)bindingContext.Model).GPSDeviceID);
      Assert.IsNull(((IDeviceQuery)bindingContext.Model).DeviceType);
    }

    [TestMethod]
    public void BindModel_ValidID_InvalidGpsDeviceID_ValidDeviceTypeTest()
    {
      IEnumerable<KeyValuePair<string, string>> query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("id", "111"), new KeyValuePair<string, string>("gpsdeviceid", string.Empty), new KeyValuePair<string, string>("devicetype", "PL121") };
      IValueProvider valueProvider = new NameValuePairsValueProvider(query, new CultureInfo(1));

      var bindingContext = new ModelBindingContext() { ModelName = "foo", ValueProvider = valueProvider };
      bindingContext.ModelMetadata = new ModelMetadata(new EmptyModelMetadataProvider(), null, null, typeof(IDeviceQuery), "IDeviceQuery");
      DeviceQueryModelBinder binder = new DeviceQueryModelBinder();
      var value = binder.BindModel(null, bindingContext);
      Assert.IsTrue(value);
      Assert.IsNull(((IDeviceQuery)bindingContext.Model).AssetID);
      Assert.AreEqual(111, ((IDeviceQuery)bindingContext.Model).ID);
      Assert.IsNull(((IDeviceQuery)bindingContext.Model).GPSDeviceID);
      Assert.AreEqual(ED.DeviceTypeEnum.PL121, ((IDeviceQuery)bindingContext.Model).DeviceType);
    }

    [TestMethod]
    public void BindModel_InValidID_ValidGpsDeviceID_ValidDeviceTypeTest()
    {
      IEnumerable<KeyValuePair<string, string>> query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("id", "111"), new KeyValuePair<string, string>("gpsdeviceid", "Test"), new KeyValuePair<string, string>("devicetype", "PL121") };
      IValueProvider valueProvider = new NameValuePairsValueProvider(query, new CultureInfo(1));

      var bindingContext = new ModelBindingContext() { ModelName = "foo", ValueProvider = valueProvider };
      bindingContext.ModelMetadata = new ModelMetadata(new EmptyModelMetadataProvider(), null, null, typeof(IDeviceQuery), "IDeviceQuery");
      DeviceQueryModelBinder binder = new DeviceQueryModelBinder();
      var value = binder.BindModel(null, bindingContext);
      Assert.IsTrue(value);
      Assert.IsNull(((IDeviceQuery)bindingContext.Model).AssetID);
      Assert.AreEqual(111, ((IDeviceQuery)bindingContext.Model).ID);
      Assert.AreEqual("Test", ((IDeviceQuery)bindingContext.Model).GPSDeviceID);
      Assert.AreEqual(ED.DeviceTypeEnum.PL121, ((IDeviceQuery)bindingContext.Model).DeviceType);
    }

    [TestMethod]
    public void BindModel_ValidID_ValidGpsDeviceID_InValidDeviceTypeTest()
    {
      IEnumerable<KeyValuePair<string, string>> query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("id", "111"), new KeyValuePair<string, string>("gpsdeviceid", "Test"), new KeyValuePair<string, string>("devicetype", "blah") };
      IValueProvider valueProvider = new NameValuePairsValueProvider(query, new CultureInfo(1));

      var bindingContext = new ModelBindingContext() { ModelName = "foo", ValueProvider = valueProvider };
      bindingContext.ModelMetadata = new ModelMetadata(new EmptyModelMetadataProvider(), null, null, typeof(IDeviceQuery), "IDeviceQuery");
      DeviceQueryModelBinder binder = new DeviceQueryModelBinder();
      var value = binder.BindModel(null, bindingContext);
      Assert.IsTrue(value);
      Assert.IsNull(((IDeviceQuery)bindingContext.Model).AssetID);
      Assert.AreEqual(111, ((IDeviceQuery)bindingContext.Model).ID);
      Assert.AreEqual("Test", ((IDeviceQuery)bindingContext.Model).GPSDeviceID);
      Assert.IsNull(((IDeviceQuery)bindingContext.Model).DeviceType);
    }

    [TestMethod]
    public void BindModel_AllInvalidTest()
    {
      IEnumerable<KeyValuePair<string, string>> query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("assetid", "xyz"), new KeyValuePair<string, string>("id", "abc"), new KeyValuePair<string, string>("gpsdeviceid", string.Empty), new KeyValuePair<string, string>("devicetype", "blah") };
      IValueProvider valueProvider = new NameValuePairsValueProvider(query, new CultureInfo(1));

      var bindingContext = new ModelBindingContext() { ModelName = "foo", ValueProvider = valueProvider };
      var actionContext = new HttpActionContext();
      bindingContext.ModelMetadata = new ModelMetadata(new EmptyModelMetadataProvider(), null, null, typeof(IDeviceQuery), "IDeviceQuery");
      DeviceQueryModelBinder binder = new DeviceQueryModelBinder();

      var value = binder.BindModel(actionContext, bindingContext);
      Assert.IsTrue(actionContext.ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError));
      Assert.AreEqual(actionContext.ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage, "Invalid request: query string does not have valid deviceID, deviceType, or assetID");
    }

    [TestMethod]
    public void BindModel_NoGpsDeviceIDTest()
    {
      IEnumerable<KeyValuePair<string, string>> query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("devicetype", "PL321") };
      IValueProvider valueProvider = new NameValuePairsValueProvider(query, new CultureInfo(1));

      var bindingContext = new ModelBindingContext() { ModelName = "foo", ValueProvider = valueProvider };
      var actionContext = new HttpActionContext();
      bindingContext.ModelMetadata = new ModelMetadata(new EmptyModelMetadataProvider(), null, null, typeof(IDeviceQuery), "IDeviceQuery");
      DeviceQueryModelBinder binder = new DeviceQueryModelBinder();

      var value = binder.BindModel(actionContext, bindingContext);
      Assert.IsTrue(actionContext.ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError));
      Assert.AreEqual(actionContext.ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage, "Invalid request: query string does not have valid gpsdeviceID and deviceType");
    }

    [TestMethod]
    public void BindModel_NoDeviceTypeIDTest()
    {
      IEnumerable<KeyValuePair<string, string>> query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("gpsdeviceid", "PL321") };
      IValueProvider valueProvider = new NameValuePairsValueProvider(query, new CultureInfo(1));

      var bindingContext = new ModelBindingContext() { ModelName = "foo", ValueProvider = valueProvider };
      var actionContext = new HttpActionContext();
      bindingContext.ModelMetadata = new ModelMetadata(new EmptyModelMetadataProvider(), null, null, typeof(IDeviceQuery), "IDeviceQuery");
      DeviceQueryModelBinder binder = new DeviceQueryModelBinder();

      var value = binder.BindModel(actionContext, bindingContext);
      Assert.IsTrue(actionContext.ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError));
      Assert.AreEqual(actionContext.ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage, "Invalid request: query string does not have valid gpsdeviceID and deviceType");
    }
  }
}
