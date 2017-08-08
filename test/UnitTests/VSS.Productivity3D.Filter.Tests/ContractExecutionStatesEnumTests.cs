using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.ResultsHandling;

namespace VSS.Productivity3D.Filter.Tests
{
  [TestClass]
  public class ContractExecutionStatesEnumTests : ExecutorBaseTests
  {
    [TestMethod]
    public void DynamicAddwithOffsetTest()
    {
      var contractExecutionStatesEnum = serviceProvider.GetRequiredService<IErrorCodesProvider>();
      Assert.AreEqual(35, contractExecutionStatesEnum.DynamicCount);
      Assert.AreEqual("Invalid filterUid.", contractExecutionStatesEnum.FirstNameWithOffset(2));
      Assert.AreEqual("UpsertFilter failed. Unable to create persistant filter.",
        contractExecutionStatesEnum.FirstNameWithOffset(24));
    }
  }
}