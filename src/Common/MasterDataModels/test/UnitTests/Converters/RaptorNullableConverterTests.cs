using System;
using System.Collections.Generic;
using Moq;
using Newtonsoft.Json;
using VSS.MasterData.Models.Converters;
using Xunit;

namespace VSS.MasterData.Models.UnitTests.Converters
{
  public class RaptorNullableConverterTests
  {
    [Fact]
    public void EnsureException()
    {
      // Cannot read values, must throw an exception
      var converter = new RaptorNullableConverter();
      Assert.False(converter.CanRead);
      Assert.Throws<NotSupportedException>(() =>
      {
        converter.ReadJson(null, null, null, null);
      });
    }

    [Fact]
    public void TestDefaultNullValues()
    {
      var jsonWriter = new Mock<JsonWriter>();

      var converter = new RaptorNullableConverter();

      var nullValues = new List<object>
      {
        sbyte.MaxValue,
        byte.MaxValue,
        short.MaxValue,
        ushort.MaxValue,
        int.MaxValue,
        uint.MaxValue,
        long.MaxValue,
        ulong.MaxValue,
        float.MaxValue,
        DateTime.MinValue
      };

      Assert.True(converter.CanWrite);

      foreach (var nullValue in nullValues)
      {
        var t = nullValue.GetType();
        Assert.True(converter.CanConvert(t), $"Should be able to convert {t}");  
        converter.WriteJson(jsonWriter.Object, nullValue, null);
      }

      jsonWriter.Verify(j => j.WriteNull(), Times.Exactly(nullValues.Count));
    }

    [Fact]
    public void TestNormalValues()
    {
      var jsonWriter = new Mock<JsonWriter>();

      var converter = new RaptorNullableConverter();

      var goodValues = new List<object>
      {
        new sbyte(),
        new byte(),
        new short(),
        new ushort(),
        new int(),
        new uint(),
        new long(),
        new ulong(),
        new float(),
        DateTime.Now,
      };
      Assert.True(converter.CanWrite);

      foreach (var goodValue in goodValues)
      {
        var t = goodValue.GetType();
        Assert.True(converter.CanConvert(t), $"Should be able to convert {t}");  
        converter.WriteJson(jsonWriter.Object, goodValue, null);
      }

      // Confirm it was called the exact amount of times
      jsonWriter.Verify(j => j.WriteValue(It.IsAny<object>()),
        Times.Exactly(goodValues.Count));

      // And confirm the actual data passed was correct too
      foreach (var goodValue in goodValues)
      {
        jsonWriter.Verify(j => j.WriteValue(It.Is<object>(o =>
            o.Equals(goodValue))),
          Times.Once);
      }
    }

    [Fact]
    public void TestOverrideValues()
    {
      var jsonWriter = new Mock<JsonWriter>();

      var defaultValues = new List<Tuple<object, object>>
      {
        new Tuple<object, object>((sbyte)0, (sbyte)1),
        new Tuple<object, object>((byte)0, (byte)1),
        new Tuple<object, object>((short)0, (short)1),
        new Tuple<object, object>((ushort)0, (ushort)1),
        new Tuple<object, object>((int)0, (int)1),
        new Tuple<object, object>((uint)0, (uint)1),
        new Tuple<object, object>((long)0, (long)1),
        new Tuple<object, object>((ulong)0, (ulong)1),
        new Tuple<object, object>((float)0, (float)1),
      };

      foreach (var defaultValue in defaultValues)
      {
        jsonWriter.Reset();
        var t = defaultValue.Item1.GetType();
        var t2 = defaultValue.Item2.GetType();
        Assert.Equal(t, t2);
        var overrideConverter = new RaptorNullableConverter(defaultValue.Item1);

        Assert.True(overrideConverter.CanConvert(t), $"Should be able to convert {t}");
        Assert.True(overrideConverter.CanConvert(t2), $"Should be able to convert {t}");

        overrideConverter.WriteJson(jsonWriter.Object, defaultValue.Item1, null); // the null value
        overrideConverter.WriteJson(jsonWriter.Object, defaultValue.Item2, null); // The non null value

        jsonWriter.Verify(j => j.WriteValue(It.Is<object>(o => o.Equals(defaultValue.Item2))), Times.Once);
        jsonWriter.Verify(j => j.WriteNull() ,Times.Once);

      }
    }
  }
}
