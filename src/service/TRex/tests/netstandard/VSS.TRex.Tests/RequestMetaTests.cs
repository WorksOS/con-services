using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.GridFabric.Interfaces;
using Xunit;

namespace VSS.TRex.Tests
{
  /// <summary>
  /// The intention of these reflection based tests is to ensure that every request based class derived from BaseRequest is
  /// represented by a unit tests declaring they cover the requests n question. 
  /// </summary>
  public class RequestMetaTests
  {
    public static IEnumerable<object[]> GetTypes()
    {
      return TypesHelper
        .FindAllDerivedTypesInAllLoadedAssemblies<IBaseRequest>("VSS")
        .Where(x => !x.IsAbstract)
        .Select(x => new object[] {x});
    }

    private IEnumerable<Type> FindClassesCoveringType(Type type)
    {
      return AppDomain.CurrentDomain
          .GetAssemblies()
          .Where(x => x.FullName.StartsWith("VSS", StringComparison.OrdinalIgnoreCase))
          .SelectMany(x => x.GetTypes())
          .Where(y => y.GetCustomAttributes(typeof(UnitTestCoveredRequestAttribute), true)
                       .Cast<UnitTestCoveredRequestAttribute>().Any(x => x.RequestType == type));         
    }

    [Fact]
    public void TestGetClassesCoveringAttribute()
    {
      var x = FindClassesCoveringType(typeof(DesignElevationSpotRequest));

      x.Should().NotBeNull();
      x.Count().Should().BeGreaterOrEqualTo(1);
    }

    [Theory]
    [MemberData(nameof(GetTypes))]
    public void CheckRequest(Type type)
    {
      // Get the set of set class that advertise they cover the type in question
      var covered = FindClassesCoveringType(type);

      // All requests need at least one unit test class that covers them
      (covered?.Count() ?? 0).Should().BeGreaterOrEqualTo(1, $"Because request type {type} is not covered by any unite tests attributed with UnitTestCoveredRequestAttribute");
    }
  }
}
