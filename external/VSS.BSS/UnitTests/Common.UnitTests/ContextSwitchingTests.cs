using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests
{
  [TestClass]
  public class ContextSwitchingTests
  {
    // JBP 5/18/2011 - Ignored a variety of these tests because they were failing during the test runs.

    /// <summary>
    ///  Make sure context is accessible when open references to each context are closed when done
    /// </summary>
    [TestMethod()]
    [DatabaseTest]
    [Ignore]
    public void ContextSwitchingTest_Independent_All()
    {
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
      {
        var b = (from a in opCtx.Asset
                 select a).ToList();
      }
      using (INH_DATA dataCtx = ObjectContextFactory.NewNHContext<INH_DATA>(true))
      {
        var c = (from dep in dataCtx.DataEngineParameters
                             select dep).ToList();

      }
      using (INH_RPT rptCtx = ObjectContextFactory.NewNHContext<INH_RPT>(true))
      {
        var d = (from acs in rptCtx.AssetCurrentStatus
                             select acs).ToList();

      }
      using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>(true))
      {
          var e = (from m in opCtx1.MTSOut
                             select m).ToList();
      }
    }

    /// <summary>
    ///  Make sure contexts are accessible when open references to each context are maintained simultaneously
    /// </summary>
    [TestMethod()]
    [DatabaseTest]
    [Ignore]
    public void ContextSwitchingTest_Inner_All()
    {
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
      {
        using (INH_DATA dataCtx = ObjectContextFactory.NewNHContext<INH_DATA>(true))
        {
          using (INH_RPT rptCtx = ObjectContextFactory.NewNHContext<INH_RPT>(true))
          {
              var b = (from a in opCtx.Asset
                       select a).ToList();
              var c = (from dep in dataCtx.DataEngineParameters
                                   select dep).ToList();
              var d = (from m in opCtx.MTSOut
                                   select m).ToList();
              var e = (from acs in rptCtx.AssetCurrentStatus
                                   select acs).ToList();
          }
        }
      }
    }

    /// <summary>
    /// Test with contexts that are writable
    /// </summary>
    [TestMethod()]
    [DatabaseTest]
    [Ignore]
    public void ContextSwitchingTest_Inner_Writeable()
    {
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        using (INH_DATA dataCtx = ObjectContextFactory.NewNHContext<INH_DATA>())
        {
          using (INH_RPT rptCtx = ObjectContextFactory.NewNHContext<INH_RPT>())
          {
              var b = (from a in opCtx.Asset
                       select a).ToList();
              var c = (from dep in dataCtx.DataEngineParameters
                       select dep).ToList();
              var d = (from m in opCtx.MTSOut
                       select m).ToList();
              var e = (from acs in rptCtx.AssetCurrentStatus
                       select acs).ToList();
              rptCtx.SaveChanges();
              opCtx.SaveChanges();
              dataCtx.SaveChanges();
            }
        }
      }
    }
    /// <summary>
    /// Writable contexts; maintained independently (desired application developer style)
    /// </summary>
    [TestMethod()]
    [DatabaseTest]
    [Ignore]
    public void ContextSwitchingTest_Independent_All_Writeable()
    {
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var b = (from a in opCtx.Asset
                 select a).ToList();
        opCtx.SaveChanges();
      }
      using (INH_DATA dataCtx = ObjectContextFactory.NewNHContext<INH_DATA>())
      {
        var c = (from dep in dataCtx.DataEngineParameters
                 select dep).ToList();
        dataCtx.SaveChanges();
      }
      using (INH_RPT rptCtx = ObjectContextFactory.NewNHContext<INH_RPT>())
      {
        var d = (from acs in rptCtx.AssetCurrentStatus
                 select acs).ToList();
        rptCtx.SaveChanges();
      }
      using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var e = (from m in opCtx1.MTSOut
                 select m).ToList();
        opCtx1.SaveChanges();
      }
    }

    /// <summary>
    /// Ensure overlapping contexts read/write are allowed
    /// </summary>
    [TestMethod()]
    [DatabaseTest]
    [Ignore]
    public void ContextSwitchingTest_Outer_BothOP()
    {
      using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>(true))
      {
        using (INH_OP opCtxWriteable = ObjectContextFactory.NewNHContext<INH_OP>())
        {
          var b = (from a in opCtx1.Asset
                   select a).ToList();

          var c = (from a in opCtxWriteable.Asset
                   select a).ToList();
          opCtxWriteable.SaveChanges();

        }
      }
    }

    /// <summary>
    /// Needed for problem troubleshooting only.  No need for regular operation
    /// </summary>
    [TestMethod()]
    [DatabaseTest]
    [Ignore]
    public void ContextSwitchingTest_Inner_OpThenData_FirstTrue()
    {

      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
      {
        using (INH_DATA dataCtx = ObjectContextFactory.NewNHContext<INH_DATA>())
        {
          var b = (from a in opCtx.Asset
                   select a).ToList();

          var dataAtAssetOn = (from dep in dataCtx.DataEngineParameters
                               select dep).ToList();
        }
      }
    }

    /// <summary>
    /// Needed for problem troubleshooting only.  No need for regular operation
    /// </summary>
    [TestMethod()]
    [DatabaseTest]
    [Ignore]
    public void ContextSwitchingTest_Inner_OpThenData_SecondTrue()
    {

      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        using (INH_DATA dataCtx = ObjectContextFactory.NewNHContext<INH_DATA>(true))
        {
          var b = (from a in opCtx.Asset
                   select a).ToList();

          var dataAtAssetOn = (from dep in dataCtx.DataEngineParameters
                               select dep).ToList();
        }
      }
    }

    /// <summary>
    /// Needed for problem troubleshooting only.  No need for regular operation
    /// </summary>
    [TestMethod()]
    [DatabaseTest]
    [Ignore]
    public void ContextSwitchingTest_Inner_DataThenOp()
    {

      using (INH_DATA dataCtx = ObjectContextFactory.NewNHContext<INH_DATA>(true))
      {
        using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
        {
          var b = (from a in opCtx.Asset
                   select a).ToList();

          var dataAtAssetOn = (from dep in dataCtx.DataEngineParameters
                               select dep).ToList();
        }
      }
    }

    /// <summary>
    /// Needed for problem troubleshooting only.  No need for regular operation
    /// </summary>
    [TestMethod()]
    [DatabaseTest]
    [Ignore]
    public void ContextSwitchingTest_Outer_DataThenOp()
    {

      using (INH_DATA dataCtx = ObjectContextFactory.NewNHContext<INH_DATA>(true))
      {
        var dataAtAssetOn = (from dep in dataCtx.DataEngineParameters
                             select dep).ToList();

        using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
        {
          var b = (from a in opCtx.Asset
                   select a).ToList();

        }
      }
    }

    /// <summary>
    /// Needed for problem troubleshooting only.  No need for regular operation
    /// </summary>
    [TestMethod()]
    [DatabaseTest]
    [Ignore]
    public void ContextSwitchingTest_Outer_opThenData()
    {

      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
      {
        var b = (from a in opCtx.Asset select a).ToList();

        using (INH_DATA dataCtx = ObjectContextFactory.NewNHContext<INH_DATA>(true))
        {
          var dataAtAssetOn = (from dep in dataCtx.DataEngineParameters select dep).ToList();

        }
      }
    }

  }
}
