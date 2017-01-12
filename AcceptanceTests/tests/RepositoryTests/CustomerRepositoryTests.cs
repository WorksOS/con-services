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

      var s = customerContext.StoreEvent(createCustomerEvent);
      s = customerContext.StoreEvent(createCustomerEvent);
      s.Wait();
      Assert.AreEqual(0, s.Result, "Customer event not written");

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
    { throw new NotImplementedException(); }

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
        ActionUTC = now.AddHours(1)
      };

      var customerContext = new CustomerRepository(new GenericConfiguration());

      var s = customerContext.StoreEvent(createCustomerEvent);
      s = customerContext.StoreEvent(updateCustomerEvent);
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
    /// Update Customer - customer doesn't exist
    ///    should create with what we have.
    /// </summary>
    [TestMethod]
    public void UpdateCustomerEvent_CustomerDoesntExist()
    { throw new NotImplementedException(); }

    /// <summary>
    /// Update Customer - customer exists with later date
    ///    should ignore
    /// </summary>
    [TestMethod]
    public void UpdateCustomer_AlreadyExistsLaterActionUTC()
    { throw new NotImplementedException(); }

    // todo is there such a thing as a deleted customer?
    /// <summary>
    /// Delete Customer - Happy path
    ///    exists, and update actionUTC is later
    /// </summary>
    [TestMethod]
    public void DeleteCustomerEvent_HappyPath()
    { throw new NotImplementedException(); }

    /// <summary>
    /// Delete Customer - customer doesn't exist
    ///    should create with what we have. ANd set IsDeleted flag
    /// </summary>
    [TestMethod]
    public void DeleteCustomerEvent_CustomerDoesntExist()
    { throw new NotImplementedException(); }

    /// <summary>
    /// Delete Customer - customer exists with later date
    ///    hmmmm ignore - this is nonsense
    /// </summary>
    [TestMethod]
    public void DeleteCustomer_AlreadyExistsLaterActionUTC()
    { throw new NotImplementedException(); }


    #endregion

    #region CustomerUser

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
    { throw new NotImplementedException(); }

    /// <summary>
    ///  AssociateCustomerUser - customerUser exists 
    ///    ignore - this is nonsense
    /// </summary>
    [TestMethod]
    public void AssociateCustomerUser_AlreadyExists()
    { throw new NotImplementedException(); }

    // todo is there a business case for this?
    /// <summary>
    ///  DissociateCustomerUser - Happy path
    ///    CustomerUser exists, delete it
    /// </summary>
    [TestMethod]
    public void DissociateCustomerUserEvent_HappyPath()
    { throw new NotImplementedException(); }

    /// <summary>
    ///  DissociateCustomerUser - customerUser doesn't exist
    ///    do nothing
    /// </summary>
    [TestMethod]
    public void DissociateCustomerUserEvent_CustomerUserDoesntExist()
    { throw new NotImplementedException(); }

    /// <summary>
    ///  DissociateCustomerUser - customerUser exists with later date
    ///    ignore - this is nonsense
    /// </summary>
    [TestMethod]
    public void DissociateCustomerUser_AlreadyExistsLaterActionUTC()
    { throw new NotImplementedException(); }

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
 
 