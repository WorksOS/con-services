
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Productivity3D.Models.Enums;
#if RAPTOR
using DesignProfilerDecls;
#endif

namespace VSS.Productivity3D.WebApiTests.RaptorServicesCommon.Contracts
{
  [TestClass]
  public class ContractExecutionStatesEnumTests
  {

    [TestMethod]
    public void DynamicAddwithOffsetTest()
    {
      var contractExecutionStates = new ContractExecutionStatesEnum();
      contractExecutionStates.DynamicAddwithOffset("Tag processing Successfull", (int)TAGProcServerProcessResultCode.OK);
      contractExecutionStates.DynamicAddwithOffset("Unknown error", (int)TAGProcServerProcessResultCode.Unknown);
      contractExecutionStates.DynamicAddwithOffset(
        "OnSubmissionBase. Connection Failure.",
        (int)TAGProcServerProcessResultCode.OnSubmissionBaseConnectionFailure);

      Assert.AreEqual(3, contractExecutionStates.DynamicCount);
      Assert.AreEqual(
        "OnSubmissionBase. Connection Failure.",
        contractExecutionStates.FirstNameWithOffset((int)TAGProcServerProcessResultCode.OnSubmissionBaseConnectionFailure));

      Assert.AreEqual(
        (int)TAGProcServerProcessResultCode.OnSubmissionBaseConnectionFailure + 2000,
        contractExecutionStates.GetErrorNumberwithOffset(
          (int)TAGProcServerProcessResultCode.OnSubmissionBaseConnectionFailure));

      contractExecutionStates.ClearDynamic();
      Assert.AreEqual(0, contractExecutionStates.DynamicCount);
    }
#if RAPTOR
    [TestMethod]
    public void DynamicAddTwoSetsTest()
    {
      var contractExecutionStates = new ContractExecutionStatesEnum();
      contractExecutionStates.DynamicAddwithOffset("Tag processing Successfull", (int)TAGProcServerProcessResultCode.OK);
      contractExecutionStates.DynamicAddwithOffset("Unknown error", (int)TAGProcServerProcessResultCode.Unknown);
      contractExecutionStates.DynamicAddwithOffset(
        "OnSubmissionBase. Connection Failure.",
        (int)TAGProcServerProcessResultCode.OnSubmissionBaseConnectionFailure);

      contractExecutionStates.DynamicAddwithOffset("OK", (int)TDesignProfilerRequestResult.dppiOK);
      contractExecutionStates.DynamicAddwithOffset("Unknown Error", (int)TDesignProfilerRequestResult.dppiUnknownError);
      contractExecutionStates.DynamicAddwithOffset("Could Not Connect To Server", (int)TDesignProfilerRequestResult.dppiCouldNotConnectToServer);
      contractExecutionStates.DynamicAddwithOffset("Failed To Convert Client WGS Coords", (int)TDesignProfilerRequestResult.dppiFailedToConvertClientWGSCoords);

      Assert.AreEqual(7, contractExecutionStates.DynamicCount);
      Assert.AreEqual(
        "OnSubmissionBase. Connection Failure.",
        contractExecutionStates.FirstNameWithOffset((int)TAGProcServerProcessResultCode.OnSubmissionBaseConnectionFailure));

      Assert.AreEqual(
        (int)TAGProcServerProcessResultCode.OnSubmissionBaseConnectionFailure + 2000,
        contractExecutionStates.GetErrorNumberwithOffset(
          (int)TAGProcServerProcessResultCode.OnSubmissionBaseConnectionFailure));

      Assert.AreEqual("Failed To Convert Client WGS Coords", contractExecutionStates.FirstNameWithOffset((int)TDesignProfilerRequestResult.dppiFailedToConvertClientWGSCoords));
      Assert.AreEqual((int)TDesignProfilerRequestResult.dppiFailedToConvertClientWGSCoords + 2000,
        contractExecutionStates.GetErrorNumberwithOffset(
          (int)TDesignProfilerRequestResult.dppiFailedToConvertClientWGSCoords));

      contractExecutionStates.ClearDynamic();
      Assert.AreEqual(0, contractExecutionStates.DynamicCount);
    }
#endif
  }
}
