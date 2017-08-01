using DesignProfilerDecls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TAGProcServiceDecls;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;

namespace VSS.Productivity3D.WebApiTests.RaptorServicesCommon.Contracts
{
    [TestClass]
    public class ContractExecutionStatesEnumTests
    {
        [TestMethod]
        public void DynamicAddwithOffsetTest()
        {
            ContractExecutionStatesEnum ContractExecutionStates = new ContractExecutionStatesEnum();
            ContractExecutionStates.DynamicAddwithOffset("Tag processing Successfull", (int)TTAGProcServerProcessResult.tpsprOK);
            ContractExecutionStates.DynamicAddwithOffset("Unknown error", (int)TTAGProcServerProcessResult.tpsprUnknown);
            ContractExecutionStates.DynamicAddwithOffset("OnSubmissionBase. Connection Failure.", (int)TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure);
            Assert.AreEqual(3, ContractExecutionStates.DynamicCount);
            Assert.AreEqual("OnSubmissionBase. Connection Failure.", ContractExecutionStates.FirstNameWithOffset((int)TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure));
            Assert.AreEqual((int)TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure,
                    ContractExecutionStates.GetErrorNumberwithOffset(
                            (int) TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure));
            ContractExecutionStates.ClearDynamic();
            Assert.AreEqual(0, ContractExecutionStates.DynamicCount);

        }

      [TestMethod]
      public void DynamicAddTwoSetsTest()
      {
        ContractExecutionStatesEnum ContractExecutionStates = new ContractExecutionStatesEnum();
        ContractExecutionStates.DynamicAddwithOffset("Tag processing Successfull", (int)TTAGProcServerProcessResult.tpsprOK);
        ContractExecutionStates.DynamicAddwithOffset("Unknown error", (int)TTAGProcServerProcessResult.tpsprUnknown);
        ContractExecutionStates.DynamicAddwithOffset("OnSubmissionBase. Connection Failure.", (int)TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure);

        ContractExecutionStates.DynamicAddwithOffset("OK", (int)TDesignProfilerRequestResult.dppiOK);
        ContractExecutionStates.DynamicAddwithOffset("Unknown Error", (int)TDesignProfilerRequestResult.dppiUnknownError);
        ContractExecutionStates.DynamicAddwithOffset("Could Not Connect To Server", (int)TDesignProfilerRequestResult.dppiCouldNotConnectToServer);
        ContractExecutionStates.DynamicAddwithOffset("Failed To Convert Client WGS Coords", (int)TDesignProfilerRequestResult.dppiFailedToConvertClientWGSCoords);

        Assert.AreEqual(7, ContractExecutionStates.DynamicCount);
        Assert.AreEqual("OnSubmissionBase. Connection Failure.", ContractExecutionStates.FirstNameWithOffset((int)TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure));
        Assert.AreEqual((int)TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure,
          ContractExecutionStates.GetErrorNumberwithOffset(
            (int)TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure));
        Assert.AreEqual("Failed To Convert Client WGS Coords", ContractExecutionStates.FirstNameWithOffset((int)TDesignProfilerRequestResult.dppiFailedToConvertClientWGSCoords));
        Assert.AreEqual((int)TDesignProfilerRequestResult.dppiFailedToConvertClientWGSCoords,
          ContractExecutionStates.GetErrorNumberwithOffset(
            (int)TDesignProfilerRequestResult.dppiFailedToConvertClientWGSCoords));

        ContractExecutionStates.ClearDynamic();
        Assert.AreEqual(0, ContractExecutionStates.DynamicCount);
      }

  }
}
