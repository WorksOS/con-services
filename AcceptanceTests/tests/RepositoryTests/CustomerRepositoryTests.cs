using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using VSS.Project.Service.Utils;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.Customer.Data;
using VSS.Customer.Data.Models;

namespace RepositoryTests
{
  [TestClass]
  public class CustomerRepositoryTests
  {
    [TestInitialize]
    public void Init()
    {
      var serviceCollection = new ServiceCollection();
      serviceCollection.AddSingleton<ILoggerFactory>((new LoggerFactory()).AddDebug());
      new DependencyInjectionProvider(serviceCollection.BuildServiceProvider());
    }

    #region Customers
    /// <summary>
    /// Test copying between kafka and repository models
    /// todo could be a unit test
    /// </summary>
    [TestMethod]
    public void CopyModels()
    {
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);

      var customer = new Customer()
      {
        CustomerUID = Guid.NewGuid().ToString(),
        Name = "The Customer Name",
        CustomerType = CustomerType.Corporate,
        LastActionedUTC = now
      };

      var kafkaCustomerEvent = CopyModel(customer);
      var copiedCustomer = CopyModel(kafkaCustomerEvent);

      Assert.AreEqual(customer, copiedCustomer, "Customer model conversion not completed sucessfully");
    }


    /// <summary>
    /// Create Customer - Happy path i.e. 
    ///   customer doesn't exist
    /// </summary>
    [TestMethod]
    public void CreateCustomer_HappyPath()
    {
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = now
      };

      var customerContext = new CustomerRepository(new GenericConfiguration());

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
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = now
      };

      var customerContext = new CustomerRepository(new GenericConfiguration());

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
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = now
      };

      var customerContext = new CustomerRepository(new GenericConfiguration());

      var s = customerContext.StoreEvent(createCustomerEvent);
      createCustomerEvent.ActionUTC = now.AddDays(1);
      s = customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "Customer event not written");

      Customer customer = CopyModel(createCustomerEvent);
      customer.LastActionedUTC = now;
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
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Dealer.ToString(),
        ActionUTC = now
      };

      var updateCustomerEvent = new UpdateCustomerEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        CustomerName = "The Customer Name Updated",
        ActionUTC = now.AddHours(1)
      };

      var customerContext = new CustomerRepository(new GenericConfiguration());

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
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = now
      };

      var updateCustomerEvent = new UpdateCustomerEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        CustomerName = "The Customer Name Updated",
        ActionUTC = now.AddMinutes(2)
      };
      
      var customerContext = new CustomerRepository(new GenericConfiguration());

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
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = now
      };

      var updateCustomerEvent = new UpdateCustomerEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        CustomerName = "The Customer Name Updated",
        ActionUTC = now.AddMinutes(2)
      };

      var deleteCustomerEvent = new DeleteCustomerEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ActionUTC = now.AddHours(1)
      };

      var customerContext = new CustomerRepository(new GenericConfiguration());

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
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUID = Guid.NewGuid();

      var updateCustomerEvent = new UpdateCustomerEvent()
      {
        CustomerUID = customerUID,
        CustomerName = "The Customer Name GotIt!",
        ActionUTC = now
      };

      var customerContext = new CustomerRepository(new GenericConfiguration());

      var s = customerContext.StoreEvent(updateCustomerEvent);
      s = customerContext.StoreEvent(updateCustomerEvent);
      s.Wait();
      Assert.AreEqual(1, s.Result, "Update Customer event not written");

      var partialWrittenCustomerEvent = new CreateCustomerEvent()
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
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = now
      };

      var updateCustomerEvent = new UpdateCustomerEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        CustomerName = "The Customer Name FirstTime",
        ActionUTC = now.AddMinutes(1)
      };

      var updateCustomerEvent2 = new UpdateCustomerEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        CustomerName = "The Customer Name SecondsTime",
        ActionUTC = now.AddMinutes(2)
      };

      var customerContext = new CustomerRepository(new GenericConfiguration());

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
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = now
      };

      var deleteCustomerEvent = new DeleteCustomerEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,       
        ActionUTC = now.AddHours(1)
      };

      var customerContext = new CustomerRepository(new GenericConfiguration());

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
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid();

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = customerUid,
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Dealer.ToString(),
        ActionUTC = now
      };

      var deleteCustomerEvent = new DeleteCustomerEvent()
      {
        CustomerUID = customerUid,
        ActionUTC = now.AddHours(1)
      };

      var partialWrittenCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = customerUid,
        CustomerName = "",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = now
      };

      var customerContext = new CustomerRepository(new GenericConfiguration());

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
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = now
      };

      var deleteCustomerEvent = new DeleteCustomerEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        ActionUTC = now.AddHours(-1)
      };

      var customerContext = new CustomerRepository(new GenericConfiguration());

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
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = now
      };

      var associateCustomerUserEvent = new AssociateCustomerUserEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        UserUID = Guid.NewGuid(),
        ActionUTC = now
      };

      var customerContext = new CustomerRepository(new GenericConfiguration());

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
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);
      var customerUid = Guid.NewGuid();

      var associateCustomerUserEvent = new AssociateCustomerUserEvent()
      {
        CustomerUID = customerUid,
        UserUID = Guid.NewGuid(),
        ActionUTC = now
      };

      var customerContext = new CustomerRepository(new GenericConfiguration());
            
      customerContext.StoreEvent(associateCustomerUserEvent).Wait();
      associateCustomerUserEvent.ActionUTC = now.AddDays(-1);
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
      Assert.AreEqual(now, h.Result.LastActionedUTC, "ActionUTC should be for original association");
    }

    /// <summary>
    ///  AssociateCustomerUser - customerUser exists 
    ///    ignore - this is nonsense
    /// </summary>
    [TestMethod]
    public void AssociateCustomerUser_AlreadyExists()
    {
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Dealer.ToString(),
        ActionUTC = now
      };

      var associateCustomerUserEvent = new AssociateCustomerUserEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        UserUID = Guid.NewGuid(),
        ActionUTC = now
      };

      var customerContext = new CustomerRepository(new GenericConfiguration());

      customerContext.StoreEvent(createCustomerEvent).Wait();
      customerContext.StoreEvent(associateCustomerUserEvent).Wait();
      associateCustomerUserEvent.ActionUTC = now.AddDays(1);
      var s = customerContext.StoreEvent(associateCustomerUserEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "CustomerUser event not written");

      var g = customerContext.GetAssociatedCustomerbyUserUid(associateCustomerUserEvent.UserUID);
      g.Wait();
      Assert.IsNotNull(g.Result, "Unable to retrieve CustomerUser from CustomerRepo");
      Assert.AreEqual(associateCustomerUserEvent.CustomerUID.ToString(), g.Result.CustomerUID, "CustomerUser UID is incorrect from CustomerRepo");
      Assert.AreEqual(now, g.Result.LastActionedUTC, "ActionUTC should be for original association");
    }

    /// <summary>
    ///  DissociateCustomerUser - Happy path
    ///    CustomerUser exists, delete it
    /// </summary>
    [TestMethod]
    public void DissociateCustomerUserEvent_HappyPath()
    {
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = now
      };

      var associateCustomerUserEvent = new AssociateCustomerUserEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        UserUID = Guid.NewGuid(),
        ActionUTC = now.AddMinutes(1)
      };

      var dissociateCustomerUserEvent = new DissociateCustomerUserEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        UserUID = associateCustomerUserEvent.UserUID,
        ActionUTC = now.AddMinutes(2)
      };

      var customerContext = new CustomerRepository(new GenericConfiguration());

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
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);
      var userUid = Guid.NewGuid();

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = now
      };

      var dissociateCustomerUserEvent = new DissociateCustomerUserEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        UserUID = userUid,
        ActionUTC = now.AddMinutes(2)
      };

      var customerContext = new CustomerRepository(new GenericConfiguration());

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
      DateTime now = new DateTime(2017, 1, 1, 2, 30, 3);

      var createCustomerEvent = new CreateCustomerEvent()
      {
        CustomerUID = Guid.NewGuid(),
        CustomerName = "The Customer Name",
        CustomerType = CustomerType.Customer.ToString(),
        ActionUTC = now
      };

      var associateCustomerUserEvent = new AssociateCustomerUserEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        UserUID = Guid.NewGuid(),
        ActionUTC = now.AddMinutes(1)
      };

      var dissociateCustomerUserEvent = new DissociateCustomerUserEvent()
      {
        CustomerUID = createCustomerEvent.CustomerUID,
        UserUID = associateCustomerUserEvent.UserUID,
        ActionUTC = now.AddMinutes(3)
      };

      var customerContext = new CustomerRepository(new GenericConfiguration());

      customerContext.StoreEvent(createCustomerEvent).Wait();
      customerContext.StoreEvent(associateCustomerUserEvent).Wait();
      customerContext.StoreEvent(dissociateCustomerUserEvent).Wait();
      dissociateCustomerUserEvent.ActionUTC = now.AddMinutes(2);
      var s = customerContext.StoreEvent(dissociateCustomerUserEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "CustomerUser event not written");

      var g = customerContext.GetAssociatedCustomerbyUserUid(associateCustomerUserEvent.UserUID);
      g.Wait();
      Assert.IsNull(g.Result, "Should no longer be associated from CustomerRepo");
    }

    #endregion

    #region private
    private CreateCustomerEvent CopyModel(Customer customer)
    {
      return new CreateCustomerEvent()
      {
        CustomerUID = Guid.Parse(customer.CustomerUID),
        CustomerName = customer.Name,
        CustomerType = customer.CustomerType.ToString(),
        ActionUTC = customer.LastActionedUTC
      };
    }

    private Customer CopyModel(CreateCustomerEvent kafkaCustomerEvent)
    {
      return new Customer()
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
 
 