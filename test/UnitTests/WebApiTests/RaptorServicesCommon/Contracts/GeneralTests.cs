
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Filters;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;

namespace VSS.Raptor.Service.WebApiTests.Common.Contracts
{
    [TestClass]
    public class GeneralTests
    {
        class TestContainer : RequestExecutorContainer
        {
            protected override ContractExecutionResult ProcessEx<T>(T item)
            {
                ContractExecutionStates.ClearDynamic();
                return new ContractExecutionResult(1,"test result");
            }

            protected override void ProcessErrorCodes()
            {
                ContractExecutionStates.DynamicAddwithOffset("OnSubmissionResult. ConnectionFailure.",
                        -10);
                ContractExecutionStates.DynamicAddwithOffset(
                        "The TAG file was found to be corrupted on its pre-processing scan.",
                        15);

              ContractExecutionStates.DynamicAddwithOffset(
                "A second list of dynamic errors",
                15, ContractExecutionStates.SecondDynamicOffset);
            }
        }

        [TestMethod]
        public void GenerateErrorlistTest()
        {
            TestContainer container = new TestContainer();
            Assert.AreEqual(11,container.GenerateErrorlist().Count);
            container.Process(WGSPoint.CreatePoint(1,1));
            Assert.AreEqual(8, container.GenerateErrorlist().Count);
        }

    }
}
