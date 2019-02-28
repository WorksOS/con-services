using System;
using System.Collections.Generic;
using VSS.TRex.GridFabric.Arguments;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  /// <summary>
  /// Scans all types derived from BaseArgument that are not generic and exercises the IBinarizable
  /// read/write serialization for them
  /// </summary>
  public class TestBinarizable_BlanketBaseRequestArgumentTests : TestBinarizable_ReflectionBasedTests
  {
    public static IEnumerable<object[]> GetTypes() => GetTypes<BaseRequestArgument>();

    [Theory]
    [MemberData(nameof(GetTypes))]
    public void Test_BlanketBaseRequestArgumentTests(Type type)
    {
      base.Test_ToFromBinary_ReflectionBasedTests<BaseRequestArgument>(type);
    }
  }
}
