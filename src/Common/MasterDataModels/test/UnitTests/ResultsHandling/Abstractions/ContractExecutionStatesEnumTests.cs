using VSS.MasterData.Models.ResultHandling.Abstractions;
using Xunit;

namespace VSS.MasterData.Models.UnitTests.ResultsHandling.Abstractions
{
  public class ContractExecutionStatesEnumTests
  {
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(-1999)]
    [InlineData(-2015)]
    public void FirstNameWithOffset_Should_not_throw_When_input_is_not_a_defined_value(int index)
    {
      Assert.Equal(new ContractExecutionStatesEnum().FirstNameWithOffset(index),
        $"ERROR: '{2000 + index}' is not a defined value of {nameof(ContractExecutionStatesEnum)}");
    }

    [Theory]
    [InlineData(-2000, "ExecutedSuccessfully")]
    [InlineData(-2005, "AuthError")]
    [InlineData(-2007, "NoSubscription")]
    public void FirstNameWithOffset_Should_return_expected_string_When_input_is_valid(int index, string expectedResult)
    {
      Assert.Equal(expectedResult, new ContractExecutionStatesEnum().FirstNameWithOffset(index));
    }
  }
}
