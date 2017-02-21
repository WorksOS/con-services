using Microsoft.VisualStudio.TestTools.UnitTesting;
using TAGProcServiceDecls;
using VSS.Raptor.Service.Common.Contracts;

namespace VSS.Raptor.Service.WebApiTests.Common.Contracts
{
    [TestClass()]
    public class ContractExecutionStatesEnumTests
    {
        [TestMethod()]
        public void DynamicAddwithOffsetTest()
        {
            ContractExecutionStatesEnum ContractExecutionStates = new ContractExecutionStatesEnum();
            ContractExecutionStates.DynamicAddwithOffset("Tag processing Successfull", (int)TTAGProcServerProcessResult.tpsprOK);
            ContractExecutionStates.DynamicAddwithOffset("Unknown error", (int)TTAGProcServerProcessResult.tpsprUnknown);
            ContractExecutionStates.DynamicAddwithOffset("OnSubmissionBase. Connection Failure.", (int)TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure);
            Assert.AreEqual(3, ContractExecutionStates.DynamicCount);
            Assert.AreEqual("OnSubmissionBase. Connection Failure.", ContractExecutionStates.FirstNameWithOffset((int)TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure));
            Assert.AreEqual((int)TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure + 100,
                    ContractExecutionStates.GetErrorNumberwithOffset(
                            (int) TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure));
            ContractExecutionStates.ClearDynamic();
            Assert.AreEqual(0, ContractExecutionStates.DynamicCount);

        }

    }
}
