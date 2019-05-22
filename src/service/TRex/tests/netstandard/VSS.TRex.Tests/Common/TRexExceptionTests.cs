using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Exceptions.Exceptions;
using VSS.TRex.Common.Utilities;
using Xunit;

namespace VSS.TRex.Tests.Common
{
  public class TRexExceptionTests
  {
    private const string ERROR_MESSAGE = "A message";
    private const string ACTUAL_TYPE_MAME = "An actual type name";
    private const string EXPECTED_TYPE_MAME = "An expected type name";

    public static IEnumerable<object[]> GetTypes()
    {
      // Select all TRexException based exceptions
      return TypesHelper.FindAllDerivedTypesInAllLoadedAssemblies<TRexException>().Select(x => new object[] { x });
    }

    [Theory]
    [MemberData(nameof(GetTypes))]
    public void Creation_Message(Type type)
    {
      object[] parameters;
      string errorMessage;

      if (type != typeof(TRexClientLeafSubGridTypeCastException) && type != typeof(TRexColorPaletteTypeCastException))
      {
        errorMessage = ERROR_MESSAGE;
        parameters = new object[] { errorMessage };
      }
      else
      {
        var typeName = type == typeof(TRexClientLeafSubGridTypeCastException) ? "ClientLeafSubGrid" : "Palette";
        errorMessage = $"Invalid {typeName} type: {ACTUAL_TYPE_MAME}. Expected type: {EXPECTED_TYPE_MAME}";
        parameters = new object[] { ACTUAL_TYPE_MAME, EXPECTED_TYPE_MAME };
      }
      
      var ex = Activator.CreateInstance(type, parameters) as TRexException;

      ex.Should().NotBeNull();
      ex.Message.Should().Be(errorMessage);
    }

    [Theory]
    [MemberData(nameof(GetTypes))]
    public void Creation_MessageWIthException(Type type)
    {
      var parameters = new List<object>();
      string errorMessage;

      if (type != typeof(TRexClientLeafSubGridTypeCastException) && type != typeof(TRexColorPaletteTypeCastException))
      {
        errorMessage = ERROR_MESSAGE;
        parameters.Add(errorMessage);
      }
      else
      {
        var typeName = type == typeof(TRexClientLeafSubGridTypeCastException) ? "ClientLeafSubGrid" : "Palette";
        errorMessage = $"Invalid {typeName} type: {ACTUAL_TYPE_MAME}. Expected type: {EXPECTED_TYPE_MAME}";
        parameters.Add(ACTUAL_TYPE_MAME);
        parameters.Add(EXPECTED_TYPE_MAME);
      }

      parameters.Add(new NotImplementedException("Not Implemented"));

      var ex = Activator.CreateInstance(type, parameters.ToArray()) as TRexException;
      ex.Message.Should().Be(errorMessage);
      ex.InnerException.Should().NotBeNull();
      ex.InnerException.Message.Should().Be("Not Implemented");

      //var ex = Activator.CreateInstance(type, new object[] { "A message", new NotImplementedException("Not Implemented"),  }) as TRexException;

      //ex.Should().NotBeNull();
      //ex.Message.Should().Be("A message");
      //ex.InnerException.Should().NotBeNull();
      //ex.InnerException.Message.Should().Be("Not Implemented");
    }

    [Fact]
    public void SerialisedVersion_Creation()
    {
      var ex = new TRexSerializationVersionException(1, 2);
      ex.Message.Should().Be("Invalid version read during deserialization: 2, expected version in [1]");

      ex = new TRexSerializationVersionException(new byte[]{1, 2}, 3);
      ex.Message.Should().Be("Invalid version read during deserialization: 3, expected version in [1, 2]");

      ex = new TRexSerializationVersionException(new uint[] { 1, 2 }, 3);
      ex.Message.Should().Be("Invalid version read during deserialization: 3, expected version in [1, 2]");
    }
  }
}
