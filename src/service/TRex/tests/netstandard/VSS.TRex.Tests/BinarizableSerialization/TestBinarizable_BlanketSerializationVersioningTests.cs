using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Apache.Ignite.Core.Binary;
using FluentAssertions;
using FluentAssertions.Common;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.CoordinateSystems.GridFabric.Arguments;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.TAGFiles.Models;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public static class LocalExtensions
  {
    /// <summary>
    /// Recursively scans a type and its inheritance tree looking for
    /// </summary>
    /// <param name="type"></param>
    /// <param name="flags"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static IEnumerable<FieldInfo> EnumerateFields(this Type type, BindingFlags flags, string fieldName)
    {
      var thisFields = type.GetFields(flags | BindingFlags.DeclaredOnly).Where(x => x.Name == fieldName).ToArray();
      if (type.BaseType == null)
        return thisFields;

      var baseFields = type.BaseType.EnumerateFields(flags, fieldName);
      return baseFields == null ? thisFields : thisFields.Concat(baseFields);
    }
  }

  /// <summary>
  /// The intent of this test class is to enforce consistent versioning treatment of all argument and response
  /// types quantities that implement
  /// IBinarizable serialization for over-the-wire transmission over the Ignite messaging/compute etc fabrics
  /// </summary>
  public class TestBinarizable_BlanketSerializationVersioningTests
  {
    private const string VERSION_NUMBER = "VERSION_NUMBER";

    public static bool TypeIsInteresting(Type x)
    {
      const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

      return (typeof(BaseRequestArgument).IsAssignableFrom(x) || typeof(BaseRequestResponse).IsAssignableFrom(x)) &&
             x.Implements(typeof(IBinarizable)) &&
             x.Implements(typeof(IFromToBinary)) &&
             !x.Implements(typeof(INonBinarizable)) &&
             x.EnumerateFields(flags, VERSION_NUMBER).Any();
    }

    public static IEnumerable<object[]> GetTypes()
    {
      // Select all request and response derived objects in the VSS.* namespaces that implement
      // IFromToBinary but not INonBinarizable. These will be interrogated for serialization versioning
      // support, and then tested for it
      return AppDomain.CurrentDomain
        .GetAssemblies()
        .Where(x => x.FullName.StartsWith("VSS", StringComparison.OrdinalIgnoreCase))
        .SelectMany(x => x.GetTypes())
        .Where(TypeIsInteresting)
        .Select(x => new object[] { x });
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
      return GetMethod(type, methodName, parameterTypes, declaredInTypeOnly) != (MethodInfo)null;
    }

    [Fact]
    public void TestEnumeration()
    {
      var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

      var result = typeof(BaseApplicationServiceRequestArgument).EnumerateFields(flags, VERSION_NUMBER).ToArray();
      result.Should().NotBeNull();

      var result2 = TypeIsInteresting(typeof(BaseApplicationServiceRequestArgument));
      result2.Should().Be(true);

      var result3 = typeof(AddCoordinateSystemArgument).EnumerateFields(flags, VERSION_NUMBER).ToArray();
      result3.Should().NotBeNull();

      var result4 = TypeIsInteresting(typeof(AddCoordinateSystemArgument));
      result4.Should().Be(true);

      var type = typeof(AddCoordinateSystemArgument);
      var typeHasReadWriteMembers = HasMethod(type, "FromBinary", new[] {typeof(IBinaryRawReader)}, true) &&
                                    HasMethod(type, "ToBinary", new[] {typeof(IBinaryRawWriter)}, true);
      typeHasReadWriteMembers.Should().BeTrue();
    }

    [Fact]
    public void Test_GetTypes()
    {
      var result = GetTypes();
      result.Should().NotBeNull();
    }

    private void PerformSerializationVersionFailTestOnField(FieldInfo field, Type type)
    {
      uint versionNumber = field.FieldType.FullName == "System.Byte" ? (byte)field.GetValue(null)
        : field.FieldType.FullName == "System.UInt32" ? (ushort)field.GetValue(null)
        : field.FieldType.FullName == "System.UInt16" ? (uint)field.GetValue(null)
        : uint.MaxValue;

      var expectedVersions = new[] { versionNumber };

      var writer = new TestBinaryWriter();
      writer.WriteByte((byte)(versionNumber + 1));
      var reader = new TestBinaryReader(writer._stream.BaseStream as MemoryStream);

      var item = Activator.CreateInstance(type) as IBinarizable;

      item.Should().NotBeNull();

      Action act = () => item.ReadBinary(reader);
      act.Should().Throw<TRexSerializationVersionException>().WithMessage(TRexSerializationVersionException.ErrorMessage(expectedVersions, SegmentRetirementQueueItem.VERSION_NUMBER + 1));
    }

    [Theory]
    [MemberData(nameof(GetTypes))]
    public void FromToBinaryVersioning(Type type)
    {
      // Determine if the class implements the IFromToBinary FromBinary/ToBinary counterparts of the IBinarizable interface
      var typeHasReadWriteMembers = HasMethod(type, "FromBinary", new[] {typeof(IBinaryRawReader)}, true) &&
                                    HasMethod(type, "ToBinary", new[] {typeof(IBinaryRawWriter)}, true);

      // The type provided either has a version number declared in its own scope, or is derived 
      // from one or more classes that do so.
      // For each of the former it should be possible to write a version number into the start of
      // an IFromToBinary based serialization implementing Ignite IBinarizable serialization).
      // For each of the latter we should find the inherited member that implement the version 
      // number and perform the same check against the base class serialization (ie: So the the base
      // serialization is exercised through the type instance

      var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;
      var field = type.GetTypeInfo().GetField(VERSION_NUMBER, flags);

      if (field != null)
      {
        typeHasReadWriteMembers.Should().BeTrue($"because class {type.FullName} implements IBinarizable, has a defined VERSION_NUMBER but no serialization logic which is suspicious");

        // The type defines the Version number and exercise the test to trigger the version failure
        PerformSerializationVersionFailTestOnField(field, type);
      }
      else
      {
        typeHasReadWriteMembers.Should().BeFalse($"because class {type.FullName} implements IBinarizable, has no defined VERSION_NUMBER but does implement serialization logic which is suspicious");

        // Find the base type that defines the version number and exercise the test to trigger the version failure
        // Todo: Need to determine how strict we want to be...
        //PerformSerializationVersionFailTestOnField(field, type);
      }
    }
  }
}
