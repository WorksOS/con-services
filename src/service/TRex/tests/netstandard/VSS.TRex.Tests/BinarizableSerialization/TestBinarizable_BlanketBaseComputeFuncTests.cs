using System;
using System.Collections.Generic;
using VSS.TRex.GridFabric.ComputeFuncs;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  /// <summary>
  /// Scans all types derived from BaseComputeFunc that are not generic and exercises the IBinarizable
  /// read/write serialization for them
  /// </summary>
  public class TestBinarizable_BlanketBaseComputeFuncTests : TestBinarizable_ReflectionBasedTests
  {
    public static IEnumerable<object[]> GetTypes() => GetTypes<BaseComputeFunc>();

    [Theory]
    [MemberData(nameof(GetTypes))]
    public void Test_BlanketBaseComputeFuncTests(Type type)
    {
      Test_ToFromBinary_ReflectionBasedTests<BaseComputeFunc>(type);
    }
  }
}
