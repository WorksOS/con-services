using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Common;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Common.Records;
using VSS.TRex.Common.Utilities.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinaryReaderWriter
{
  /// <summary>
  /// This test class finds all types that implement IBinaryReaderWriter and test they can serialise
  /// and deserialise a default instance of the type
  /// </summary>
  public class IBinaryReaderWriter_Mimic_Tests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    public static bool TypeIsInteresting(Type x)
    {
      return !x.IsInterface && 
             !x.Implements(typeof(IBinaryReaderWriter)) &&
             !x.Implements(typeof(INonBinaryReaderWriterMimicable)) &&             
             HasMethod(x, "Write", new[] { typeof(BinaryWriter) }, true) &&
             HasMethod(x, "Read", new[] { typeof(BinaryReader) }, true);
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

    private static MethodInfo GetMethod(
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

    private static bool HasMethod(
      Type type,
      string methodName,
      IEnumerable<Type> parameterTypes,
      bool declaredInTypeOnly)
    {
      return GetMethod(type, methodName, parameterTypes, declaredInTypeOnly) != (MethodInfo) null;
    }

    [Fact]
    public void Test_TypeIsIteresting()
    {
      var result = TypeIsInteresting(typeof(PassCountRangeRecord));
      result.Should().BeTrue();
    }

    [Fact]
    public void Test_GetTypes()
    {
      var result = GetTypes();
      result.Should().NotBeNull();
    }

    private void TestStandardWrite(Type type)
    {
      var instance = Activator.CreateInstance(type);
      var ms = new MemoryStream();
      var bw = new BinaryWriter(ms);

      instance.GetType().InvokeMember("Write", BindingFlags.InvokeMethod, null, instance, new[] {bw});

      ms.Position = 0;
      var br = new BinaryReader(ms);
      var instance2 = Activator.CreateInstance(type);

      instance2.GetType().InvokeMember("Read", BindingFlags.InvokeMethod, null, instance2, new[] { br });
      instance.Should().BeEquivalentTo(instance2, options => options.RespectingRuntimeTypes().IgnoringCyclicReferences());
    }

    private void TestBufferedWrite(Type type)
    {
      var instance = Activator.CreateInstance(type);
      var ms = new MemoryStream();
      var bw = new BinaryWriter(ms);

      instance.GetType().InvokeMember("Write", BindingFlags.InvokeMethod, null, instance, new object[] { bw, new byte[10000] });

      ms.Position = 0;
      var br = new BinaryReader(ms);
      var instance2 = Activator.CreateInstance(type);

      instance2.GetType().InvokeMember("Read", BindingFlags.InvokeMethod, null, instance2, new[] { br });
      instance.Should().BeEquivalentTo(instance2, options => options.RespectingRuntimeTypes().IgnoringCyclicReferences());
    }

    [Theory]
    [MemberData(nameof(GetTypes))]
    public void Serialization_StandardWrite(Type type)
    {
      // Determine if the class implements the BinaryReaderWriter Read/Write members of the IBinaryReaderWriter interface
      var typeHasReadWriteMembers = HasMethod(type, "Write", new[] {typeof(BinaryWriter) }, true) &&
                                    HasMethod(type, "Read", new[] {typeof(BinaryReader) }, true);
      var typeHasBufferedWriterMember = HasMethod(type, "Write", new[] {typeof(BinaryWriter), typeof(byte[])}, true);

      if (typeHasReadWriteMembers)
      {
        typeHasReadWriteMembers.Should().BeTrue($"because class {type.FullName} implements IBinaryReaderWriter but no serialization logic which is suspicious");

        TestStandardWrite(type);
      }
      else
      {
        typeHasReadWriteMembers.Should().BeFalse($"because class {type.FullName} implements IBinaryReaderWriter but does implement serialization logic which is suspicious");
      }

      if (typeHasBufferedWriterMember)
      {
        TestBufferedWrite(type);
      }
    }
  }
}
