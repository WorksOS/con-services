using System;
using System.Collections.Generic;
using VSS.TRex.GridFabric.Interfaces;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  /// <summary>
  /// Scans all types derived from BaseRequestResponse that are not generic and exercises the IBinarizable
  /// read/write serialization for them
  /// </summary>
  public class TestBinarizable_BlanketBaseRequestTests : TestBinarizable_ReflectionBasedTests
  {
    public static IEnumerable<object[]> GetTypes() => GetTypes<IBaseRequest>();

    [Theory]
    [MemberData(nameof(GetTypes))]
    public void Test_BlanketBaseRequestResponseTests(Type type)
    {
      Test_ToFromBinary_ReflectionBasedTests<IBaseRequest>(type);
    }
  }
}
