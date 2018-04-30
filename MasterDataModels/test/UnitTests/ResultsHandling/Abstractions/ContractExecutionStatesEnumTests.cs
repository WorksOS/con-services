using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Models.UnitTests.ResultsHandling.Abstractions
{
  [TestClass]
  public class ContractExecutionStatesEnumTests
  {
    [TestMethod]
    [DataRow(-1)]
    [DataRow(0)]
    [DataRow(2)]
    [DataRow(-1999)]
    [DataRow(-2015)]
    public void FirstNameWithOffset_Should_throw_When_input_is_not_a_defined_value(int index)
    {
      var contractExecutionStates = new ContractExecutionStatesEnum();

      Assert.ThrowsException<ArgumentException>(
        () => contractExecutionStates.FirstNameWithOffset(index),
        $"'{2000 + index}' is not a defined value of ContractExecutionStatesEnum");
    }

    [TestMethod]
    [DataRow(-2000, "ExecutedSuccessfully")]
    [DataRow(-2005, "AuthError")]
    [DataRow(-2007, "NoSubscription")]
    public void FirstNameWithOffset_Should_return_expected_string_When_input_is_valid(int index, string expectedResult)
    {
      var contractExecutionStates = new ContractExecutionStatesEnum();

      Assert.AreEqual(expectedResult, contractExecutionStates.FirstNameWithOffset(index));
    }
  }
}