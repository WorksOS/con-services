using VSS.MasterData.Models.ResultHandling.Abstractions;
using Xunit;

namespace VSS.MasterData.ProjectTests.Extensions
{
  public static class ContractExecutionResultExtensions
  {
    public static void IsSuccessResponse(this ContractExecutionResult contractExecutionResult) =>
      contractExecutionResult.ShouldBe(ContractExecutionStatesEnum.ExecutedSuccessfully, ContractExecutionResult.DefaultMessage);

    public static void ShouldBe(this ContractExecutionResult contractExecutionResult, int returnCode, string message)
    {
      Assert.Equal(returnCode, contractExecutionResult.Code);
      Assert.Equal(message, contractExecutionResult.Message);
    }
  }
}
