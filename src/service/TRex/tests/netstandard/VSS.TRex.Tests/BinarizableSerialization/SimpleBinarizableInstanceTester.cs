using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Interfaces;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  /// <summary>
  /// Abstract away the details of serialising and deserialising an object implementing IFromToBinary...
  /// </summary>
  public static class SimpleBinarizableInstanceTester
  {
    /// <summary>
    /// Given an instance of a class to test serialise it to an Ignite IBinaryObject, then deserialise it to
    /// the source type, comparing the before and after versions for equality
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <param name="instance"></param>
    /// <param name="failureMsg"></param>
    public static void TestAClassBinarizableSerialization<T, U>(T instance, string failureMsg = "") where U : class, IFromToBinary, new() where T : TestBinarizable_Class<U>
    {
      var binObj = TestBinarizable_DefaultIgniteNode.GetIgnite().GetBinary().ToBinary<IBinaryObject>(instance);
      var result = binObj.Deserialize<T>();

      if (failureMsg != "")
        Assert.True(instance.member.Equals(result.member), $"{typeof(T).FullName}: {failureMsg}");
      else
        Assert.True(instance.member.Equals(result.member), $"{typeof(T).FullName} not the same after round trip serialisation");
    }

    /// <summary>
    /// Given an instance of a class to test serialise it to an Ignite IBinaryObject, then deserialise it to
    /// the source type, comparing the before and after versions for equality
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <param name="instance"></param>
    /// <param name="failureMsg"></param>
    public static void TestAnExtensionBinarizableSerialization<T, U>(T instance, string failureMsg = "") where U : class, new() where T : TestBinarizable_Extension<U>
    {
      var binObj = TestBinarizable_DefaultIgniteNode.GetIgnite().GetBinary().ToBinary<IBinaryObject>(instance);
      var result = binObj.Deserialize<T>();

      if (failureMsg != "")
        Assert.True(instance.member.Equals(result.member), $"{typeof(U).FullName}: {failureMsg}");
      else
        Assert.True(instance.member.Equals(result.member), $"{typeof(U).FullName} not the same after round trip serialisation");
    }

    /// <summary>
    /// Given an instance of a class to test serialise it to an Ignite IBinaryObject, then deserialise it to
    /// the source type, comparing the before and after versions for equality
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <param name="instance"></param>
    /// <param name="failureMsg"></param>
    public static void TestAStructBinarizableSerialization<T, U>(T instance, string failureMsg = "") where U : struct, IFromToBinary where T : TestBinarizable_Struct<U>
    {
      var binObj = TestBinarizable_DefaultIgniteNode.GetIgnite().GetBinary().ToBinary<IBinaryObject>(instance);
      var result = binObj.Deserialize<T>();

      if (failureMsg != "")
        Assert.True(instance.member.Equals(result.member), $"{typeof(U).FullName}: {failureMsg}");
      else
        Assert.True(instance.member.Equals(result.member), $"{typeof(U).FullName} not the same after round trip serialisation");
    }

    /// <summary>
    /// Tests a class by instantiating a default instance and wrapping it in an IBinarizable implementing class which
    /// then exercises the IFromToBinary TRex interface on that class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void TestExtension<T>(string failureMsg = "") where T: class, new()
    {
      TestAnExtensionBinarizableSerialization<TestBinarizable_Extension<T>, T>(new TestBinarizable_Extension<T>(), failureMsg);
    }

    /// <summary>
    /// Tests a class by taking a pre-built instance and wrapping it in an IBinarizable implementing class which
    /// then exercises the IFromToBinary TRex interface on that class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="testMember"></param>
    /// <param name="failureMsg"></param>
    public static void TestExtension<T>(T testMember, string failureMsg = "") where T : class, new()
    {
      TestAnExtensionBinarizableSerialization<TestBinarizable_Extension<T>, T>(new TestBinarizable_Extension<T> {member = testMember}, failureMsg);
    }

    /// <summary>
    /// Tests a class by instantiating a default instance and wrapping it in an IBinarizable implementing class which
    /// then exercises the IFromToBinary TRex interface on that class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void TestClass<T>(string failureMsg = "") where T : class, IFromToBinary, new()
    {
      TestAClassBinarizableSerialization<TestBinarizable_Class<T>, T>(new TestBinarizable_Class<T>(), failureMsg);
    }

    /// <summary>
    /// Tests a class by instantiating a default instance and wrapping it in an IBinarizable implementing class which
    /// then exercises the IFromToBinary TRex interface on that class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void TestNonBinarizableClass<T>() where T : class, IFromToBinary, new()
    {
      Assert.Throws<TRexNonBinarizableException>(() => 
        TestBinarizable_DefaultIgniteNode.GetIgnite().GetBinary().ToBinary<IBinaryObject>(new TestBinarizable_Class<T>()));
    }
    
    /// <summary>
    /// Tests a class by instantiating a default instance and wrapping it in an IBinarizable implementing class which
    /// then exercises the IFromToBinary TRex interface on that class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void TestClass<T>(T testMember, string failureMsg = "") where T : class, IFromToBinary, new()
    {
      TestAClassBinarizableSerialization<TestBinarizable_Class<T>, T>(new TestBinarizable_Class<T> { member = testMember }, failureMsg);
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
