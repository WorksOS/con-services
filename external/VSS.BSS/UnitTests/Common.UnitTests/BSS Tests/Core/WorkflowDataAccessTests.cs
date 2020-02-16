using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class WorkflowDataAccessTests : BssUnitTestBase
  {

    [TestMethod]
    public void Data_Current_gets_reference_to_implmentation_of_DataContext()
    {
      Assert.IsInstanceOfType(Data.Context, typeof(DataContext));
      Assert.IsInstanceOfType(Data.Context, typeof(IDisposable));
      Data.Context.Dispose();
    }
  }
}