using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common._Framework.CustomAttributes;
using VSS.UnitTest.Common._Framework.CustomAttributes.Implementation;
using VSS.UnitTest.Common.Contexts;

namespace VSS.UnitTest.Common
{
  [UnitTestBaseClass(typeof(DatabaseTestAspect))]
  [TestClass]
  public class UnitTestBase : AopUnitTestBase
  {
    private TestDataHelper _testData;
    private readonly DateTime _startTime = DateTime.Now;
    protected bool FailLongRunningTests = false;
    protected double AllowableTestDuration = 3.0D;

    private static System.Data.Entity.SqlServer.SqlProviderServices _instance =
      System.Data.Entity.SqlServer.SqlProviderServices.Instance;

    protected virtual IContextContainer Ctx
    {
      get { return ContextContainer.Current; }
    }
  
    protected TestDataHelper TestData
    {
      get
      {
        if (_testData == null)
        {
          _testData = new TestDataHelper();
					
        }
          return _testData;
      }
    }

    public virtual void InitializeTest()
    {
      // Change this to true to find long running tests
      FailLongRunningTests = false;

      // Dispose of any leftovers - just to make sure
      Reset();
      
      InitializeTestData();
    }

    

    public virtual void InitializeTestData()
    {
    }

    [TestInitialize]
    public void UnitTestBaseTestInitialize()
    {
      InitializeTest();
    }

    [TestCleanup]
    public void UnitTestBaseTestCleanup()
    {
      Reset();
      TestTimer();
    }

    protected virtual void Reset()
    {
      SetContextCreation();
      DisposeContexts();
    }

    protected virtual void DisposeContexts()
    {
      _opContextMock = null;
      

      var sesh = new SessionContext();
      sesh.DisposeNHOpContext();
      
      Ctx.Dispose();
    }

    public virtual void SetContextCreation()
    {
      // SWAP IN MOCK CONTEXT CREATION METHODS
      ObjectContextFactory.ContextCreationFuncs(
      (bool readOnly) => GetOpContext(readOnly));
    }

    protected virtual void TestTimer() 
    {
      var testDuration = DateTime.Now.Subtract(_startTime);
      Debug.WriteLine(string.Format("Unit Test Duration: {0}", testDuration));

      if (FailLongRunningTests && testDuration.TotalSeconds > AllowableTestDuration)
      {
        Assert.Fail("UnitTest duration too long...");
      }
    }
            
    #region MOCKED CONTEXTS

    
    private INH_OP _opContextMock;
    
    private INH_OP OpContext
    {
      get
      {
        if (_opContextMock == null)
          _opContextMock = new NH_OPMock();
        return _opContextMock;
      }
    }
    private INH_OP GetOpContext(bool readOnly)
    {
      INH_OP actualContext = OpContext;
      ((NH_OPMock)actualContext).SetReadOnlyness(readOnly);
      return actualContext;
    }

  

    #endregion

    public static DateTime GetValidTestDate()
    {
      DateTime testDate = DateTime.UtcNow.AddMonths(-8);

      return new DateTime(testDate.Year, testDate.Month, testDate.Day);
    }

    public static DateTime GetFirstMondayInMonth(DateTime originalDate)
    {
      var day1 = new DateTime(originalDate.Year, originalDate.Month, 1);
      while (true)
      {
          if (day1.DayOfWeek == DayOfWeek.Monday)
                return day1;

          day1 = day1.AddDays(1);
      }
    }
  }
}
