using System;
using System.Collections.Generic;
using System.Linq;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.GridFabric.Interfaces;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  /// <summary>
  /// Scans all types that implement IBinarizable and that are not covered by the base request, request argument,
  /// request response and compute function classes and exercises the IBinarizable read/write serialization for them
  /// </summary>
  public class TestBinarizable_BlanketGeneralIBinarizableTests : TestBinarizable_ReflectionBasedTests
  {
    public static IEnumerable<object[]> GetTypes()
    {
      return GetTypes<IBinarizable>()
        .Where(x => !typeof(BaseRequestResponse).IsAssignableFrom((Type)x[0]))
        .Where(x => !typeof(BaseRequestArgument).IsAssignableFrom((Type)x[0]))
        .Where(x => !typeof(BaseComputeFunc).IsAssignableFrom((Type)x[0]))
        .Where(x => !typeof(IBaseRequest).IsAssignableFrom((Type)x[0]))
        .ToList();
    }

    [Theory]
    [MemberData(nameof(GetTypes))]
    public void Test_BlanketBaseRequestResponseTests(Type type)
    {
      Test_ToFromBinary_ReflectionBasedTests<IBinarizable>(type);
    }
  }
}
