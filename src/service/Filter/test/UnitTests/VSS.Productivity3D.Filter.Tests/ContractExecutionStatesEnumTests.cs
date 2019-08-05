using Microsoft.Extensions.DependencyInjection;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using Xunit;

namespace VSS.Productivity3D.Filter.Tests
{
  public class ContractExecutionStatesEnumTests : ExecutorBaseTests
  {
    [Fact] 
    public void DynamicAddwithOffsetTest()
    {
      var filterErrorCodesProvider = serviceProvider.GetRequiredService<IErrorCodesProvider>();
    
      Assert.Equal("Invalid filterUid.", filterErrorCodesProvider.FirstNameWithOffset(2));
      Assert.Equal("UpsertFilter failed. Unable to create persistent filter.",
        filterErrorCodesProvider.FirstNameWithOffset(24));
    }
  }
}
