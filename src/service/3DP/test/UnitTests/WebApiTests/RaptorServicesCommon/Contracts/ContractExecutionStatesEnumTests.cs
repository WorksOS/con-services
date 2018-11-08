using DesignProfilerDecls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TAGProcServiceDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApiTests.RaptorServicesCommon.Contracts
{
  [TestClass]
  public class ContractExecutionStatesEnumTests
  {

    [TestMethod]
    public void DynamicAddwithOffsetTest()
    {
      var contractExecutionStates = new ContractExecutionStatesEnum();
      contractExecutionStates.DynamicAddwithOffset("Tag processing Successfull", (int)TTAGProcServerProcessResult.tpsprOK);
      contractExecutionStates.DynamicAddwithOffset("Unknown error", (int)TTAGProcServerProcessResult.tpsprUnknown);
      contractExecutionStates.DynamicAddwithOffset(
        "OnSubmissionBase. Connection Failure.",
        (int)TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure);

      Assert.AreEqual(3, contractExecutionStates.DynamicCount);
      Assert.AreEqual(
        "OnSubmissionBase. Connection Failure.",
        contractExecutionStates.FirstNameWithOffset((int)TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure));

      Assert.AreEqual(
        (int)TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure + 2000,
        contractExecutionStates.GetErrorNumberwithOffset(
          (int)TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure));

      contractExecutionStates.ClearDynamic();
      Assert.AreEqual(0, contractExecutionStates.DynamicCount);
    }

    [TestMethod]
    public void DynamicAddTwoSetsTest()
    {
      var contractExecutionStates = new ContractExecutionStatesEnum();
      contractExecutionStates.DynamicAddwithOffset("Tag processing Successfull", (int)TTAGProcServerProcessResult.tpsprOK);
      contractExecutionStates.DynamicAddwithOffset("Unknown error", (int)TTAGProcServerProcessResult.tpsprUnknown);
      contractExecutionStates.DynamicAddwithOffset(
        "OnSubmissionBase. Connection Failure.",
        (int)TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure);

      contractExecutionStates.DynamicAddwithOffset("OK", (int)TDesignProfilerRequestResult.dppiOK);
      contractExecutionStates.DynamicAddwithOffset("Unknown Error", (int)TDesignProfilerRequestResult.dppiUnknownError);
      contractExecutionStates.DynamicAddwithOffset("Could Not Connect To Server", (int)TDesignProfilerRequestResult.dppiCouldNotConnectToServer);
      contractExecutionStates.DynamicAddwithOffset("Failed To Convert Client WGS Coords", (int)TDesignProfilerRequestResult.dppiFailedToConvertClientWGSCoords);

      Assert.AreEqual(7, contractExecutionStates.DynamicCount);
      Assert.AreEqual(
        "OnSubmissionBase. Connection Failure.",
        contractExecutionStates.FirstNameWithOffset((int)TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure));

      Assert.AreEqual(
        (int)TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure + 2000,
        contractExecutionStates.GetErrorNumberwithOffset(
          (int)TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure));

      Assert.AreEqual("Failed To Convert Client WGS Coords", contractExecutionStates.FirstNameWithOffset((int)TDesignProfilerRequestResult.dppiFailedToConvertClientWGSCoords));
      Assert.AreEqual((int)TDesignProfilerRequestResult.dppiFailedToConvertClientWGSCoords + 2000,
        contractExecutionStates.GetErrorNumberwithOffset(
          (int)TDesignProfilerRequestResult.dppiFailedToConvertClientWGSCoords));

      contractExecutionStates.ClearDynamic();
      Assert.AreEqual(0, contractExecutionStates.DynamicCount);
    }
  }
}