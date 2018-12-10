using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;

namespace VSS.Productivity3D.WebApiTests.RaptorServicesCommon.Contracts
{
  [TestClass]
  public class GeneralTests
  {
    private class TestContainer : RequestExecutorContainer
    {
      public TestContainer()
      {
        ProcessErrorCodes();
      }

      protected override ContractExecutionResult ProcessEx<T>(T item)
      {
        ContractExecutionStates.ClearDynamic();
        return new ContractExecutionResult(1, "test result");
      }

      protected sealed override void ProcessErrorCodes()
      {
        ContractExecutionStates.DynamicAddwithOffset("OnSubmissionResult. ConnectionFailure.", -10);
        ContractExecutionStates.DynamicAddwithOffset("The TAG file was found to be corrupted on its pre-processing scan.", 15);
        ContractExecutionStates.DynamicAddwithOffset("A second list of dynamic errors", 15);
      }
    }

    [TestMethod]
    public void GenerateErrorlistTest()
    {
      TestContainer container = new TestContainer();
      Assert.AreEqual(19, container.GenerateErrorlist().Count);
      container.Process(new WGSPoint(1, 1));
      Assert.AreEqual(16, container.GenerateErrorlist().Count);
    }
  }
}
