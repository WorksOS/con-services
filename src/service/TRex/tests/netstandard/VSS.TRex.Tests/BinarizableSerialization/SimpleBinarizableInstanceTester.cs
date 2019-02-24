using System.IO;
using Apache.Ignite.Core.Binary;
using FluentAssertions;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Interfaces;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  /// <summary>
  /// Abstract away the details of serializing and deserializing an object implementing IFromToBinary...
  /// </summary>
  public static class SimpleBinarizableInstanceTester
  {
    /// <summary>
    /// Given an instance of a class to test serialise it to an Ignite IBinaryObject, then deserialize it to
    /// the source type, comparing the before and after versions for equality
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <param name="instance"></param>
    /// <param name="failureMsg"></param>
    public static T TestAClassFromToBinarySerialization<T, U>(T instance, string failureMsg = "") where U : class, IFromToBinary, new() where T : TestFromToBinary_Class<U>, new()
    {
      var bw = new TestBinaryWriter();
      instance.member.ToBinary(bw);

      var br = new TestBinaryReader(bw._stream.BaseStream as MemoryStream);
      var result = new T();

      result.member.FromBinary(br);

      if (failureMsg != "")
        result.member.Should().BeEquivalentTo(instance.member, $"{typeof(T).FullName}: {failureMsg}");
      else
        result.member.Should().BeEquivalentTo(instance.member, $"{typeof(T).FullName}: not the same after round trip serialisation");

      return result;
    }

    /// <summary>
    /// Given an instance of a class to test serialise it to an Ignite IBinaryObject, then deserialize it to
    /// the source type, comparing the before and after versions for equality
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <param name="instance"></param>
    /// <param name="failureMsg"></param>
    public static T TestAClassBinarizableSerialization<T, U>(T instance, string failureMsg = "") where U : class, IFromToBinary, IBinarizable, new() where T : TestBinarizable_Class<U>, new()
    {
      var bw = new TestBinaryWriter();
      instance.member.WriteBinary(bw); //ToBinary(bw);

      var br = new TestBinaryReader(bw._stream.BaseStream as MemoryStream);
      var result = new T();

      result.member.ReadBinary(br); //FromBinary(br);
      
      if (failureMsg != "")
        result.member.Should().BeEquivalentTo(instance.member, $"{typeof(T).FullName}: {failureMsg}");
      else
        result.member.Should().BeEquivalentTo(instance.member, $"{typeof(T).FullName}: not the same after round trip serialisation");

      return result;
    }

    /// <summary>
    /// Given an instance of a class to test serialise it to an Ignite IBinaryObject, then deserialize it to
    /// the source type, comparing the before and after versions for equality
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <param name="instance"></param>
    /// <param name="failureMsg"></param>
    public static T TestAnExtensionBinarizableSerialization<T, U>(T instance, string failureMsg = "") where U : class, new() where T : TestBinarizable_Extension<U>, new()
    {
      var bw = new TestBinaryWriter();
      instance.WriteBinary(bw);

      var br = new TestBinaryReader(bw._stream.BaseStream as MemoryStream);
      var result = new T();

      result.ReadBinary(br);

      if (failureMsg != "")
        result.member.Should().BeEquivalentTo(instance.member, $"{typeof(T).FullName}: {failureMsg}");
      else
        result.member.Should().BeEquivalentTo(instance.member, $"{typeof(T).FullName}: not the same after round trip serialisation");

      return result;
    }

    /// <summary>
    /// Given an instance of a class to test serialise it to an Ignite IBinaryObject, then deserialize it to
    /// the source type, comparing the before and after versions for equality
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <param name="instance"></param>
    /// <param name="failureMsg"></param>
    public static T TestAStructBinarizableSerialization<T, U>(T instance, string failureMsg = "") where U : struct, IFromToBinary where T : TestBinarizable_Struct<U>, new()
    {
      var bw = new TestBinaryWriter();
      instance.member.ToBinary(bw);

      var br = new TestBinaryReader(bw._stream.BaseStream as MemoryStream);
      var result = new T();

      result.member.FromBinary(br);

      if (failureMsg != "")
        result.member.Should().BeEquivalentTo(instance.member, $"{typeof(T).FullName}: {failureMsg}");
      else
        result.member.Should().BeEquivalentTo(instance.member, $"{typeof(T).FullName}: not the same after round trip serialisation");

      return result;
    }

    /// <summary>
    /// Tests a class by instantiating a default instance and wrapping it in an IBinarizable implementing class which
    /// then exercises the IFromToBinary TRex interface on that class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static TestBinarizable_Extension<T> TestExtension<T>(string failureMsg = "") where T: class, new()
    {
      return TestAnExtensionBinarizableSerialization<TestBinarizable_Extension<T>, T>(new TestBinarizable_Extension<T>(), failureMsg);
    }

    /// <summary>
    /// Tests a class by taking a pre-built instance and wrapping it in an IBinarizable implementing class which
    /// then exercises the IFromToBinary TRex interface on that class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="testMember"></param>
    /// <param name="failureMsg"></param>
    public static TestBinarizable_Extension<T> TestExtension<T>(T testMember, string failureMsg = "") where T : class, new()
    {
      return TestAnExtensionBinarizableSerialization<TestBinarizable_Extension<T>, T>(new TestBinarizable_Extension<T> {member = testMember}, failureMsg);
    }

    /// <summary>
    /// Tests a class by instantiating a default instance and wrapping it in an IBinarizable implementing class which
    /// then exercises the IFromToBinary TRex interface on that class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void TestNonBinarizableClass<T>() where T : class, IFromToBinary, new()
    {
      Assert.Throws<TRexNonBinarizableException>(() =>
      {
        var instance = new TestBinarizable_Class<T>();
        var bw = new TestBinaryWriter();
        instance.WriteBinary(bw);
      });
    }

    /// <summary>
    /// Tests a class by instantiating a default instance and wrapping it in an IBinarizable implementing class which
    /// then exercises the IFromToBinary TRex interface on that class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static TestFromToBinary_Class<T> TestClassEx<T>(string failureMsg = "") where T : class, IFromToBinary, new()
    {
      return TestAClassFromToBinarySerialization<TestFromToBinary_Class<T>, T>(new TestFromToBinary_Class<T>(), failureMsg);
    }
    
    /// <summary>
    /// Tests a class by instantiating a default instance and wrapping it in an IBinarizable implementing class which
    /// then exercises the IFromToBinary TRex interface on that class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static TestFromToBinary_Class<T> TestClassEx<T>(T testMember, string failureMsg = "") where T : class, IFromToBinary, new()
    {
      return TestAClassFromToBinarySerialization<TestFromToBinary_Class<T>, T>(new TestFromToBinary_Class<T> { member = testMember }, failureMsg);
    }

    /// <summary>
    /// Tests a class by instantiating a default instance and wrapping it in an IBinarizable implementing class which
    /// then exercises the IFromToBinary TRex interface on that class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static TestBinarizable_Class<T> TestClass<T>(string failureMsg = "") where T : class, IFromToBinary, IBinarizable, new()
    {
      return TestAClassBinarizableSerialization<TestBinarizable_Class<T>, T>(new TestBinarizable_Class<T>(), failureMsg);
    }

    /// <summary>
    /// Tests a class by instantiating a default instance and wrapping it in an IBinarizable implementing class which
    /// then exercises the IFromToBinary TRex interface on that class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static TestBinarizable_Class<T> TestClass<T>(T testMember, string failureMsg = "") where T : class, IFromToBinary, IBinarizable, new()
    {
      return TestAClassBinarizableSerialization<TestBinarizable_Class<T>, T>(new TestBinarizable_Class<T> { member = testMember }, failureMsg);
    }

    /// <summary>
    /// Tests a class by instantiating a default instance and wrapping it in an IBinarizable implementing class which
    /// then exercises the IFromToBinary TRex interface on that class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void TestStruct<T>(string failureMsg = "") where T : struct, IFromToBinary
    {
      TestAStructBinarizableSerialization<TestBinarizable_Struct<T>, T>(new TestBinarizable_Struct<T>(), failureMsg);
    }

    /// <summary>
    /// Tests a class by instantiating a default instance and wrapping it in an IBinarizable implementing class which
    /// then exercises the IFromToBinary TRex interface on that class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void TestStruct<T>(T testMember, string failureMsg = "") where T : struct, IFromToBinary
    {
      TestAStructBinarizableSerialization<TestBinarizable_Struct<T>, T>(new TestBinarizable_Struct<T> { member = testMember }, failureMsg);
    }
  }
}
