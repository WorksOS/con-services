using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Common.ResultHandling
{
  public class TRexResult : ContractExecutionStatesEnum
  {
    public static void AddErrorMessages(ContractExecutionStatesEnum contractExecutionStates)
    {
      contractExecutionStates.DynamicAddwithOffset("OK", 0);
    }

    public static void AddTagProcessorErrorMessages(ContractExecutionStatesEnum contractExecutionStates)
    {
      contractExecutionStates.DynamicAddwithOffset("Tagfile OK",0);
    }

    public static void AddDesignProfileErrorMessages(ContractExecutionStatesEnum contractExecutionStates)
    {
      contractExecutionStates.DynamicAddwithOffset("Design Profiler OK", 0);
    }

    public static void AddExportErrorMessages(ContractExecutionStatesEnum contractExecutionStates)
    {
      contractExecutionStates.DynamicAddwithOffset("Export OK",0);
    }

    public static void AddMissingTargetDataResultMessages(ContractExecutionStatesEnum contractExecutionStates)
    {
      contractExecutionStates.DynamicAddwithOffset("No problems due to missing target data could still be no data however", 0);
    }

    public static void AddCoordinateResultErrorMessages(ContractExecutionStatesEnum contractExecutionStates)
    {
      contractExecutionStates.DynamicAddwithOffset("No error", 0);
    }
  }
}
