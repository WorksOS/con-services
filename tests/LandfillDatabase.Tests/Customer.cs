using System;
using System.Linq;
using Common.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LandfillDatabase.Tests
{
  [TestClass]
  public class Customer : TestBase
  {
    [TestMethod]
    public void GetAssociatedCustomerbyUserUid_Succeeds()
    {
      int legacyCustomerId;
      Guid customerUid;
      Guid userUid;
      Guid projectUid;
      Guid projectGeofenceUid;
      Guid landfillGeofenceUid;
      Guid subscriptionUid;
      var isCreatedOk = CreateAProjectWithLandfill(out legacyCustomerId,
        out customerUid, out userUid, out projectUid, out projectGeofenceUid, out landfillGeofenceUid,
        out subscriptionUid);
      Assert.IsTrue(isCreatedOk, "Failed to create a project.");

      var customers = LandfillDb.GetAssociatedCustomerbyUserUid(userUid).ToList();
      Assert.AreEqual(1, customers.Count, "Error trying to get the created customer.");
      Assert.AreEqual(customerUid.ToString(), customers[0].CustomerUID, "Failed to get the correct customer.");
    }

    [TestMethod]
    public void GetCustomer_Succeeds()
    {
      int legacyCustomerId;
      Guid customerUid;
      Guid userUid;
      Guid projectUid;
      Guid projectGeofenceUid;
      Guid landfillGeofenceUid;
      Guid subscriptionUid;
      var isCreatedOk = CreateAProjectWithLandfill(out legacyCustomerId,
        out customerUid, out userUid, out projectUid, out projectGeofenceUid, out landfillGeofenceUid,
        out subscriptionUid);
      Assert.IsTrue(isCreatedOk, "Failed to create a project.");

      var customer = LandfillDb.GetCustomer(customerUid);
      Assert.IsNotNull(customer, "Error trying to get the created customer.");
      Assert.AreEqual(customerUid.ToString(), customer.CustomerUID, "Failed to get the correct customer.");
    }
  }
}
