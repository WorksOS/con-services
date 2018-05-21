using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Filter.Tests
{
  [TestClass]
  public class ContractExecutionStatesEnumTests : ExecutorBaseTests
  {
    [TestMethod] 
    public void DynamicAddwithOffsetTest()
    {
      var filterErrorCodesProvider = serviceProvider.GetRequiredService<IErrorCodesProvider>();
    
      Assert.AreEqual("Invalid filterUid.", filterErrorCodesProvider.FirstNameWithOffset(2));
      Assert.AreEqual("UpsertFilter failed. Unable to create persistent filter.",
        filterErrorCodesProvider.FirstNameWithOffset(24));
    }
  }
}