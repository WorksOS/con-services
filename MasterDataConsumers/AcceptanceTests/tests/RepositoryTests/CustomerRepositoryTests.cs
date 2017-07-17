using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RepositoryTests.Internal;
using System;
using VSS.ConfigurationStore;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace RepositoryTests
{
  [TestClass]
  public class CustomerRepositoryTests : TestControllerBase
  {
    CustomerRepository customerContext;

    [TestInitialize]
    public void Init()
    {
      SetupLogging();

      customerContext = new CustomerRepository(ServiceProvider.GetService<IConfigurationStore>(), ServiceProvider.GetService<ILoggerFactory>());
    }

    #region Customers
    
    /// <summary>
    /// Create Customer - Happy path i.e. 
    ///   customer doesn't exist
    /// </summary>
    [TestMethod]
    public void CreateCustomer_HappyPath()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };

      var s = customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      Customer customer = CopyModel(createCustomerEvent);
      var g = customerContext.GetCustomer(createCustomerEvent.CustomerUID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Customer from CustomerRepo");
      Assert.AreEqual(customer, g.Result, "Customer details are incorrect from CustomerRepo");
    }

    /// <summary>
    /// Create Customer - customer already exists same date
    ///   should ignore
    /// </summary>
    [TestMethod]
    public void CreateCustomer_AlreadyExistsSameActionUTC()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };

      customerContext.StoreEvent(createCustomerEvent).Wait();
      customerContext.StoreEvent(createCustomerEvent).Wait();

      Customer customer = CopyModel(createCustomerEvent);
      var g = customerContext.GetCustomer(createCustomerEvent.CustomerUID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Customer from CustomerRepo");
      Assert.AreEqual(customer, g.Result, "Customer details are incorrect from CustomerRepo");
    }

    /// <summary>
    /// Create Customer - customer already exists earlier date
    ///    should ignore
    /// </summary>
    [TestMethod]
    public void CreateCustomer_AlreadyExistsEarlierActionUTC()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };
      
      var s = customerContext.StoreEvent(createCustomerEvent);
      createCustomerEvent.ActionUTC = ActionUTC.AddDays(1);
      s = customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "Customer event not written");

      Customer customer = CopyModel(createCustomerEvent);
      customer.LastActionedUTC = ActionUTC;
      var g = customerContext.GetCustomer(createCustomerEvent.CustomerUID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Customer from CustomerRepo");
      Assert.AreEqual(customer, g.Result, "Customer details are incorrect from CustomerRepo");
    }


    /// <summary>
    /// Create Customer - customer already exists later date
    ///    customer must have been created with a later UpdateCustomer
    ///    write only columns not included in an UpdateCustomer
    /// </summary>
    [TestMethod]
    public void CreateCustomer_AlreadyExistsLaterActionUTC()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Dealer.ToString(),
        ActionUTC = ActionUTC
      };

      var updateCustomerEvent = new UpdateCustomerEvent
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        CustomerName = "The Customer Name Updated",
        ActionUTC = ActionUTC.AddHours(1)
      };
      
      var s = customerContext.StoreEvent(updateCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Update Customer event not written");

      s = customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Create Customer event not written");

      Customer customer = CopyModel(createCustomerEvent);
      customer.Name = updateCustomerEvent.CustomerName;
      customer.LastActionedUTC = updateCustomerEvent.ActionUTC;
      var g = customerContext.GetCustomer(createCustomerEvent.CustomerUID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Customer from CustomerRepo");
      Assert.AreEqual(customer, g.Result, "Customer details are incorrect from CustomerRepo");
    }

    /// <summary>
    /// Update Customer - Happy path
    ///    exists, and update actionUTC is later
    /// </summary>
    [TestMethod]
    public void UpdateCustomerEvent_HappyPath()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };

      var updateCustomerEvent = new UpdateCustomerEvent
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        CustomerName = "The Customer Name Updated",
        ActionUTC = ActionUTC.AddMinutes(2)
      };
      
      customerContext.StoreEvent(createCustomerEvent).Wait();
      var s = customerContext.StoreEvent(updateCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      Customer customer = CopyModel(createCustomerEvent);
      customer.Name = updateCustomerEvent.CustomerName;
      customer.LastActionedUTC = updateCustomerEvent.ActionUTC;
      var g = customerContext.GetCustomer(createCustomerEvent.CustomerUID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Customer from CustomerRepo");
      Assert.AreEqual(customer, g.Result, "Customer details are incorrect from CustomerRepo");
    }

    /// <summary>
    /// Update Customer - customer has been delted
    ///    should idnore it
    /// </summary>
    [TestMethod]
    public void UpdateCustomerEvent_CustomerDeleted()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };

      var updateCustomerEvent = new UpdateCustomerEvent
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        CustomerName = "The Customer Name Updated",
        ActionUTC = ActionUTC.AddMinutes(2)
      };

      var deleteCustomerEvent = new DeleteCustomerEvent
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ActionUTC = ActionUTC.AddHours(1)
      };
      
      customerContext.StoreEvent(createCustomerEvent).Wait();
      customerContext.StoreEvent(deleteCustomerEvent).Wait();
      var s = customerContext.StoreEvent(updateCustomerEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "Customer event should not be updated");

      customerContext.StoreEvent(deleteCustomerEvent).Wait();

      Customer customer = CopyModel(createCustomerEvent);
      customer.IsDeleted = true;
      customer.LastActionedUTC = deleteCustomerEvent.ActionUTC;
      var g = customerContext.GetCustomer(createCustomerEvent.CustomerUID);
      g.Wait();
      Assert.IsNull(g.Result, "should not retrieve delted Customer from CustomerRepo");

      g = customerContext.GetCustomer_UnitTest(createCustomerEvent.CustomerUID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Customer from CustomerRepo");
      Assert.AreEqual(customer, g.Result, "Customer details are incorrect from CustomerRepo");
    }

    /// <summary>
    /// Update Customer - customer doesn't exist
    ///    should create with what we have.
    /// </summary>
    [TestMethod]
    public void UpdateCustomerEvent_CustomerDoesntExist()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUID = Guid.NewGuid();

      var updateCustomerEvent = new UpdateCustomerEvent
      {
        CustomerUID = customerUID,
        CustomerName = "The Customer Name GotIt!",
        ActionUTC = ActionUTC
      };
      
      var s = customerContext.StoreEvent(updateCustomerEvent);
      s = customerContext.StoreEvent(updateCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Update Customer event not written");

      var partialWrittenCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = updateCustomerEvent.CustomerUID,
        CustomerName = updateCustomerEvent.CustomerName,
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = updateCustomerEvent.ActionUTC
      };

      Customer customer = CopyModel(partialWrittenCustomerEvent);
      var g = customerContext.GetCustomer(partialWrittenCustomerEvent.CustomerUID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Customer from CustomerRepo");
      Assert.AreEqual(customer, g.Result, "Customer details are incorrect from CustomerRepo");
    }

  /// <summary>
  /// Update Customer - customer exists with later date
  ///    should ignore
  /// </summary>
  [TestMethod]
    public void UpdateCustomer_AlreadyExistsLaterActionUTC()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };

      var updateCustomerEvent = new UpdateCustomerEvent
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        CustomerName = "The Customer Name FirstTime",
        ActionUTC = ActionUTC.AddMinutes(1)
      };

      var updateCustomerEvent2 = new UpdateCustomerEvent
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        CustomerName = "The Customer Name SecondsTime",
        ActionUTC = ActionUTC.AddMinutes(2)
      };

      customerContext.StoreEvent(createCustomerEvent).Wait();
      customerContext.StoreEvent(updateCustomerEvent2).Wait();
      var s = customerContext.StoreEvent(updateCustomerEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "Update Customer is old, should be ignored");

      Customer customer = CopyModel(createCustomerEvent);
      customer.Name = updateCustomerEvent2.CustomerName;
      customer.LastActionedUTC = updateCustomerEvent2.ActionUTC;
      var g = customerContext.GetCustomer(createCustomerEvent.CustomerUID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Customer from CustomerRepo");
      Assert.AreEqual(customer, g.Result, "Customer details are incorrect from CustomerRepo");
    }

    /// <summary>
    /// Delete Customer - Happy path
    ///    exists, and update actionUTC is later
    ///    set isDeleted flag
    /// </summary>
    [TestMethod]
    public void DeleteCustomerEvent_HappyPath()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };

      var deleteCustomerEvent = new DeleteCustomerEvent
      {
        CustomerUID = createCustomerEvent.CustomerUID,       
        ActionUTC = ActionUTC.AddHours(1)
      };

      customerContext.StoreEvent(createCustomerEvent).Wait();
      var s = customerContext.StoreEvent(deleteCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      Customer customer = CopyModel(createCustomerEvent);
      customer.IsDeleted = true;
      customer.LastActionedUTC = deleteCustomerEvent.ActionUTC;
      var g = customerContext.GetCustomer(createCustomerEvent.CustomerUID);
      g.Wait();
      Assert.IsNull(g.Result, "should not retrieve delted Customer from CustomerRepo");
      
      g = customerContext.GetCustomer_UnitTest(createCustomerEvent.CustomerUID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Customer from CustomerRepo");
      Assert.AreEqual(customer, g.Result, "Customer details are incorrect from CustomerRepo");
    }

    /// <summary>
    /// Delete Customer - customer doesn't exist
    ///    should create with what we have. ANd set IsDeleted flag
    ///      this is so that if we get the Create later on, 
    ///          we don't then create it and have no idea that it was subsequently deleted
    /// </summary>
    [TestMethod]
    public void DeleteCustomerEvent_CustomerDoesntExist()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid();

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = customerUid,
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Dealer.ToString(),
        ActionUTC = ActionUTC
      };

      var deleteCustomerEvent = new DeleteCustomerEvent
      {
        CustomerUID = customerUid,
        ActionUTC = ActionUTC.AddHours(1)
      };

      var partialWrittenCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = customerUid,
        CustomerName = "",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };

      var s = customerContext.StoreEvent(deleteCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer event not written");

      s = customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "Customer event not written");

      Customer customer = CopyModel(partialWrittenCustomerEvent);
      customer.IsDeleted = true;
      customer.LastActionedUTC = deleteCustomerEvent.ActionUTC;
      var g = customerContext.GetCustomer(createCustomerEvent.CustomerUID);
      g.Wait();
      Assert.IsNull(g.Result, "should not retrieve delted Customer from CustomerRepo");

      g = customerContext.GetCustomer_UnitTest(partialWrittenCustomerEvent.CustomerUID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve Customer from CustomerRepo");
      Assert.AreEqual(customer, g.Result, "Customer details are incorrect from CustomerRepo");
    }

    /// <summary>
    /// Delete Customer - customer exists with later date
    ///    hmmmm ignore - this is nonsense
    /// </summary>
    [TestMethod]
    public void DeleteCustomer_AlreadyExistsLaterActionUTC()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };

      var deleteCustomerEvent = new DeleteCustomerEvent
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ActionUTC = ActionUTC.AddHours(-1)
      };

      customerContext.StoreEvent(createCustomerEvent).Wait();
      var s = customerContext.StoreEvent(deleteCustomerEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "Customer event not written");

      Customer customer = CopyModel(createCustomerEvent);
      var g = customerContext.GetCustomer(createCustomerEvent.CustomerUID);
      g.Wait();
      Assert.IsNotNull(g.Result, "should retrieve Customer from CustomerRepo");
      Assert.AreEqual(customer, g.Result, "Customer details are incorrect from CustomerRepo");
    }

    #endregion

    #region AssociateCustomerWithUser

    /// <summary>
    ///  AssociateCustomerUser - Happy path
    ///    CustomerUser doesn't exist. Customer does
    /// </summary>
    [TestMethod]
    public void AssociateCustomerUserEvent_HappyPath()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUTC
      };

      var associateCustomerUserEvent = new AssociateCustomerUserEvent
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        UserUID = Guid.NewGuid(),
        ActionUTC = actionUTC
      };

      customerContext.StoreEvent(createCustomerEvent).Wait();
      var s = customerContext.StoreEvent(associateCustomerUserEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "CustomerUser event not written");

      var g = customerContext.GetAssociatedCustomerbyUserUid(associateCustomerUserEvent.UserUID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve CustomerUser from CustomerRepo");
      Assert.AreEqual(associateCustomerUserEvent.CustomerUID.ToString(), g.Result.CustomerUID, "CustomerUser UID is incorrect from CustomerRepo");
      Assert.AreEqual(createCustomerEvent.CustomerName, g.Result.Name, "CustomerUser name is incorrect from CustomerRepo");
    }

    /// <summary>
    ///  AssociateCustomerUser - customer doesn't exist
    ///    should create CustomerUser with what we have. Don't create Customer.
    /// </summary>
    [TestMethod]
    public void AssociateCustomerUserEvent_CustomerDoesntExist()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid();

      var associateCustomerUserEvent = new AssociateCustomerUserEvent
      {
        CustomerUID = customerUid,
        UserUID = Guid.NewGuid(),
        ActionUTC = actionUTC
      };
                  
      customerContext.StoreEvent(associateCustomerUserEvent).Wait();
      associateCustomerUserEvent.ActionUTC = actionUTC.AddDays(-1);
      var s = customerContext.StoreEvent(associateCustomerUserEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "CustomerUser event not written");

      var g = customerContext.GetAssociatedCustomerbyUserUid(associateCustomerUserEvent.UserUID);
      g.Wait();
      Assert.IsNull(g.Result, "Unable to retrieve CustomerUser from CustomerRepo");

      var h = customerContext.GetAssociatedCustomerbyUserUid_UnitTest(associateCustomerUserEvent.UserUID);
      h.Wait();
      Assert.IsNotNull(h.Result, "Unable to retrieve CustomerUser from CustomerRepo");
      Assert.AreEqual(associateCustomerUserEvent.CustomerUID.ToString(), h.Result.CustomerUID, "CustomerUser UID is incorrect from CustomerRepo");
      Assert.AreEqual(actionUTC, h.Result.LastActionedUTC, "ActionUTC should be for original association");
    }

    /// <summary>
    ///  AssociateCustomerUser - customerUser exists 
    ///    ignore - this is nonsense
    /// </summary>
    [TestMethod]
    public void AssociateCustomerUser_AlreadyExists()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Dealer.ToString(),
        ActionUTC = actionUTC
      };

      var associateCustomerUserEvent = new AssociateCustomerUserEvent
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        UserUID = Guid.NewGuid(),
        ActionUTC = actionUTC
      };
      
      customerContext.StoreEvent(createCustomerEvent).Wait();
      customerContext.StoreEvent(associateCustomerUserEvent).Wait();
      associateCustomerUserEvent.ActionUTC = actionUTC.AddDays(1);
      var s = customerContext.StoreEvent(associateCustomerUserEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "CustomerUser event not written");

      var g = customerContext.GetAssociatedCustomerbyUserUid(associateCustomerUserEvent.UserUID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve CustomerUser from CustomerRepo");
      Assert.AreEqual(associateCustomerUserEvent.CustomerUID.ToString(), g.Result.CustomerUID, "CustomerUser UID is incorrect from CustomerRepo");
      Assert.AreEqual(actionUTC, g.Result.LastActionedUTC, "ActionUTC should be for original association");
    }

    /// <summary>
    ///  DissociateCustomerUser - Happy path
    ///    CustomerUser exists, delete it
    /// </summary>
    [TestMethod]
    public void DissociateCustomerUserEvent_HappyPath()
    {
      DateTime actionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = actionUTC
      };

      var associateCustomerUserEvent = new AssociateCustomerUserEvent
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        UserUID = Guid.NewGuid(),
        ActionUTC = actionUTC.AddMinutes(1)
      };

      var dissociateCustomerUserEvent = new DissociateCustomerUserEvent
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        UserUID = associateCustomerUserEvent.UserUID,
        ActionUTC = actionUTC.AddMinutes(2)
      };
      
      customerContext.StoreEvent(createCustomerEvent).Wait();
      customerContext.StoreEvent(associateCustomerUserEvent).Wait();
      var g = customerContext.GetAssociatedCustomerbyUserUid(associateCustomerUserEvent.UserUID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve CustomerUser from CustomerRepo");
      

      var s = customerContext.StoreEvent(dissociateCustomerUserEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "CustomerUser event not written");
      g = customerContext.GetAssociatedCustomerbyUserUid(associateCustomerUserEvent.UserUID);
      g.Wait();
      Assert.IsNull(g.Result, "Should no longer be associated from CustomerRepo");      
    }

    /// <summary>
    ///  DissociateCustomerUser - customerUser doesn't exist
    ///    do nothing
    /// </summary>
    [TestMethod]
    public void DissociateCustomerUserEvent_CustomerUserDoesntExist()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);
      var userUid = Guid.NewGuid();

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };

      var dissociateCustomerUserEvent = new DissociateCustomerUserEvent
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        UserUID = userUid,
        ActionUTC = ActionUTC.AddMinutes(2)
      };
      
      customerContext.StoreEvent(createCustomerEvent).Wait();
      var g = customerContext.GetAssociatedCustomerbyUserUid(dissociateCustomerUserEvent.UserUID);
      g.Wait();
      Assert.IsNull(g.Result, "Should be no association");

      var s = customerContext.StoreEvent(dissociateCustomerUserEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "Should have ignored this event");
      g = customerContext.GetAssociatedCustomerbyUserUid(dissociateCustomerUserEvent.UserUID);
      g.Wait();
      Assert.IsNull(g.Result, "Should stil not be associated from CustomerRepo");
    }

    /// <summary>
    ///  DissociateCustomerUser - customerUser exists with later date
    ///    ignore - this is nonsense
    /// </summary>
    [TestMethod]
    public void DissociateCustomerUser_AlreadyExistsLaterActionUTC()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };

      var associateCustomerUserEvent = new AssociateCustomerUserEvent
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        UserUID = Guid.NewGuid(),
        ActionUTC = ActionUTC.AddMinutes(1)
      };

      var dissociateCustomerUserEvent = new DissociateCustomerUserEvent
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        UserUID = associateCustomerUserEvent.UserUID,
        ActionUTC = ActionUTC.AddMinutes(3)
      };
      
      customerContext.StoreEvent(createCustomerEvent).Wait();
      customerContext.StoreEvent(associateCustomerUserEvent).Wait();
      customerContext.StoreEvent(dissociateCustomerUserEvent).Wait();
      dissociateCustomerUserEvent.ActionUTC = ActionUTC.AddMinutes(2);
      var s = customerContext.StoreEvent(dissociateCustomerUserEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "CustomerUser event not written");

      var g = customerContext.GetAssociatedCustomerbyUserUid(associateCustomerUserEvent.UserUID);
      g.Wait();
      Assert.IsNull(g.Result, "Should no longer be associated from CustomerRepo");
    }

    #endregion

    #region tccOrg

    /// <summary>
    /// Create Customer and CustomerTccOrg - Happy path i.e. 
    ///   to test the getter
    /// </summary>
    [TestMethod]
    public void CreateCustomerTccOrg_HappyPath()
    {
      DateTime ActionUTC = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = ActionUTC
      };

      var createCustomerTccOrgEvent = new CreateCustomerTccOrgEvent
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        TCCOrgID = "TCCOrgID " + createCustomerEvent.CustomerUID.ToString(),
        ActionUTC = ActionUTC
      };

      customerContext.StoreEvent(createCustomerEvent).Wait();
      var s = customerContext.StoreEvent(createCustomerTccOrgEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Customer TCCOrg event not written");

      var g = customerContext.GetCustomerWithTccOrg(createCustomerEvent.CustomerUID);  g.Wait();
      var byCustomerUID = g.Result;
      Assert.AreEqual(createCustomerTccOrgEvent.TCCOrgID, byCustomerUID.TCCOrgID, "CustomerTCCOrg details are incorrect from CustomerRepo");

      g = customerContext.GetCustomerWithTccOrg(createCustomerTccOrgEvent.TCCOrgID); g.Wait();
      var byTCCOrg = g.Result;
      Assert.AreEqual(byCustomerUID, byTCCOrg, "CustomerTCCOrg details are incorrect from CustomerRepo");
    }

    #endregion tccOrg

    #region private

    private static Customer CopyModel(CreateCustomerEvent kafkaCustomerEvent)
    {
      return new Customer
      {
        CustomerUID = kafkaCustomerEvent.CustomerUID.ToString(),
        Name = kafkaCustomerEvent.CustomerName,
        CustomerType = (CustomerType)Enum.Parse(typeof(CustomerType), kafkaCustomerEvent.CustomerType, true),
        LastActionedUTC = kafkaCustomerEvent.ActionUTC
      };
    }
    #endregion
  }
}