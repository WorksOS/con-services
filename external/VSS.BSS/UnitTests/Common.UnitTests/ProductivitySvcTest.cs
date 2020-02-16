using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Nighthawk.ServicesAPI;
using System.Transactions;
using VSS.Nighthawk.NHCommonServices;
using VSS.Nighthawk.NHWebServices;
using EM = VSS.Nighthawk.EntityModels;
using VSS.Nighthawk.EntityModels;
using System.ServiceModel;
using System.Data.Objects;
using System.Linq.Expressions;

namespace UnitTests
{

  [TestClass]
  public class ProductivitySvcTest : ServerAPITestBase
  {

    #region Additional test attributes
    //
    // You can use the following additional attributes as you write your tests:
    //
    // Use ClassInitialize to run code before running the first test in the class
    [ClassInitialize()]
    public static void MyClassInitialize(TestContext testContext)
    {
      ConfigureEnvironment();
    }
    //
    // Use ClassCleanup to run code after all tests in a class have run
    // [ClassCleanup()]
    // public static void MyClassCleanup() { }
    //
    // Use TestInitialize to run code before running each test 
    // [TestInitialize()]
    // public void MyTestInitialize() { }
    //
    // Use TestCleanup to run code after each test has run
    // [TestCleanup()]
    // public void MyTestCleanup() { }
    //
    #endregion

 

  }

}
