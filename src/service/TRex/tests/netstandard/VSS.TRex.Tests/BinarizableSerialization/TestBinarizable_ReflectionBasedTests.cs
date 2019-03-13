using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Apache.Ignite.Core.Binary;
using FluentAssertions;
using FluentAssertions.Common;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Common.Utilities;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class TestBinarizable_ReflectionBasedTests_Fixture : IDisposable
  {
    public TestBinarizable_ReflectionBasedTests_Fixture()
    {
      DIBuilder.New()
        .AddLogging()
        .Add(x => x.AddSingleton<IFilterSet>(factory => new FilterSet()))
        .Complete();
    }

    public void Dispose()
    {
      DIBuilder.Eject();
    }
  }

  public class TestBinarizable_ReflectionBasedTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    public static IEnumerable<object[]> GetTypes<T>() where T : class, IBinarizable
    {
      return TypesHelper
        .FindAllDerivedTypesInAllLoadedAssemblies<T>("VSS")
        .Where(x => !x.ContainsGenericParameters)
        .Where(x => !x.Implements(typeof(INonBinarizable)))
        .Where(x => !x.IsAbstract)
        .Select(x => new object[] { x })
        .ToList();
    }

    public void Test_ToFromBinary_ReflectionBasedTests<T>(Type type) where T : class, IBinarizable
    {
      // Create an instance of the type and exercise the IBinarizable serialization methods on the default object,
      // casting it to IBinarizable (which it must implement).

      var instance = Activator.CreateInstance(type) as T;
      instance.Should().NotBeNull();

      var writer = new TestBinaryWriter();
      instance.WriteBinary(writer);

      var reader = new TestBinaryReader(writer._stream.BaseStream as MemoryStream);
      var newInstance = Activator.CreateInstance(type) as T;
      newInstance.Should().NotBeNull();

      newInstance.ReadBinary(reader);

      // Check the two match. If there are no state members for the comparison BeEquivalent will throw 
      // a System.InvalidOperationException exception.Catch it and return success in this case
      try
      {
        newInstance.Should().BeEquivalentTo(instance, "Binarizable serialization/deserialization failed to produce correct result");
      }
      catch (InvalidOperationException e)
      {
        if (e.Message != "No members were found for comparison. Please specify some members to include in the comparison or choose a more meaningful assertion.")
          throw;
      }
    }
  }
}
