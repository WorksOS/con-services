using System;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  public class ClientLeafSubGridTests
  {
    [Fact]
    public void Test_ClientLeafSubGridTests_SubGridCacheAssignationArraySize()
    {
      var arrayLength = ClientLeafSubGrid.SupportsAssignationFromCachedPreProcessedClientSubGrid.Length;
      var enumLength = Enum.GetNames(typeof(GridDataType)).Length;
      Assert.True(arrayLength == enumLength,
        $"SupportsAssignationFromCachedPreProcessedClientSubGrid has different length to GridDataType {arrayLength} vs {enumLength}");
    }
  }
}
