using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Utilities;
using Xunit;

namespace VSS.TRex.Tests.Common
{
  public class TRexExceptionTests
  {
    public static IEnumerable<object[]> GetTypes()
    {
      // Select all TRexException based exceptions
      return TypesHelper.FindAllDerivedTypesInAllLoadedAssemblies<TRexException>().Select(x => new object[] { x });
    }

    [Theory]
    [MemberData(nameof(GetTypes))]
    public void Creation_Message(Type type)
    {
      var ex = Activator.CreateInstance(type, new object[] { "A message"}) as TRexException;

      ex.Should().NotBeNull();
      ex.Message.Should().Be("A message");
    }

    [Theory]
    [MemberData(nameof(GetTypes))]
    public void Creation_MessageWIthException(Type type)
    {
      var ex = Activator.CreateInstance(type, new object[] { "A message", new NotImplementedException("Not Implemented"),  }) as TRexException;

      ex.Should().NotBeNull();
      ex.Message.Should().Be("A message");
      ex.InnerException.Should().NotBeNull();
      ex.InnerException.Message.Should().Be("Not Implemented");
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
