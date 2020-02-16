using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Metadata.Providers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ReferenceIdentifierService.Helpers;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Constants;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;

namespace VSS.Nighthawk.ReferenceIdentifierService.Tests
{
  [TestClass]
  public class IdentifierDefinitionModelBinderTests
  {
    [TestMethod]
    public void BindModel_AllFieldsAreValid()
    {
      var actionContext = new HttpActionContext(); 
      var guid = new UUIDSequentialGuid().CreateGuid();
      IEnumerable<KeyValuePair<string, string>> query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("storeid", "1"), new KeyValuePair<string, string>("alias", "abc"), new KeyValuePair<string, string>("value", "xyz"), new KeyValuePair<string, string>("uid", guid.ToString()) };
      IValueProvider valueProvider = new NameValuePairsValueProvider(query, new CultureInfo(1));

      var bindingContext = new ModelBindingContext
      {
        ModelName = "foo",
        ValueProvider = valueProvider,
        ModelMetadata =
          new ModelMetadata(new EmptyModelMetadataProvider(), null, null, typeof (IdentifierDefinition),
            "IdentifierDefinition")
      };
      var binder = new IdentifierDefinitionModelBinder();
      var value = binder.BindModel(actionContext, bindingContext);
      Assert.IsTrue(value);
      Assert.AreEqual(1, ((IdentifierDefinition)bindingContext.Model).StoreId);
      Assert.AreEqual("abc", ((IdentifierDefinition)bindingContext.Model).Alias);
      Assert.AreEqual("xyz", ((IdentifierDefinition)bindingContext.Model).Value);
      Assert.AreEqual(guid, ((IdentifierDefinition)bindingContext.Model).UID);
    }

    [TestMethod]
    public void BindModel_MissingStoreId()
    {
      var actionContext = new HttpActionContext(); 
      var guid = new UUIDSequentialGuid().CreateGuid();
      IEnumerable<KeyValuePair<string, string>> query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("alias", "abc"), new KeyValuePair<string, string>("value", "xyz"), new KeyValuePair<string, string>("uid", guid.ToString()) };
      IValueProvider valueProvider = new NameValuePairsValueProvider(query, new CultureInfo(1));

      var bindingContext = new ModelBindingContext
      {
        ModelName = "foo",
        ValueProvider = valueProvider,
        ModelMetadata =
          new ModelMetadata(new EmptyModelMetadataProvider(), null, null, typeof (IdentifierDefinition),
            "IdentifierDefinition")
      };
      var binder = new IdentifierDefinitionModelBinder();
      var value = binder.BindModel(actionContext, bindingContext);
      Assert.IsFalse(value);
      Assert.IsTrue(actionContext.ModelState.ContainsKey(ModelBinderConstants.IdentifierDefinitionModelBinderError));
      Assert.AreEqual("Invalid request: query string does not have valid storeId", actionContext.ModelState[ModelBinderConstants.IdentifierDefinitionModelBinderError].Errors[0].ErrorMessage);
    }

    [TestMethod]
    public void BindModel_MissingAlias()
    {
      var actionContext = new HttpActionContext();
      var guid = new UUIDSequentialGuid().CreateGuid();
      IEnumerable<KeyValuePair<string, string>> query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("storeid", "1"), new KeyValuePair<string, string>("value", "xyz"), new KeyValuePair<string, string>("uid", guid.ToString()) };
      IValueProvider valueProvider = new NameValuePairsValueProvider(query, new CultureInfo(1));

      var bindingContext = new ModelBindingContext
      {
        ModelName = "foo",
        ValueProvider = valueProvider,
        ModelMetadata =
          new ModelMetadata(new EmptyModelMetadataProvider(), null, null, typeof (IdentifierDefinition),
            "IdentifierDefinition")
      };
      var binder = new IdentifierDefinitionModelBinder();
      var value = binder.BindModel(actionContext, bindingContext);
      Assert.IsFalse(value);
      Assert.IsTrue(actionContext.ModelState.ContainsKey(ModelBinderConstants.IdentifierDefinitionModelBinderError));
      Assert.AreEqual("Invalid request: query string does not have valid alias", actionContext.ModelState[ModelBinderConstants.IdentifierDefinitionModelBinderError].Errors[0].ErrorMessage);
    }

    [TestMethod]
    public void BindModel_MissingValue()
    {
      var actionContext = new HttpActionContext();
      var guid = new UUIDSequentialGuid().CreateGuid();
      IEnumerable<KeyValuePair<string, string>> query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("storeid", "1"), new KeyValuePair<string, string>("alias", "abc"), new KeyValuePair<string, string>("uid", guid.ToString()) };
      IValueProvider valueProvider = new NameValuePairsValueProvider(query, new CultureInfo(1));

      var bindingContext = new ModelBindingContext
      {
        ModelName = "foo",
        ValueProvider = valueProvider,
        ModelMetadata =
          new ModelMetadata(new EmptyModelMetadataProvider(), null, null, typeof (IdentifierDefinition),
            "IdentifierDefinition")
      };
      var binder = new IdentifierDefinitionModelBinder();
      var value = binder.BindModel(actionContext, bindingContext);
      Assert.IsFalse(value);
      Assert.IsTrue(actionContext.ModelState.ContainsKey(ModelBinderConstants.IdentifierDefinitionModelBinderError));
      Assert.AreEqual("Invalid request: query string does not have valid value", actionContext.ModelState[ModelBinderConstants.IdentifierDefinitionModelBinderError].Errors[0].ErrorMessage);
    }

    [TestMethod]
    public void BindModel_MissingUid()
    {
      var actionContext = new HttpActionContext();
      IEnumerable<KeyValuePair<string, string>> query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("storeid", "1"), new KeyValuePair<string, string>("alias", "abc"), new KeyValuePair<string, string>("value", "xyz") };
      IValueProvider valueProvider = new NameValuePairsValueProvider(query, new CultureInfo(1));

      var bindingContext = new ModelBindingContext
      {
        ModelName = "foo",
        ValueProvider = valueProvider,
        ModelMetadata =
          new ModelMetadata(new EmptyModelMetadataProvider(), null, null, typeof (IdentifierDefinition),
            "IdentifierDefinition")
      };
      var binder = new IdentifierDefinitionModelBinder();
      var value = binder.BindModel(actionContext, bindingContext);
      Assert.IsTrue(value);
      Assert.AreEqual(1, ((IdentifierDefinition)bindingContext.Model).StoreId);
      Assert.AreEqual("abc", ((IdentifierDefinition)bindingContext.Model).Alias);
      Assert.AreEqual("xyz", ((IdentifierDefinition)bindingContext.Model).Value);
    }

    [TestMethod]
    public void BindModel_BadStoreId()
    {
      var actionContext = new HttpActionContext();
      var guid = new UUIDSequentialGuid().CreateGuid();
      IEnumerable<KeyValuePair<string, string>> query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("storeid", "not_a_number"), new KeyValuePair<string, string>("alias", "abc"), new KeyValuePair<string, string>("value", "xyz"), new KeyValuePair<string, string>("uid", guid.ToString()) };
      IValueProvider valueProvider = new NameValuePairsValueProvider(query, new CultureInfo(1));

      var bindingContext = new ModelBindingContext
      {
        ModelName = "foo",
        ValueProvider = valueProvider,
        ModelMetadata =
          new ModelMetadata(new EmptyModelMetadataProvider(), null, null, typeof(IdentifierDefinition),
            "IdentifierDefinition")
      };
      var binder = new IdentifierDefinitionModelBinder();
      var value = binder.BindModel(actionContext, bindingContext);
      Assert.IsFalse(value);
      Assert.IsTrue(actionContext.ModelState.ContainsKey(ModelBinderConstants.IdentifierDefinitionModelBinderError));
      Assert.AreEqual("Invalid request: query string does not have valid storeId", actionContext.ModelState[ModelBinderConstants.IdentifierDefinitionModelBinderError].Errors[0].ErrorMessage);
    }

    [TestMethod]
    public void BindModel_BadUid()
    {
      var actionContext = new HttpActionContext();
      IEnumerable<KeyValuePair<string, string>> query = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("storeid", "1"), new KeyValuePair<string, string>("alias", "abc"), new KeyValuePair<string, string>("value", "xyz"), new KeyValuePair<string, string>("uid", "bad_uid") };
      IValueProvider valueProvider = new NameValuePairsValueProvider(query, new CultureInfo(1));

      var bindingContext = new ModelBindingContext
      {
        ModelName = "foo",
        ValueProvider = valueProvider,
        ModelMetadata =
          new ModelMetadata(new EmptyModelMetadataProvider(), null, null, typeof(IdentifierDefinition),
            "IdentifierDefinition")
      };
      var binder = new IdentifierDefinitionModelBinder();
      var value = binder.BindModel(actionContext, bindingContext);
      Assert.IsFalse(value);
      Assert.IsTrue(actionContext.ModelState.ContainsKey(ModelBinderConstants.IdentifierDefinitionModelBinderError));
      Assert.AreEqual("Invalid request: query string does not have valid uid", actionContext.ModelState[ModelBinderConstants.IdentifierDefinitionModelBinderError].Errors[0].ErrorMessage);
    }
  }
}
