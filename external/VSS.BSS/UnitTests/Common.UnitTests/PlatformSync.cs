using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Transactions;
using VSS.Nighthawk.EntityModels;

namespace UnitTests
{
  [TestClass]
  public class PlatformSync : ServerAPITestBase
  {

    #region Additional test attributes
    //
    // You can use the following additional attributes as you write your tests:
    //
    // Use ClassInitialize to run code before running the first test in the class
    // [ClassInitialize()]
    // public static void MyClassInitialize(TestContext testContext) { }
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
    [TestMethod()]
    [ExpectedException(typeof(OptimisticConcurrencyException))]
    public void Concurrency()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        using (NH_OP ctx1 = Model.NewNHContext<NH_OP>())
        {
          TelvisantPlatform tp = (from item in ctx1.TelvisantPlatform
                                  where item.CustomerName == "CS"
                                  select item).FirstOrDefault<TelvisantPlatform>();
          Assert.IsNotNull(tp, "This test is utilizing the base data entry in the TelvisantPlatform table");

          tp.SyncInProgress = true;
          tp.LastSyncUTC = DateTime.UtcNow;

          using (NH_OP ctx2 = Model.NewNHContext<NH_OP>())
          {
            TelvisantPlatform tp2 = (from item in ctx2.TelvisantPlatform
                                    where item.CustomerName == "CS"
                                    select item).FirstOrDefault<TelvisantPlatform>();
            Assert.IsNotNull(tp2, "This test is utilizing the base data entry in the TelvisantPlatform table");

            tp2.SyncInProgress = true;
            tp2.LastSyncUTC = DateTime.UtcNow.AddSeconds(1.0);
            int res2 = ctx2.SaveChanges();
            Assert.IsTrue(res2 > 0, "Save failed. Dammit!");
          }

          int res1 = ctx1.SaveChanges();
          Assert.AreEqual<int>(0, res1, "Save should be rejected due to concurrency mode of Fixed set on LastSyncUTC field");
          Assert.IsTrue(false, "Actually, we expect an OptimisticConcurrencyException to be thrown in the SaveChanges above, so you shouldn't even get to here");
        }
      }
    }


  }
}
