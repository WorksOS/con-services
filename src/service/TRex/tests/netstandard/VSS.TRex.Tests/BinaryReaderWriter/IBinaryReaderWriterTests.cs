using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Common;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.BinaryReaderWriter
{
  /// <summary>
  /// This test class finds all types that implement IBinaryReaderWriter and test they can serialise
  /// and deserialise a default instance of the type
  /// </summary>
  public class IBinaryReaderWriterTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    public static bool TypeIsInteresting(Type x)
    {
      return !x.IsInterface && x.Implements(typeof(IBinaryReaderWriter));
    }

    public static IEnumerable<object[]> GetTypes()
    {
      // Select all IBinaryReaderWriter implementing objects in the VSS.* namespaces.
      // These will be tested for serialization support.
      return AppDomain.CurrentDomain
        .GetAssemblies()
        .Where(x => x.FullName.StartsWith("VSS", StringComparison.OrdinalIgnoreCase))
        .SelectMany(x => x.GetTypes())
        .Where(TypeIsInteresting)
        .Select(x => new object[] {x});
    }

    private MethodInfo GetMethod(
      Type type,
      string methodName,
      IEnumerable<Type> parameterTypes,
      bool declaredInTypeOnly)
    {
      var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
      if (declaredInTypeOnly)
        flags |= BindingFlags.DeclaredOnly;

      return type.GetMethods(flags).SingleOrDefault(m =>
      {
        if (m.Name == methodName)
          return m.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes);
        return false;
      });
    }

    private bool HasMethod(
      Type type,
      string methodName,
      IEnumerable<Type> parameterTypes,
      bool declaredInTypeOnly)
    {
      return GetMethod(type, methodName, parameterTypes, declaredInTypeOnly) != (MethodInfo) null;
    }

    [Fact]
    public void Test_GetTypes()
    {
      var result = GetTypes();
      result.Should().NotBeNull();
    }

    private void TestStandardWrite(Type type)
    {
      var instance = Activator.CreateInstance(type) as IBinaryReaderWriter;
      var ms = new MemoryStream(Consts.TREX_DEFAULT_MEMORY_STREAM_CAPACITY_ON_CREATION);
      var bw = new BinaryWriter(ms);
      instance.Write(bw);

      ms.Position = 0;
      var br = new BinaryReader(ms);
      var instance2 = Activator.CreateInstance(type) as IBinaryReaderWriter;
      instance2.Read(br);

      instance.Should().BeEquivalentTo(instance2, options => options.RespectingRuntimeTypes().IgnoringCyclicReferences());
    }

    private void TestBufferedWrite(Type type)
    {
      var instance = Activator.CreateInstance(type) as IBinaryReaderWriter;
      var ms = new MemoryStream(Consts.TREX_DEFAULT_MEMORY_STREAM_CAPACITY_ON_CREATION);
      var bw = new BinaryWriter(ms);
      instance.Write(bw, new byte[10000]);

      ms.Position = 0;
      var br = new BinaryReader(ms);
      var instance2 = Activator.CreateInstance(type) as IBinaryReaderWriter;
      instance2.Read(br);

      instance.Should().BeEquivalentTo(instance2, options => options.RespectingRuntimeTypes().IgnoringCyclicReferences());
    }

    [Theory]
    [MemberData(nameof(GetTypes))]
    public void Serialization_StandardWrite(Type type)
    {
      // Determine if the class implements the BinaryReaderWriter Read/Write members of the IBinaryReaderWriter interface
      var typeHasReadWriteMembers = HasMethod(type, "Write", new[] {typeof(BinaryWriter) }, true) &&
                                    HasMethod(type, "Write", new[] {typeof(BinaryWriter), typeof(byte[])}, true) &&
                                    HasMethod(type, "Read", new[] {typeof(BinaryReader)}, true);

      if (typeHasReadWriteMembers)
      {
        typeHasReadWriteMembers.Should().BeTrue($"because class {type.FullName} implements IBinaryReaderWriter but no serialization logic which is suspicious");

        TestStandardWrite(type);
        TestBufferedWrite(type);
      }
      else
      {
        typeHasReadWriteMembers.Should().BeFalse($"because class {type.FullName} implements IBinaryReaderWriter but does implement serialization logic which is suspicious");

        // Find the base type that defines the version number and exercise the test to trigger the version failure
        // Todo: Need to determine how strict we want to be...
      }
    }
  }
}
