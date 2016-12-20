

using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Raptor.Service.WebApiModels.Interfaces;
using VSS.Raptor.Service.WebApiModels.ResultHandling;

namespace WebApiTests
{
  [TestClass]
  public class GeneralTests
  {
    class TestContainer : RequestExecutorContainer
    {
      protected override ContractExecutionResult ProcessEx<T>(T item)
      {
        ContractExecutionStates.ClearDynamic();
        return new ContractExecutionResult(1, "test result");
      }

      protected override void ProcessErrorCodes()
      {
        ContractExecutionStates.DynamicAddwithOffset("OnSubmissionResult. ConnectionFailure.",
                -10);
        ContractExecutionStates.DynamicAddwithOffset(
                "The TAG file was found to be corrupted on its pre-processing scan.",
                15);
      }
    }

    [TestMethod]
    public void GenerateErrorlistTest()
    {
      TestContainer container = new TestContainer();
      Assert.AreEqual(10, container.GenerateErrorlist().Count);
      container.Process(new ContractExecutionResult());//any object will do here
      Assert.AreEqual(8, container.GenerateErrorlist().Count);
    }

  }
}
