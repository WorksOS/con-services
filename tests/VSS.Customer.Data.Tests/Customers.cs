//using System;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using VSS.VisionLink.Interfaces.Events.MasterData.Models;

//namespace VSS.Customer.Data.Tests
//{
//  [TestClass]
//  public class Customers
//  {
//    private readonly MySqlCustomerRepository _customerService;

//     public Customers()
//    {
//      _customerService = new MySqlCustomerRepository();
//    }

//     private CreateCustomerEvent GetNewCreateCustomerEvent()
//     {
//       return new CreateCustomerEvent()
//       {
//         CustomerUID = Guid.NewGuid(),
//         CustomerName = "Test Customer",
//         CustomerType = CustomerType.Dealer.ToString(),
//         ActionUTC = DateTime.UtcNow,
//         ReceivedUTC = DateTime.UtcNow.AddMilliseconds(1000)
//       };
//     }

//     private UpdateCustomerEvent GetNewUpdateCustomerEvent(Guid customerUID, string customerName, DateTime lastActionedUTC)
//     {
//       return new UpdateCustomerEvent()
//       {
//         CustomerUID = customerUID,
//         CustomerName = customerName,
//         ActionUTC = lastActionedUTC,
//         ReceivedUTC = DateTime.UtcNow.AddMilliseconds(100)
//       };
//     }

//     private DeleteCustomerEvent GetNewDeleteCustomerEvent(Guid customerUID, DateTime lastActionedUTC)
//     {
//       return new DeleteCustomerEvent()
//       {
//         CustomerUID = customerUID,
//         ActionUTC = lastActionedUTC,
//         ReceivedUTC = DateTime.UtcNow.AddMilliseconds(100)
//       };
//     }

//    private AssociateCustomerUserEvent GetNewAssociateCustomerUserEvent(Guid customerUID)
//    {
//      return new AssociateCustomerUserEvent
//      {
//        CustomerUID = customerUID,
//        UserUID = Guid.NewGuid(),
//        ActionUTC = DateTime.UtcNow,
//        ReceivedUTC = DateTime.UtcNow.AddMilliseconds(1000)
//      };
//    }

//    private DissociateCustomerUserEvent GetNewDissociateCustomerUserEvent(Guid customerUID, Guid userUID)
//    {
//      return new DissociateCustomerUserEvent
//      {
//        CustomerUID = customerUID,
//        UserUID = userUID,
//        ActionUTC = DateTime.UtcNow,
//        ReceivedUTC = DateTime.UtcNow.AddMilliseconds(1000)
//      };
//    }
    
//    [TestMethod]
//     public void CreateCustomer_Succeeds()
//     {
//       _customerService.InRollbackTransaction<object>(o =>
//       {
//         var createCustomerEvent = GetNewCreateCustomerEvent();
//         var upsertCount = _customerService.StoreCustomer(createCustomerEvent);
//         Assert.IsTrue(upsertCount == 1, "Failed to create a customer!");

//         var customer = _customerService.GetCustomer(createCustomerEvent.CustomerUID);
//         Assert.IsNotNull(customer, "Failed to get the created customer!");

//         return null;
//       });
//     }

//    [TestMethod]
//    public void UpsertCustomer_Fails()
//    {
//      var upsertCount = _customerService.StoreCustomer(null);
//      Assert.IsTrue(upsertCount == 0, "Should fail to upsert a customer!");
//    }

//    [TestMethod]
//    public void UpdateCustomer_Succeeds()
//    {
//      _customerService.InRollbackTransaction<object>(o =>
//      {
//        var createCustomerEvent = GetNewCreateCustomerEvent();
//        var upsertCount = _customerService.StoreCustomer(createCustomerEvent);
//        Assert.IsTrue(upsertCount == 1, "Failed to create a customer!");

//        var updateCustomerEvent = GetNewUpdateCustomerEvent(createCustomerEvent.CustomerUID,
//                                                            createCustomerEvent.CustomerName,
//                                                            DateTime.UtcNow);
//        upsertCount = _customerService.StoreCustomer(updateCustomerEvent);
//        Assert.IsTrue(upsertCount == 1, "Failed to update the customer!");

//        var customer = _customerService.GetCustomer(createCustomerEvent.CustomerUID);
//        Assert.IsNotNull(customer, "Failed to get the updated customer!");

//        Assert.IsTrue(customer.CustomerUID == updateCustomerEvent.CustomerUID.ToString(), "CustomerUID should not be changed!");
//        Assert.IsTrue(customer.CustomerName == updateCustomerEvent.CustomerName, "Customer Name should not be changed!");
//        Assert.IsTrue(customer.LastActionedUTC > createCustomerEvent.ActionUTC, "LastActionedUtc of the updated customer was incorrectly updated!");

//        return null;
//      });
//    }

//    [TestMethod]
//    public void DeleteCustomer_Succeeds()
//    {
//      _customerService.InRollbackTransaction<object>(o =>
//      {
//        var createCustomerEvent = GetNewCreateCustomerEvent();
//        var upsertCount = _customerService.StoreCustomer(createCustomerEvent);
//        Assert.IsTrue(upsertCount == 1, "Failed to create a customer!");

//        var deleteCustomerEvent = GetNewDeleteCustomerEvent(createCustomerEvent.CustomerUID, DateTime.UtcNow);

//        upsertCount = _customerService.StoreCustomer(deleteCustomerEvent);
//        Assert.IsTrue(upsertCount == 1, "Failed to delete the customer!");

//        var customer = _customerService.GetCustomer(createCustomerEvent.CustomerUID);
//        Assert.IsNull(customer, "Succeeded to get the deleted customer!");

//        return null;
//      });
//    }

//    [TestMethod]
//    public void AssociateCustomerUser_Fails()
//    {
//      _customerService.InRollbackTransaction<object>(o =>
//      {
//        var associateEvent = GetNewAssociateCustomerUserEvent(Guid.NewGuid());
//        var upsertCount = _customerService.StoreCustomer(associateEvent);
//        Assert.IsTrue(upsertCount == 1, "Failed to associate customer with user!");

//        upsertCount = _customerService.StoreCustomer(associateEvent);
//        Assert.IsTrue(upsertCount == 0, "Should fail to associate customer with user");

//        return null;
//      });     
//    }

//    [TestMethod]
//    public void AssociateDissociateCustomerUser_Succeeds()
//    {
//      _customerService.InRollbackTransaction<object>(o =>
//      {
//        var createCustomerEvent = GetNewCreateCustomerEvent();
//        var upsertCount = _customerService.StoreCustomer(createCustomerEvent);
//        Assert.IsTrue(upsertCount == 1, "Failed to create a customer!");

//        var associateEvent = GetNewAssociateCustomerUserEvent(createCustomerEvent.CustomerUID);
//        upsertCount = _customerService.StoreCustomer(associateEvent);
//        Assert.IsTrue(upsertCount == 1, "Failed to associate customer with user!");

//        var customer = _customerService.GetAssociatedCustomerbyUserUid(associateEvent.UserUID);
//        Assert.IsNotNull(customer, "Failed to get the customer for the user!");

//        var dissociateEvent = GetNewDissociateCustomerUserEvent(associateEvent.CustomerUID, associateEvent.UserUID);
//        upsertCount = _customerService.StoreCustomer(dissociateEvent);
//        Assert.IsTrue(upsertCount == 1, "Failed to dissociate customer and user!");

//        customer = _customerService.GetAssociatedCustomerbyUserUid(associateEvent.UserUID);
//        Assert.IsNull(customer, "Should fail to get customer for user!");

//        return null;
//      });     
//    }



//  }
//}
