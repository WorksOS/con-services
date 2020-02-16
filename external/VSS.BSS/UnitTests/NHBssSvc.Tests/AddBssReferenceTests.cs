using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.NHBssSvc;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Lookups;

namespace NHBssSvc.Tests
{
  [TestClass]
  public class AddBssReferenceTests
  {
    [TestMethod]
    public void AddAssetReference_NewReference_Success()
    {
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();
      const long storeId = (long)StoreEnum.CAT;
      const string alias = "MakeCode_SN";
      const string value = "CAT_5YW00051";
      var uid = Guid.NewGuid();
      mockAssetLookup.Setup(o => o.Get(storeId, alias, value)).Returns((Guid?)null);
      var addBssReference = new BssReference(mockAssetLookup.Object, mockCustomerLookup.Object);
      addBssReference.AddAssetReference(storeId, alias, value, uid);
      mockAssetLookup.Verify(o => o.Add(storeId, alias, value, uid), Times.Once());
    }

    [TestMethod]
    public void AddAssetReference_ReferenceExists_Success()
    {
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();
      const long storeId = (long)StoreEnum.CAT;
      const string alias = "MakeCode_SN";
      const string value = "CAT_5YW00051";
      var existingUid = Guid.NewGuid();
      var uid = Guid.NewGuid();
      mockAssetLookup.Setup(o => o.Get(storeId, alias, value)).Returns(existingUid);
      var addBssReference = new BssReference(mockAssetLookup.Object, mockCustomerLookup.Object);
      addBssReference.AddAssetReference(storeId, alias, value, uid);
      mockAssetLookup.Verify(o => o.Add(storeId, alias, value, uid), Times.Never());
    }

    [TestMethod]
    public void AddCustomerReference_NewReference_Success()
    {
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();
      const long storeId = (long)StoreEnum.CAT;
      const string alias = "MakeCode_SN";
      const string value = "CAT_5YW00051";
      var uid = Guid.NewGuid();
      mockAssetLookup.Setup(o => o.Get(storeId, alias, value)).Returns((Guid?)null);
      var addBssReference = new BssReference(mockAssetLookup.Object, mockCustomerLookup.Object);
      addBssReference.AddCustomerReference(storeId, alias, value, uid);
      mockCustomerLookup.Verify(o => o.Add(storeId, alias, value, uid), Times.Once());
    }

    [TestMethod]
    public void AddCustomerReference_ReferenceExists_Success()
    {
      var mockAssetLookup = new Mock<IAssetLookup>();
      var mockCustomerLookup = new Mock<ICustomerLookup>();
      const long storeId = (long)StoreEnum.CAT;
      const string alias = "MakeCode_SN";
      const string value = "CAT_5YW00051";
      var existingUid = Guid.NewGuid();
      var uid = Guid.NewGuid();
      mockCustomerLookup.Setup(o => o.Get(storeId, alias, value)).Returns(existingUid);
      var addBssReference = new BssReference(mockAssetLookup.Object, mockCustomerLookup.Object);
      addBssReference.AddCustomerReference(storeId, alias, value, uid);
      mockCustomerLookup.Verify(o => o.Add(storeId, alias, value, uid), Times.Never());
    }
  }
}
