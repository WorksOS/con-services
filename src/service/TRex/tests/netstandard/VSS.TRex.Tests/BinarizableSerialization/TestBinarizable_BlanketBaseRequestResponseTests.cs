using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions.Common;
using VSS.TRex.Common;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Common.Utilities;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  /// <summary>
  /// Scans all types derived from BaseRequestResponse that are not generic and exercises the IBinarizable
  /// read/write serialization for them
  /// </summary>
  public class TestBinarizable_BlanketBaseRequestResponseTests : TestBinarizable_ReflectionBasedTests
  {
    public static IEnumerable<object[]> GetTypes() => GetTypes<BaseRequestResponse>();

    [Theory]
    [MemberData(nameof(GetTypes))]
    public void Test_BlanketBaseRequestResponseTests(Type type)
    {
      base.Test_ToFromBinary_ReflectionBasedTests<BaseRequestResponse>(type);
    }
  }
}
