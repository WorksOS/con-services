using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests
{
  /// <summary>
  /// Summary description for ObjectContextTransactionScopeTests
  /// </summary>
  [TestClass]
  public class ObjectContextTransactionScopeTests : UnitTestBase
  {  
    [TestMethod]
    public void EnrollObjectContextsNullObjectContexts_Success() 
    {
      bool expectedException = false;
      ObjectContextTransactionScope target = new ObjectContextTransactionScope();

      try
      {
        target.EnrollObjectContexts(null);
      }
      catch (Exception ex)
      {
        Assert.IsTrue(ex is ArgumentNullException);
        expectedException = true;
      }
      Assert.IsTrue(expectedException);
    }

    [TestMethod]
    [DatabaseTest]
    public void EnrollObjectContextsAlreadyDisposed_Success()
    {
      bool expectedException = false;
      PrivateObject target = new PrivateObject(typeof(ObjectContextTransactionScope));
      target.SetFieldOrProperty("_disposed", true);

      try
      {
        target.Invoke("EnrollObjectContexts", new object[] { Ctx.RawContext });
      }
      catch (Exception ex)
      {
        Assert.IsTrue(ex is ObjectDisposedException);
        expectedException = true;
      }
      Assert.IsTrue(expectedException);
    }

    [TestMethod]
    [DatabaseTest]
    public void EnrollObjectContextsConnectionStateClosed_Success()
    {
      bool unexpectedException = false;

      try
      {
        ObjectContextTransactionScope target = new ObjectContextTransactionScope();
        ObjectContextTransactionScope result = (ObjectContextTransactionScope)target.EnrollObjectContexts(Ctx.OpContext, Ctx.RawContext);
      }
      catch (Exception)
      {
        unexpectedException = true;
      }
      Assert.IsFalse(unexpectedException);
    }

    [TestMethod]
    public void CommitAlreadyDisposed_Success() 
    {
      bool expectedException = false;
      PrivateObject target = new PrivateObject(typeof(ObjectContextTransactionScope));
      target.SetFieldOrProperty("_disposed", true);

      try
      {
        target.Invoke("Commit");
      }
      catch (Exception ex)
      {
        Assert.IsTrue(ex is ObjectDisposedException);
        expectedException = true;
      }
      Assert.IsTrue(expectedException);
    }

    [TestMethod]
    [DatabaseTest]
    public void Commit_Success()
    {
      bool unexpectedException = false;

      try
      {
        ObjectContextTransactionScope target = new ObjectContextTransactionScope();
        ObjectContextTransactionScope result = (ObjectContextTransactionScope)target.EnrollObjectContexts(Ctx.OpContext, Ctx.RawContext);
        result.Commit();
      }
      catch (Exception)
      {
        unexpectedException = true;
      }
      Assert.IsFalse(unexpectedException);
    }

    
    [TestMethod]
    [DatabaseTest]
    public void Dispose_Success() 
    {
      bool unexpectedException = false;

      try
      {
        ObjectContextTransactionScope target = new ObjectContextTransactionScope();
        ObjectContextTransactionScope result = (ObjectContextTransactionScope)target.EnrollObjectContexts(Ctx.OpContext, Ctx.RawContext);
        result.Dispose();
      }
      catch (Exception)
      {
        unexpectedException = true;
      }
      Assert.IsFalse(unexpectedException);
    }
  }
}
